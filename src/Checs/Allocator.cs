using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;

namespace Checs
{
	internal unsafe static class Allocator
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void* Alloc(int bytes)
		{
			return NativeMemory.Alloc((nuint)bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void* Realloc(void* ptr, int bytes)
		{
			return NativeMemory.Realloc(ptr, (nuint)bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void* Calloc(int bytes)
		{
			var ptr = NativeMemory.Alloc((nuint)bytes);
			Unsafe.InitBlockUnaligned(ptr, 0, (uint)bytes);

			return ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Free(void* ptr)
		{
			NativeMemory.Free(ptr);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void* AlignedAlloc(int bytes, int alignment)
		{
			return NativeMemory.AlignedAlloc((nuint)bytes, (nuint)alignment);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AlignedFree(void* ptr)
		{
			NativeMemory.AlignedFree(ptr);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RoundToPowerOfTwo(int value)
		{
			return (int)BitOperations.RoundUpToPowerOf2((uint)value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static int Align16(int byteCount)
		{
			return ((byteCount - 1) | 15) + 1;
		}
	}
}
