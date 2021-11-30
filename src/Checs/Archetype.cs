using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct Archetype
	{
		public ArchetypeChunkArray* chunkArray;

		public int chunkCapacity;

		public int entityCount;

		public int componentHashCode;

		public int componentCount;

		public int* componentTypes; // Allocate arrays in the same memory block as the archetype struct?

		public int* componentSizes;

		public int* componentOffsets;
	}
}
