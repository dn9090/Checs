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

		[FieldOffset(16)]
		public ulong sequenceNumber;

		[FieldOffset(24)]
		public ulong commitVersion;

		[FieldOffset(64)]
		public fixed byte buffer[4];

		public const int ChunkSize = 16 * 1024;

		public const int BufferSize = ChunkSize - 64;
	}
}
