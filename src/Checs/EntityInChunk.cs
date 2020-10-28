using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe readonly struct EntityInChunk
	{
		public readonly Chunk* chunk;
	
		public readonly int index;

		public EntityInChunk(Chunk* chunk, int index)
		{
			this.chunk = chunk;
			this.index = index;
		}
	}
}