using System;
using System.Runtime.InteropServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct ArchetypeList : IDisposable
	{
		public const int InitialCapacity = 8;

		public int capacity;

		public int count;

		public Archetype** archetypes;

		public void Add(Archetype* archetype)
		{
			if(this.count == this.capacity)
				Resize();

			this.archetypes[this.count++] = archetype;
		}

		public void Resize()
		{
			this.capacity = this.capacity == 0 ? InitialCapacity : this.capacity * 2;
			this.archetypes = (Archetype**)Allocator.Realloc(this.archetypes, sizeof(Archetype*) * this.capacity);
		}

		public void Dispose()
		{
			Allocator.Free(this.archetypes);
		}
	}
}
