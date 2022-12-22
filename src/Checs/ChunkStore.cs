using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	internal unsafe struct ChunkStore : IDisposable
	{
		public IntPtr head;

		public IntPtr free;
		
		public Chunk* Aquire()
		{
			Chunk* chunk = null;

			do
			{
				chunk = (Chunk*)this.free;

				// If another thread releases a chunk at this moment
				// we simply don't care and allocate a new one.
				if(chunk == null)
					return Allocate();
					
			} while(chunk != (Chunk*)Interlocked.CompareExchange(ref this.free, (IntPtr)chunk->free, (IntPtr)chunk));

			return chunk;
		}

		public Chunk* Allocate()
		{
			var chunk = (Chunk*)Allocator.AlignedAlloc(Chunk.ChunkSize, Chunk.Alignment);

			chunk->free = null;
			chunk->sequenceNumber = 0;
			
			do
			{
				chunk->next = (Chunk*)this.head;
			} while(chunk->next != (Chunk*)Interlocked.CompareExchange(ref this.head, (IntPtr)chunk, (IntPtr)chunk->next));

			return chunk;
		}

		public void Release(Chunk* chunk)
		{
			++chunk->sequenceNumber;

			do
			{
				chunk->free = (Chunk*)this.free;
			} while(chunk->free != (Chunk*)Interlocked.CompareExchange(ref this.free, (IntPtr)chunk, (IntPtr)chunk->free));
		}

		public int GetCount()
		{
			var count = 0;
			var next = (Chunk*)this.head;

			while(next != null)
			{
				next = next->next;
				++count;
			}

			return count;
		}

		public void Dispose()
		{
			var next = (Chunk*)this.head;

			while(next != null)
			{
				var chunk = next;
				next = next->next;

				Allocator.AlignedFree(chunk);
			}
		}
	}
}