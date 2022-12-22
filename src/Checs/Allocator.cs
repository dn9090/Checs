using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;

namespace Checs
{
	internal unsafe static class AllocatorNOpe
	{
		// I'm the native allocator, I'm bi-modal. Just run
		//   for(int i = 0; i < count; ++i)
		//	   pointers[i] = (byte*)NativeMemory.Alloc(1024 * 16);
		//   for(int i = 0; i < count; ++i)
		//     NativeMemory.Free(pointers[i]);
		// and look at this shit. Latency between 9 and 18 us/op.
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
		
		public static int RoundToPowerOfTwo(int value)
		{
			return (int)BitOperations.RoundUpToPowerOf2((uint)value);
		}
	}
}
