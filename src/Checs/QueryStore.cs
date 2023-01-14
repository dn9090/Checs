using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct QueryStore : IDisposable
	{
		public int capacity;

		public int count;

		public Query** queries;

		public QueryStore(int initialCapacity)
		{
			this.count = 0;
			this.capacity = initialCapacity;
			this.queries = (Query**)Allocator.Alloc(sizeof(Query*) * this.capacity);
		}

		public Query* Aquire(int size)
		{
			if(this.count == this.capacity)
			{
				this.capacity = this.capacity * 2;
				this.queries = (Query**)Allocator.Realloc(this.queries, sizeof(Query*) * this.capacity);
			}

			var index = this.count++;

			this.queries[index] = (Query*)Allocator.AlignedAlloc(Query.Size + size, Query.Alignment);
			this.queries[index]->index = index;

			return this.queries[index];
		}
		
		public void Dispose()
		{
			for(int i = 0; i < this.count; ++i)
			{
				this.queries[i]->Dispose();
				Allocator.AlignedFree(this.queries[i]);
			}

			Allocator.Free(this.queries);

			this.queries  = null;
			this.count    = 0;
			this.capacity = 0;
		}
	}
}
