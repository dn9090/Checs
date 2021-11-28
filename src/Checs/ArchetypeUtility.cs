using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	internal static unsafe class ArchetypeUtility
	{
		public static void ConstructComponentData(Archetype* archetype,
			Span<int> componentTypes, Span<int> componentSizes, int hashCode)
		{
			archetype->entityCount = 0;
			archetype->componentCount = componentSizes.Length; // Temporary fix... see CreateArchetype()
			archetype->componentHashCode = hashCode;

			if(componentSizes.Length == 0)
				return;
			
			archetype->componentTypes = MemoryUtility.Malloc<int>(componentTypes);
			archetype->componentSizes = MemoryUtility.Malloc<int>(componentSizes);
			archetype->componentOffsets = CalculateComponentOffsets(archetype, archetype->chunkCapacity);
		}

		public static int* CalculateComponentOffsets(Archetype* archetype, int blockSize)
		{
			var count = archetype->componentCount;
			var capacity = archetype->chunkCapacity;
			var componentSizes = archetype->componentSizes;

			int* offsets = MemoryUtility.Malloc<int>(count);

			offsets[0] = sizeof(Entity) * capacity;

			// TODO: Optimize with Vector.

			for(int i = 1; i < count; ++i)
				offsets[i] = offsets[i - 1] + (capacity * componentSizes[i - 1]);
			
			return offsets;
		}

		public static int GetTypeIndex(Archetype* archetype, int typeIndex)
		{
			// TODO: Optimize with Vector.

			var types = archetype->componentTypes;
			var count = archetype->componentCount;

			for(int i = 0; i < count; ++i)
			{
				if(types[i] == typeIndex)
					return i;
			}

			return -1;
		}

		public static bool MatchesComponentTypes(Archetype* archetype, Span<int> componentTypes)
		{
			// TODO: Optimize with Vector.
			
			var types = archetype->componentTypes;
			var count = archetype->componentCount;

			for(int i = 0; i < componentTypes.Length; ++i)
			{
				bool matches = false;

				for(int k = 0; k < count; ++k)
				{
					if(types[k] == componentTypes[i])
					{
						matches = true;
						break;
					}
				}
				
				if(!matches)
					return false;
			}

			return true;
		}
	}
}
