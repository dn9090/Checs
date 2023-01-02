using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct ArchetypeStore : IDisposable
	{
		public int count;

		public int capacity;

		public Archetype** archetypes;

		public ArchetypeStore(int initialCapacity)
		{
			this.count = 0;
			this.capacity = initialCapacity;
			this.archetypes = (Archetype**)Allocator.Alloc(sizeof(Archetype*) * this.capacity);
		}

		public Archetype* Aquire(int bufferSize)
		{
			if(this.count == this.capacity)
			{
				this.capacity = this.capacity * 2;
				this.archetypes = (Archetype**)Allocator.Realloc(this.archetypes, sizeof(Archetype*) * this.capacity);
			}

			var index = this.count++;

			this.archetypes[index] = (Archetype*)Allocator.AlignedAlloc(Archetype.Size + bufferSize, Archetype.Alignment);
			this.archetypes[index]->index = index;

			return this.archetypes[index];
		}

		public void Dispose()
		{			
			for(int i = 0; i < this.count; ++i)
			{
				this.archetypes[i]->Dispose();
				Allocator.AlignedFree(this.archetypes[i]);
			}

			Allocator.Free(this.archetypes);
		}
	}
}
