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
			int typeIndex = TypeRegistry.emptyTypeIndex; // Check if there is a better way.
			return CreateArchetypeInternal(new Span<int>(&typeIndex, 1), Span<int>.Empty, 0);
		}

		public EntityArchetype CreateArchetype<T>() where T : unmanaged
		{
			int typeIndex = TypeRegistry<T>.typeIndex;
			int size = sizeof(T);
			return CreateArchetypeInternal(new Span<int>(&typeIndex, 1), new Span<int>(&size, 1), size);
		}

		public EntityArchetype CreateArchetype(Type type)
		{
			int typeIndex = TypeRegistry.ToTypeIndex(type);
			int size = Marshal.SizeOf(type);
			return CreateArchetypeInternal(new Span<int>(&typeIndex, 1), new Span<int>(&size, 1), size);
		}

		public EntityArchetype CreateArchetype(ReadOnlySpan<Type> types)
		{
			if(types.Length == 0)
				return CreateArchetype();
			
			// The allocation > 16 is not cool. Is it possible to avoid this?
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

			SortUtility.Sort(componentTypes, componentSizes);

			return CreateArchetypeInternal(componentTypes, componentSizes, blockSize);
		}

		internal EntityArchetype CreateArchetypeInternal(Span<int> componentTypes,
			Span<int> componentSizes, int absoluteBlockSize)
		{
			int hashCode = GetComponentTypeHash(componentTypes);

			if(this.archetypeStore->typeLookup.TryGet(hashCode, out EntityArchetype entityArchetype))
				return entityArchetype;

			int chunkCapacity = ChunkUtility.CalculateChunkBufferCapacity(absoluteBlockSize);

			if(chunkCapacity == 0)
				throw new ArchetypeTooLargeException(absoluteBlockSize);

			this.archetypeStore->EnsureCapacity();

			int index = this.archetypeStore->count++;

			Archetype* archetype = this.archetypeStore->archetypes + index;

			// TODO: Move more stuff in the utility or avoid the utility. YOU DECIDE!
			ArchetypeUtility.ConstructComponentData(archetype, componentTypes, componentSizes, hashCode);
			ArchetypeUtility.CalculateComponentOffsets(archetype, chunkCapacity);

			archetype->chunkArray = ArchetypeChunkArray.Allocate(archetype);
			archetype->chunkCapacity = chunkCapacity;

			entityArchetype = new EntityArchetype(index);

			// Move to seperate method that takes Archetype* as argument?
			this.archetypeStore->typeLookup.Add(hashCode, entityArchetype);

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
			// TODO: Needs a check if the archetypes contains typeof(T).

			var srcCount = archetype->componentCount;

			if(srcCount == 0)
				return CreateArchetype<T>();

			int destCount = srcCount + 1;
			Span<int> componentData = destCount > 16
				? new int[destCount * 2]
				: stackalloc int[destCount * 2];

			var destTypes = componentData.Slice(0, destCount);
			var destSizes = componentData.Slice(destCount);

			var sourceTypes = new Span<int>(archetype->componentTypes, srcCount);
			var sourceSizes = new Span<int>(archetype->componentSizes, srcCount);

			sourceTypes.CopyTo(destTypes);
			sourceSizes.CopyTo(destSizes);

			destTypes[srcCount] = TypeRegistry<T>.typeIndex;
			destSizes[srcCount] = sizeof(T);

			// Sometimes the added type is a new type, therefore the typeIndex is
			// higher than the indices in the array. In this case skip the sorting.
			if(destTypes[srcCount] < destTypes[srcCount - 1])
				SortUtility.Sort(destTypes, destSizes);

			int blockSize = 0;

			for(int i = 0; i < destSizes.Length; ++i) // TODO: Use CalculateBlockSize instead.
				blockSize += destSizes[i];

			return CreateArchetypeInternal(destTypes, destSizes, blockSize);
		}

		internal EntityArchetype RemoveTypeFromArchetype<T>(Archetype* archetype) where T : unmanaged
		{
			// TODO: Needs a check if the archetypes contains typeof(T).

			int srcCount = archetype->componentCount;
			
			if(srcCount <= 1)
				return CreateArchetype();

			int destCount = srcCount - 1;
			Span<int> componentData = destCount > 16
				? new int[destCount * 2]
				: stackalloc int[destCount * 2];

			var destTypes = componentData.Slice(0, destCount);
			var destSizes = componentData.Slice(destCount);

			var sourceTypes = new Span<int>(archetype->componentTypes, srcCount);
			var sourceSizes = new Span<int>(archetype->componentSizes, srcCount);

			int indexInArchetype = ArchetypeUtility.GetTypeIndex(archetype, TypeRegistry<T>.typeIndex);

			// Is it somehow possible to remove the branches here?
			
			if(indexInArchetype > 0)
			{
				sourceTypes.Slice(0, indexInArchetype).CopyTo(destTypes);
				sourceSizes.Slice(0, indexInArchetype).CopyTo(destSizes);
			}
			
			if(indexInArchetype < destCount)
			{
				sourceTypes.Slice(indexInArchetype + 1).CopyTo(destTypes.Slice(indexInArchetype));
				sourceSizes.Slice(indexInArchetype + 1).CopyTo(destSizes.Slice(indexInArchetype));
			}

			int blockSize = 0;

			for(int i = 0; i < destSizes.Length; ++i) // TODO: Use CalculateBlockSize instead.
				blockSize += destSizes[i];

			return CreateArchetypeInternal(destTypes, destSizes, blockSize);
		}
	}
}