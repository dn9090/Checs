using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	public unsafe partial class EntityManager
	{
		internal Chunk* GetOrConstructChunk(Archetype* archetype, int requestedCapacity, ref int startIndex)
		{
			if(requestedCapacity < archetype->chunkCapacity)
			{
				var chunkList = &archetype->chunkList;

				for(int i = startIndex; i < chunkList->count; ++i)
				{
					if(chunkList->chunks[i]->count < chunkList->chunks[i]->capacity)
					{
						startIndex = i + 1;
						return chunkList->chunks[i];
					}
				}
			}

			return ConstructAndAppendChunk(archetype);
		}

		internal Chunk* GetOrConstructChunkWithEmptySlots(Archetype* archetype, ref int indexInList)
		{
			var chunkList = &archetype->chunkList;

			for(int i = indexInList; i < chunkList->count; ++i)
			{
				if(chunkList->chunks[i]->count < chunkList->chunks[i]->capacity)
				{
					indexInList = i;
					return chunkList->chunks[i];
				}
			}
			
			return ConstructAndAppendChunk(archetype);
		}

		internal Chunk* ConstructAndAppendChunk(Archetype* archetype)
		{
			Chunk* chunk = this.chunkStore.Aquire();

			ChunkUtility.ConstructChunk(chunk, archetype);
			
			archetype->chunkList.Add(chunk);

			return chunk;
		}

		internal void ReleaseChunkIfEmpty(Chunk* chunk)
		{
			if(chunk->count == 0)
			{
				chunk->archetype->chunkList.Remove(chunk);
				this.chunkStore.Release(chunk);
			}

			// TODO: Dispose Buffers
		}
	}
}