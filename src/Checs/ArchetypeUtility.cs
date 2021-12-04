using System;
using System.Runtime.InteropServices;

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

			var size = componentSizes.Length * sizeof(int);

			archetype->componentTypes = (int*)MemoryUtility.Malloc(size);
			archetype->componentSizes = (int*)MemoryUtility.Malloc(size);

			fixed(int* srcTypes = componentTypes, srcSizes = componentSizes)
			{
				Buffer.MemoryCopy(srcTypes, archetype->componentTypes, size, size);
				Buffer.MemoryCopy(srcSizes, archetype->componentSizes, size, size);
			}
		}

		public static void CalculateComponentOffsets(Archetype* archetype, int chunkCapacity)
		{
			var count = archetype->componentCount;
			var componentSizes = archetype->componentSizes;

			int* offsets = MemoryUtility.Malloc<int>(count);

			offsets[0] = sizeof(Entity) * chunkCapacity;

			for(int i = 1; i < count; ++i)
				offsets[i] = offsets[i - 1] + (chunkCapacity * componentSizes[i - 1]);
			
			archetype->componentOffsets = offsets;
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
	}
}
