using System;
using System.Runtime.InteropServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct ChunkList : IDisposable
	{
		public const int InitialCapacity = 8;

		public int capacity;

		public int count;

		public Chunk** chunks;

		public void Add(Chunk* chunk)
		{
			if(this.count == this.capacity)
				Resize();

			chunk->index = this.count;
			this.chunks[this.count++] = chunk;
		}

		public void Remove(Chunk* chunk)
		{
			this.chunks[chunk->index] = this.chunks[--this.count];
			this.chunks[chunk->index]->index = chunk->index;
		}

		public void Resize()
		{
			this.capacity = this.capacity == 0 ? InitialCapacity : this.capacity * 2;
			this.chunks = (Chunk**)Allocator.Realloc(this.chunks, sizeof(Chunk*) * this.capacity);
		}

		public void Dispose()
		{
			Allocator.Free(this.chunks);

			this.chunks   = null;
			this.capacity = 0;
			this.count    = 0;
		}
	}
}
