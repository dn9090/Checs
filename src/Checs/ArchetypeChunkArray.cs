using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	// TODO: Looking at it again, this thing needs a lot of work.
	// Maybe move the methods to the ArchetypeUtility and
	// also add API's for requesting multiple chunks.

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct ArchetypeChunkArray : IDisposable
	{
		public Archetype* archetype;

		public int count;

		public int capacity;

		public Chunk** chunks;

		public static ArchetypeChunkArray* Allocate(Archetype* archetype)
		{
			ArchetypeChunkArray* array = MemoryUtility.Malloc<ArchetypeChunkArray>();
			array->archetype = archetype;
			array->count = 0;
			array->capacity = 2;
			array->chunks = MemoryUtility.MallocPtrArray<Chunk>(array->capacity);

			return array;
		}

		public Chunk* CreateChunk()
		{
			EnsureCapacity();

			this.chunks[count] = ChunkPool.Rent(this.archetype);
			return this.chunks[count++];
		}

		public void ReleaseChunk(int index)
		{
			Chunk* chunk = chunks[index];
			chunks[index] = chunks[--count];
		
			ChunkPool.Return(chunk);
		}

		// TODO: What if the chunk array gets sorted for calls like GetChunkWithEmptySlots?
		// The chunks with the most empty slots are at Length - 1 (or 0).
		// Now a simple if((chunks[i].capacity - chunks[i]) > 0) with --i can solve a lot
		// of things more efficently.

		public Chunk* GetChunkWithEmptySlots(ref int index)
		{
			for(int i = index; i < this.count; ++i)
			{
				if(this.chunks[i]->count < this.chunks[i]->capacity)
				{
					index = i;
					return this.chunks[i];
				}
			}

			index = count;

			return CreateChunk();
		}

		public Chunk* GetChunkWithEmptySlots(int count)
		{
			// Less efficent than GetChunkWithEmptySlots(ref int index) for small counts,
			// but it actually tries to store the entities together in one chunk.

			var indexWithMostEmptySlots = 0;
			var emptySlots = 0;

			for(int i = 0; i < this.count; ++i)
			{
				var emptyChunkSlots = this.chunks[i]->capacity - this.chunks[i]->count;

				if(emptyChunkSlots >= count)
					return this.chunks[i];

				if(emptyChunkSlots >= emptySlots)
				{
					emptySlots = emptyChunkSlots;
					indexWithMostEmptySlots = i;
				}
			}
			
			if(emptySlots > 0)
				return this.chunks[indexWithMostEmptySlots];

			return CreateChunk();
		}

		public void EnsureCapacity()
		{
			if(this.count == this.capacity)
			{
				this.capacity = this.capacity * 2;
				this.chunks = MemoryUtility.ReallocPtrArray(this.chunks, this.capacity);
			}
		}

		public void Dispose()
		{
			for(int i = 0; i < this.count; ++i)
				ChunkPool.Free(this.chunks[i]);
			
			MemoryUtility.Free(this.chunks);
		}
	}
}