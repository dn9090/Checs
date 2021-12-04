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