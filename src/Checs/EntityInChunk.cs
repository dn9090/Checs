using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct EntityInChunk
	{
		public Chunk* chunk;
	
		public int index; // we dont care if the entity is destroyed, so reuse that as next value

		public uint version;

		public EntityInChunk(Chunk* chunk, int index, uint version)
		{
			this.chunk = chunk;
			this.index = index;
			this.version = version;
		}
	}
}