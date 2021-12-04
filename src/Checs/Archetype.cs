using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct Archetype : IDisposable
	{
		public ArchetypeChunkArray* chunkArray; // Place directly in Archetype (should still be smaller than a cache line)?

		public int chunkCapacity;

		public int entityCount;

		public int componentHashCode;

		public int componentCount;

		public int* componentTypes; // Allocate arrays in the same memory block as the archetype struct?

		public int* componentSizes;

		public int* componentOffsets;

		public void Dispose()
		{
			this.chunkArray->Dispose();

			MemoryUtility.Free<ArchetypeChunkArray>(this.chunkArray);

			if(this.componentCount > 0)
			{
				MemoryUtility.Free(this.componentTypes);
				MemoryUtility.Free(this.componentSizes);
				MemoryUtility.Free(this.componentOffsets);
			}
		}
	}
}
