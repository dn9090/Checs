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
			ChunkUtility.ConstructChunk(chunk, archetype);
			ChunkUtility.AssignSequenceNumber(chunk, nextSequenceNumber++);

			rentedCount += 1;

			return chunk;		
		}

		public static void Return(Chunk* chunk)
		{
			if(count > rentedCount)
				Free(chunk);
			else
				RecycleChunk(chunk);
		}

		public static void Free(Chunk* chunk)
		{
			MemoryUtility.Free<Chunk>(chunk);
			rentedCount -= 1;
		}

		private static void RecycleChunk(Chunk* chunk)
		{
			EnsureCapacity();
			
			chunks[count++] = chunk;
			rentedCount -= 1;
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