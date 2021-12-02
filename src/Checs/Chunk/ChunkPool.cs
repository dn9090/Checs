using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	internal unsafe static class ChunkPool
	{
		public static ulong nextSequenceNumber; 
		
		public static int rentedCount;

		public static int count;

		public static int capacity;

		public static Chunk** chunks;

		static ChunkPool()
		{
			nextSequenceNumber = 0;
			rentedCount = 0;
			count = 0;
			capacity = 16;
			chunks = MemoryUtility.MallocPtrArray<Chunk>(capacity);
		}

		public static Chunk* Rent(Archetype* archetype)
		{
			Chunk* chunk = count > 0 ? chunks[--count] : ChunkUtility.AllocateChunk();
			ChunkUtility.ConstructChunk(chunk, archetype); // Mh, should this be done here?
			ChunkUtility.AssignSequenceNumber(chunk, nextSequenceNumber++);

			++rentedCount;

			return chunk;		
		}

		public static void Return(Chunk* chunk)
		{
			if(count > rentedCount) // Why I've written this??
				Free(chunk);
			else
				Recycle(chunk);
		}

		public static void Free(Chunk* chunk)
		{
			MemoryUtility.Free<Chunk>(chunk);
			--rentedCount;
		}

		private static void Recycle(Chunk* chunk)
		{
			EnsureCapacity();
			
			chunks[count++] = chunk;
			--rentedCount;
		}

		private static void EnsureCapacity()
		{
			if(count == capacity)
			{
				capacity = capacity * 2;
				chunks = MemoryUtility.ReallocPtrArray(chunks, capacity);
			}	
		}
	}
}