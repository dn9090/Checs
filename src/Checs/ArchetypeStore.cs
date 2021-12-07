using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct ArchetypeStore : IDisposable
	{
		public int count;

		public int capacity;

		public HashMap<EntityArchetype> typeLookup;

		public Archetype* archetypes;

		public static void Construct(ArchetypeStore* store)
		{
			store->typeLookup = HashMap<EntityArchetype>.Empty();
			store->count = 0;
			store->capacity = store->typeLookup.capacity;
			store->archetypes = MemoryUtility.Malloc<Archetype>(store->capacity);
		}

		public void EnsureCapacity()
		{
			if(this.count == this.capacity)
			{
				this.capacity = this.capacity * 2;
				this.archetypes = MemoryUtility.Realloc(archetypes, this.capacity);
			}
		}

		public int GetChunkCount()
		{
			int count = 0;

			for(int i = 0; i < this.count; ++i)
				count += archetypes[i].chunkArray->count;
			
			return count;
		}

		public int GetArchetypeIndex(Archetype* archetype) => (int)(archetype - this.archetypes);

		public void Dispose()
		{
			for(int i = 0; i < this.count; ++i)
				this.archetypes[i].Dispose();

			this.count = 0;
			this.capacity = 0;
			this.typeLookup.Dispose();
			MemoryUtility.Free<Archetype>(this.archetypes);
		}
	}
}
