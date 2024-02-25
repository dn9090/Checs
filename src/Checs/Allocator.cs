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
			return NativeMemory.AllocZeroed((nuint)bytes);
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
		public static void* AlignedRealloc(void* ptr, int bytes, int alignment)
		{
			return NativeMemory.AlignedRealloc(ptr, (nuint)bytes, (nuint)alignment);
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
	}
}
