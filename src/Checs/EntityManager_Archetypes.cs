using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	// TODO: Block structural changes in query.

	public unsafe partial class EntityManager
	{
		public EntityArchetype CreateArchetype()
		{
			Span<int> componentTypes = stackalloc int[] { TypeRegistry.emptyTypeIndex };
			return CreateArchetypeInternal(componentTypes, Span<int>.Empty, 0);
		}

		public EntityArchetype CreateArchetype(Type type)
		{
			Span<int> componentTypes = stackalloc int[] { TypeRegistry.ToTypeIndex(type) };
			Span<int> componentSizes = stackalloc int[] { Marshal.SizeOf(type) };
			return CreateArchetypeInternal(componentTypes, componentSizes, componentSizes[0]);	
		}

		public EntityArchetype CreateArchetype(Span<Type> types)
		{
			if(types.Length == 0)
				return CreateArchetype();
			
			Span<int> componentData = types.Length > 16
				? new int[types.Length * 2]
				: stackalloc int[types.Length * 2];
			
			Span<int> componentTypes = componentData.Slice(0, types.Length);
			Span<int> componentSizes = componentData.Slice(types.Length);
			int blockSize = 0;

			for(int i = 0; i < types.Length; ++i)
			{
				componentTypes[i] = TypeRegistry.ToTypeIndex(types[i]);
				componentSizes[i] = Marshal.SizeOf(types[i]);
				blockSize += componentSizes[i];
			}

			SortComponentData(componentTypes, componentSizes);

			return CreateArchetypeInternal(componentTypes, componentSizes, blockSize);
		}

		internal EntityArchetype CreateArchetypeInternal(Span<int> componentTypes,
			Span<int> componentSizes, int absoluteBlockSize)
		{
			int hashCode = 0;
			for(int i = 0; i < componentTypes.Length; ++i)
				hashCode = HashCode.Combine(hashCode, componentTypes[i]);
			
			if(this.archetypeStore->typeLookup.TryGet(hashCode, out EntityArchetype entityArchetype))
				return entityArchetype;

			this.archetypeStore->EnsureCapacity();
				
			int chunkCapacity = ChunkUtility.CalculateChunkBufferCapacity(absoluteBlockSize);

			if(chunkCapacity == 0)
				throw new ArchetypeTooLargeException(absoluteBlockSize);

			int index = this.archetypeStore->count++;

			Archetype archetype = new Archetype();
			archetype.chunkArray = ArchetypeChunkArray.Allocate(&this.archetypeStore->archetypes[index]);
			archetype.chunkCapacity = chunkCapacity;
			archetype.entityCount = 0;

			ArchetypeUtility.ConstructComponentData(&archetype, componentTypes, componentSizes, hashCode);

			entityArchetype = new EntityArchetype(index);

			this.archetypeStore->archetypes[index] = archetype;
			this.archetypeStore->typeLookup.Add(archetype.componentHashCode, entityArchetype);

			return entityArchetype;
		}

		internal EntityArchetype CreateArchetypeInternal(Archetype* archetype) =>
			 this.archetypeStore->typeLookup.Get(archetype->componentHashCode);

		public EntityArchetype GetArchetype(Entity entity) =>
			CreateArchetypeInternal(GetArchetypeInternal(entity));
		
		internal Archetype* GetArchetypeInternal(Entity entity) =>
			this.entityStore->GetChunk(entity)->archetype;

		internal Archetype* GetArchetypeInternal(EntityArchetype archetype) =>
			&this.archetypeStore->archetypes[archetype.index];

		internal EntityArchetype AddTypeToArchetype<T>(Archetype* archetype) where T : unmanaged
		{
			var count = archetype->componentCount;

			if(count == 0)
				return CreateArchetype(typeof(T));

			int componentCount = count + 1;
			Span<int> componentData = componentCount > 16
				? new int[componentCount * 2]
				: stackalloc int[componentCount * 2];

			var componentTypes = componentData.Slice(0, componentCount);
			var componentSizes = componentData.Slice(componentCount);

			var sourceTypes = new Span<int>(archetype->componentTypes, count);
			var sourceSizes = new Span<int>(archetype->componentSizes, count);

			sourceTypes.CopyTo(componentTypes);
			sourceSizes.CopyTo(componentSizes);

			componentTypes[count] = TypeRegistry<T>.typeIndex;
			componentSizes[count] = sizeof(T);

			SortComponentData(componentTypes, componentSizes);

			int blockSize = 0;

			for(int i = 0; i < componentSizes.Length; ++i)
				blockSize += componentSizes[i];

			return CreateArchetypeInternal(componentTypes, componentSizes, blockSize);
		}

		internal EntityArchetype RemoveTypeFromArchetype<T>(Archetype* archetype) where T : unmanaged
		{
			int count = archetype->componentCount;
			
			if(count <= 1)
				return CreateArchetype();

			int componentCount = count - 1;
			Span<int> componentData = componentCount > 16
				? new int[componentCount * 2]
				: stackalloc int[componentCount * 2];

			var componentTypes = componentData.Slice(0, componentCount);
			var componentSizes = componentData.Slice(componentCount);

			var sourceTypes = new Span<int>(archetype->componentTypes, count);
			var sourceSizes = new Span<int>(archetype->componentSizes, count);

			int index = ArchetypeUtility.GetTypeIndex(archetype, TypeRegistry<T>.typeIndex);

			// REVIEW: Is it somehow possible to remove the branches here?
			
			if(index > 0)
			{
				sourceTypes.Slice(0, index).CopyTo(componentTypes);
				sourceSizes.Slice(0, index).CopyTo(componentSizes);
			}
			
			if(index < componentCount)
			{
				sourceTypes.Slice(index + 1).CopyTo(componentTypes.Slice(index));
				sourceSizes.Slice(index + 1).CopyTo(componentSizes.Slice(index));
			}

			int blockSize = 0;

			for(int i = 0; i < componentSizes.Length; ++i)
				blockSize += componentSizes[i];

			return CreateArchetypeInternal(componentTypes, componentSizes, blockSize);
		}
	}
}