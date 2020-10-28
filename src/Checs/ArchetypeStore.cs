using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	internal unsafe struct ArchetypeStore : IDisposable
	{
		public int count;

		public int capacity;

		public HashMap<EntityArchetype> typeLookup;

		public Archetype* archetypes;

		public static ArchetypeStore Empty()
		{
			ArchetypeStore store = new ArchetypeStore();
			store.typeLookup = HashMap<EntityArchetype>.Empty();
			store.count = 0;
			store.capacity = store.typeLookup.capacity;
			store.archetypes = MemoryUtility.Malloc<Archetype>(store.capacity);

			return store;
		}

		public static void Construct(void* ptr) => Construct((Archetype*)ptr);

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

		public void Dispose()
		{
			this.count = 0;
			this.typeLookup.Dispose();
			this.archetypes = null;
		}
	}
}
