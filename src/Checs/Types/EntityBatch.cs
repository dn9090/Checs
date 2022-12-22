using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	internal readonly unsafe ref struct EntityBatch
	{
		public readonly Chunk* chunk;

		public readonly int index;

		public readonly int count;

		public EntityBatch(Chunk* chunk)
		{
			this.chunk = chunk;
			this.index = 0;
			this.count = chunk->count;
		}

		public EntityBatch(Chunk* chunk, int index, int count)
		{
			this.chunk = chunk;
			this.index = index;
			this.count = count;
		}

		public EntityBatch(EntityInChunk entityInChunk)
		{
			this.chunk = entityInChunk.chunk;
			this.index = entityInChunk.index;
			this.count = 1;
		}
	}
}