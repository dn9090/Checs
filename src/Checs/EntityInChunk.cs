using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct EntityInChunk
	{
		/// <summary>
		/// TThe chunk in which the entity is stored.
		/// </summary>
		public Chunk* chunk;

		/// <summary>
		/// If the entity exists, the index is the index of the entity in the chunk.
		/// If the entity does not exist, the index points to the next free index.
		/// </summary>
		public int index;

		/// <summary>
		/// The version of the entity. A value of 0 is used as tombstone.
		/// </summary>
		public uint version;

		public EntityInChunk(Chunk* chunk, int index, uint version)
		{
			this.chunk = chunk;
			this.index = index;
			this.version = version;
		}
	}
}