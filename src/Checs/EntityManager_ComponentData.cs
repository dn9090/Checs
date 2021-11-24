using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Checs
{
	// REVIEW: Consider removing the unsafe context in
	//         safe methods (because it looks ugly).

	// TODO: Block structural changes in query.

	public unsafe partial class EntityManager
	{
		internal static void SortComponentData(Span<int> componentTypes, Span<int> componentSizes)
		{
			for(int i = 1; i < componentTypes.Length; ++i)
			{
				var type = componentTypes[i];
				var size = componentSizes[i];
				var index = i;

				for(; index > 0 && type.CompareTo(componentTypes[index - 1]) <= 0; --index)
				{
					componentTypes[index] = componentTypes[index - 1];
					componentSizes[index] = componentSizes[index - 1];
				}
					
				componentTypes[index] = type;
				componentSizes[index] = size;
			}
		}

		public bool HasComponentData<T>(Entity entity) where T : unmanaged, IComponentData
		{
			if(!IsAlive(entity))
				return false;

			int typeIndex = TypeRegistry<T>.typeIndex;
			return ArchetypeUtility.GetTypeIndex(GetArchetypeInternal(entity), typeIndex) >= 0;			
		}

		public T GetComponentData<T>(Entity entity) where T : unmanaged, IComponentData
		{
			TryGetComponentData<T>(entity, out T value);
			return value;
		}

		public bool TryGetComponentData<T>(Entity entity, out T value) where T : unmanaged, IComponentData
		{
			if(!IsAlive(entity))
			{
				value = default;
				return false;
			}

			int typeIndex = TypeRegistry<T>.typeIndex;
			var entityInChunk = this.entityStore->entitiesInChunk[entity.index];
			return ChunkUtility.TryGetComponentData(entityInChunk.chunk, entityInChunk.index, typeIndex, out value);
		}

		public ref T RefComponentData<T>(Entity entity) where T : unmanaged, IComponentData
		{
			if(!IsAlive(entity))
				throw new InvalidOperationException("Cannot reference component data of a dead entity.");

			int typeIndex = TypeRegistry<T>.typeIndex;
			var entityInChunk = this.entityStore->entitiesInChunk[entity.index];

			var ptr = ChunkUtility.GetComponentDataPtr<T>(entityInChunk.chunk, entityInChunk.index, typeIndex);

			if(ptr == null)
				throw new InvalidOperationException("Cannot reference non existent component data.");

			return ref Unsafe.AsRef<T>(ptr); 
		}

		public bool SetComponentData<T>(Entity entity, T value) where T : unmanaged, IComponentData
		{
			if(!IsAlive(entity))
				return false;

			int typeIndex = TypeRegistry<T>.typeIndex;
			var entityInChunk = this.entityStore->entitiesInChunk[entity.index];
			return ChunkUtility.SetComponentData<T>(entityInChunk.chunk, entityInChunk.index, typeIndex, value);
		}

		public bool SetComponentData<T>(Entity entity, in T value) where T : unmanaged, IComponentData
		{
			throw new NotImplementedException();
		}

		public bool AddComponentData<T>(Entity entity) where T : unmanaged, IComponentData
		{
			if(!IsAlive(entity))
				return false;

			Archetype* source = GetArchetypeInternal(entity);
			EntityArchetype archetype = AddTypeToArchetype<T>(source);
			Archetype* dest = GetArchetypeInternal(archetype);

			if(source != dest)
				this.entityStore->MoveEntityToArchetype(entity, dest);
				
			return true;
		}

		public void DestroyComponentData<T>(Entity entity) where T : unmanaged, IComponentData
		{
			if(!IsAlive(entity))
				return;

			Archetype* source = GetArchetypeInternal(entity);
			EntityArchetype archetype = RemoveTypeFromArchetype<T>(source);
			Archetype* dest = GetArchetypeInternal(archetype);

			if(source != dest)
				this.entityStore->MoveEntityToArchetype(entity, dest);
		}

		public int CopyComponentData<T>(EntityArchetype archetype, Span<T> destination) where T : unmanaged, IComponentData
			=> CopyComponentDataInternal(GetArchetypeInternal(archetype), destination, TypeRegistry<T>.typeIndex);

		public int CopyComponentData<T>(EntityQuery query, Span<T> destination) where T : unmanaged, IComponentData
		{
			var queryData = GetUpdatedQueryData(query);
			var typeIndex = TypeRegistry<T>.typeIndex;
			var count = 0;

			for(int i = 0; i < queryData->archetypeCount && count < destination.Length; ++i)
				count += CopyComponentDataInternal(queryData->archetypes[i], destination.Slice(count), typeIndex);

			return count;
		}

		internal int CopyComponentDataInternal<T>(Archetype* archetype, Span<T> destination, int typeIndex)  where T : unmanaged
		{
			var chunkCount = archetype->chunkArray->count;
			var chunks = archetype->chunkArray->chunks;
			var count = 0;

			fixed(T* dest = destination)
			{
				for(int i = 0; i < chunkCount && count < destination.Length; ++i)
				{
					var indexInArchetype = ArchetypeUtility.GetTypeIndex(chunks[i]->archetype, typeIndex);
					var bufferCount = destination.Length - count;
					var max = bufferCount < chunks[i]->count ? bufferCount : chunks[i]->count;
					var size = max * sizeof(T);

					Buffer.MemoryCopy(ChunkUtility.GetComponentPtrInBuffer<T>(chunks[i], indexInArchetype), dest + count, size, size);

					count += max;
				}
			}

			return count;
		}

		internal int CopyComponentDataInternal2<T>(Archetype* archetype, Span<T> destination, int typeIndex)  where T : unmanaged
		{
			var chunkCount = archetype->chunkArray->count;
			var chunks = archetype->chunkArray->chunks;
			var count = 0;

			for(int i = 0; i < chunkCount && count < destination.Length; ++i) // This needs some work and cleanup.
			{
				var slice = destination.Slice(count);
				var data = ChunkUtility.GetComponentData<T>(chunks[i], typeIndex);
				var max = slice.Length < data.Length ? slice.Length : data.Length;
				data.Slice(0, max).CopyTo(slice);
				count += max;
			}

			return count;
		}
	}
}