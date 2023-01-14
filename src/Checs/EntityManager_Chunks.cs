using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics;

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

			Debug.Assert(chunk != null);

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

			// TODO: Merge chunks
			//       var c1 = chunk->count < (archetype->chunkCapacity / 2);
			//       var c2 = chunk2->count < (archetype->chunkCapacity / 2);
			//       if(c1 && c2) ChunkUtility.MergeChunks(c1, c2);
		}
	}
}