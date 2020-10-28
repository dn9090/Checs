using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	internal unsafe struct ChunkIterator : IDisposable
	{
		public int capacity;

		public int count;

		public int index;

		public Chunk** chunks;

		public ChunkIterator(int capacity)
		{
			this.capacity = capacity;
			this.count = 0;
			this.index = 0;
			this.chunks = MemoryUtility.MallocPtrArray<Chunk>(this.capacity);
		}

		public void Add(Archetype* archetype)
		{
			var chunkArray = archetype->chunkArray;
			Add(chunkArray->chunks, chunkArray->count);
		}

		public void Add(Chunk** chunks, int count)
		{
			EnsureCapacity(count);
			Unsafe.CopyBlock((void*)&this.chunks[this.count], (void*)chunks, (uint)(sizeof(Chunk*) * count));
		}

		public Chunk* Next()
		{
			int next = Interlocked.Increment(ref index);

			if((uint)this.index < (uint)this.count)
				return this.chunks[next];

			return null;
		}

		public void Dispose()
		{
		}

		private void EnsureCapacity(int count)
		{
			int requiredCapacity = this.count + count;

			if(requiredCapacity > this.capacity)
			{
				this.capacity = MemoryUtility.RoundToPowerOfTwo(requiredCapacity);
				this.chunks = MemoryUtility.ReallocPtrArray<Chunk>(this.chunks, this.capacity);
			}
		}
	}
}