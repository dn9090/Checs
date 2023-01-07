using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct Chunk
	{
		/// <summary>
		/// The archetype the chunk belongs to.
		/// </summary>
		[FieldOffset(0)]
		public Archetype* archetype;

		/// <summary>
		/// The number of entities in the chunk.
		/// </summary>
		[FieldOffset(8)]
		public int count;

		/// <summary>
		/// The maximum number of entities in the chunk.
		/// </summary>
		[FieldOffset(12)]
		public int capacity;

		/// <summary>
		/// The index of the chunk in the archetype.
		/// </summary>
		[FieldOffset(24)]
		public int index;

		[FieldOffset(28)]
		public uint changeVersion;

		/// <summary>
		/// The version of the chunk. The version is incremented when
		/// entities are added to the chunk or removed from the chunk.
		/// </summary>
		/// <remarks>
		/// Do not overwrite this field in other chunk types.
		/// </remarks>
		[FieldOffset(40)]
		public uint version;
		
		/// <summary>
		/// The number of times the chunk was recycled.
		/// </summary>
		/// <remarks>
		/// Do not overwrite this field in other chunk types.
		/// </remarks>
		[FieldOffset(44)]
		public uint sequenceNumber;
		
		/// <summary>
		/// Points to the next chunk in a linked list of all
		/// allocated chunks.
		/// </summary>
		/// <remarks>
		/// Do not overwrite this field in other chunk types.
		/// </remarks>
		[FieldOffset(48)]
		public Chunk* next;
		
		/// <summary>
		/// Points to the next chunk in a list of chunks that
		/// can be recycled.
		/// </summary>
		/// <remarks>
		/// Do not overwrite this field in other chunk types.
		/// </remarks>
		[FieldOffset(56)]
		public Chunk* free;

		[FieldOffset(64)]
		public fixed byte buffer[4];

		public const int ChunkSize = 16 * 1024;

		public const int HeaderSize = 64;

		public const int BufferSize = ChunkSize - HeaderSize;

		public const int Alignment = 64;
	}
}
