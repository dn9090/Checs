using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Checs
{
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct Archetype : IDisposable
	{
		/// <summary>
		/// The index of the archetype.
		/// </summary>
		[FieldOffset(0)]
		public int index;

		/// <summary>
		/// The number of entities in the archetype.
		/// </summary>
		[FieldOffset(4)]
		public int entityCount;

		/// <summary>
		/// The number of components of the archetype.
		/// </summary>
		[FieldOffset(8)]
		public int componentCount;

		/// <summary>
		/// The maximum number of entities in a chunk.
		/// </summary>
		[FieldOffset(12)]
		public int chunkCapacity;

		/// <summary>
		/// The highest chunk version of in the archetype.
		/// </summary>
		[FieldOffset(16)]
		public uint chunkVersion;

		[FieldOffset(24)]
		public ChangeVersion changeVersion;

		/// <summary>
		/// List of chunks that belong to the archetype.
		/// </summary>
		[FieldOffset(32)]
		public ChunkList chunkList;

		[FieldOffset(64)]
		public fixed byte buffer[4];

		public const int Size = 64;

		public const int Alignment = 64;

		public const int ComponentDataCount = 4;

		public void Dispose()
		{
			this.chunkList.Dispose();
		}

		public static void Construct(Archetype* archetype, uint* componentHashCodes, int* componentSizes,
			int componentCount, int chunkCapacity, ChangeVersion changeVersion)
		{
			archetype->entityCount = 0;
			archetype->componentCount = componentCount;
			archetype->chunkCapacity = chunkCapacity;
			archetype->chunkVersion = 0;
			archetype->chunkList = new ChunkList();
			archetype->changeVersion = changeVersion;

			var hashCodes = GetComponentHashCodes(archetype);
			var sizes     = GetComponentSizes(archetype);
			var offsets   = GetComponentOffsets(archetype);
			var versions  = GetComponentVersions(archetype);
			var byteCount = componentCount * sizeof(int);

			Unsafe.CopyBlockUnaligned(hashCodes, componentHashCodes, (uint)byteCount);
			Unsafe.CopyBlockUnaligned(sizes, componentSizes, (uint)byteCount);
			Unsafe.InitBlockUnaligned(versions, 0, (uint)byteCount);

			ChunkUtility.CalculateOffsets(sizes, offsets, componentCount, chunkCapacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SizeOfBuffer(int componentCount)
		{
			return sizeof(int) * ComponentDataCount * componentCount;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint* GetComponentHashCodes(Archetype* archetype)
		{
			return (uint*)archetype->buffer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int* GetComponentSizes(Archetype* archetype)
		{
			return (int*)archetype->buffer + archetype->componentCount;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int* GetComponentOffsets(Archetype* archetype)
		{
			return (int*)archetype->buffer + archetype->componentCount * 2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint* GetComponentVersions(Archetype* archetype)
		{
			return (uint*)archetype->buffer + archetype->componentCount * 3;
		}
	}
}