using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Checs
{
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct Archetype : IDisposable
	{
		[FieldOffset(0)]
		public int index;

		[FieldOffset(4)]
		public int entityCount;

		[FieldOffset(8)]
		public int componentCount;

		[FieldOffset(12)]
		public int chunkCapacity;

		[FieldOffset(16)]
		public uint chunkVersion;

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
			int componentCount, int chunkCapacity)
		{
			archetype->entityCount = 0;
			archetype->componentCount = componentCount;
			archetype->chunkCapacity = chunkCapacity;
			archetype->chunkVersion = 0;
			archetype->chunkList = new ChunkList();

			var hashCodes = GetComponentHashCodes(archetype);
			var sizes     = GetComponentSizes(archetype);
			var offsets   = GetComponentOffsets(archetype);
			var versions  = GetComponentVersions(archetype);
			var byteCount = componentCount * sizeof(int);

			Unsafe.CopyBlockUnaligned(hashCodes, componentHashCodes, (uint)byteCount);
			Unsafe.CopyBlockUnaligned(sizes, componentSizes, (uint)byteCount);
			Unsafe.InitBlockUnaligned(versions, 0, (uint)byteCount);

			ArchetypeUtility.CalculateComponentOffsets(sizes, offsets, componentCount, chunkCapacity);
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
		public static int* GetComponentVersions(Archetype* archetype)
		{
			return (int*)archetype->buffer + archetype->componentCount * 3;
		}
	}
}