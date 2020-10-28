using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe readonly struct EntityBatchInChunk
	{
		public readonly Chunk* chunk;
		
		public readonly int index;

		public readonly int count;

		public EntityBatchInChunk(Chunk* chunk, int index, int count)
		{
			this.chunk = chunk;
			this.index = index;
			this.count = count;
		}
	}
}