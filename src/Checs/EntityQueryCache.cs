using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct EntityQueryCache : IDisposable
	{
		public int count;

		public int capacity;

		public HashMap<EntityQuery> typeLookup;

		public EntityQueryData* queries;

		public static void Construct(EntityQueryCache* cache)
		{
			cache->typeLookup = HashMap<EntityQuery>.Empty();
			cache->count = 0;
			cache->capacity = cache->typeLookup.capacity;
			cache->queries = MemoryUtility.Malloc<EntityQueryData>(cache->capacity);
		}

		public void EnsureCapacity()
		{
			if(this.count == this.capacity)
			{
				this.capacity = this.capacity * 2;
				this.queries = MemoryUtility.Realloc(queries, this.capacity);
			}
		}

		public void Dispose()
		{
			this.count = 0;
			this.capacity = 0;
			this.typeLookup.Dispose();
			MemoryUtility.Free<EntityQueryData>(this.queries);
		}
	}
}