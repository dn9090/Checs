using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct Chunk
	{
		[FieldOffset(0)]
		public Archetype* archetype;

		[FieldOffset(8)]
		public int count;

		[FieldOffset(12)]
		public int capacity;

		[FieldOffset(24)]
		public int index;

		[FieldOffset(28)]
		public int changeVersion;

		// ---------------------------------
		// DO NOT OVERWRITE THE FIELDS BELOW
		// IN OTHER CHUNK TYPES.
		// ---------------------------------

		[FieldOffset(40)]
		public int structuralVersion;

		[FieldOffset(44)]
		public uint sequenceNumber;
		
		[FieldOffset(48)]
		public Chunk* next;
		
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
