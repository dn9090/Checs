using System;
using System.Runtime.InteropServices;
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

		public bool HasComponentData<T>(Entity entity) where T : unmanaged
		{
			// Check destroyed?

			int typeIndex = TypeRegistry<T>.typeIndex;
			return ArchetypeUtility.GetTypeIndex(GetArchetypeInternal(entity), typeIndex) >= 0;			
		}

		public T GetComponentData<T>(Entity entity) where T : unmanaged
		{
			TryGetComponentData<T>(entity, out T value);
			return value;
		}

		public bool TryGetComponentData<T>(Entity entity, out T value) where T : unmanaged
		{
			int typeIndex = TypeRegistry<T>.typeIndex;
			var entityInChunk = this.entityStore->entitiesInChunk[entity.index];
			return ChunkUtility.TryGetComponentData(entityInChunk.chunk, entityInChunk.index, typeIndex, out value);
		}

		// ref T RefComponentData<T>(Entity entity)

		public bool SetComponentData<T>(Entity entity, T value) where T : unmanaged
		{
			int typeIndex = TypeRegistry<T>.typeIndex;
			var entityInChunk = this.entityStore->entitiesInChunk[entity.index];
			return ChunkUtility.SetComponentData<T>(entityInChunk.chunk, entityInChunk.index, typeIndex, value);
		}

		public void AddComponentData<T>(Entity entity) where T : unmanaged
		{
			Archetype* source = GetArchetypeInternal(entity);
			EntityArchetype archetype = AddTypeToArchetype<T>(source);
			Archetype* dest = GetArchetypeInternal(archetype);

			if(source != dest)
				this.entityStore->MoveEntityToArchetype(entity, dest);
		}

		public void DestroyComponentData<T>(Entity entity) where T : unmanaged
		{
			Archetype* source = GetArchetypeInternal(entity);
			EntityArchetype archetype = RemoveTypeFromArchetype<T>(source);
			Archetype* dest = GetArchetypeInternal(archetype);

			if(source != dest)
				this.entityStore->MoveEntityToArchetype(entity, dest);
		}

		// int CopyComponentData<T>(ref Span<T> values)

		// int CopyComponentData<T>(ref Span<Entity> entities, ref Span<T> values)

		// int CopyComponentData<T>(EntityArchetype archetype, ref Span<Entity> entities, ref Span<T> values)
	}
}