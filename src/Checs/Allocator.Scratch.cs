using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	internal unsafe static class Allocator_Scratch
	{
		[StructLayout(LayoutKind.Sequential, Size = 64)]
		internal struct Node
		{
			public int bytes;
		}

		public static byte* memory;

		public static long used;

		public static long capacity;

		static Allocator_Scratch()
		{
			capacity = RoundUp(1_000_000_000);
			used = 0;
			memory = (byte*)NativeMemory.AlignedAlloc((nuint)capacity, 64);

			if(memory == null)
				throw new InvalidOperationException("Cannot allocate memory.");
		}

		public static void Reset()
		{
			used = 0;
		}

		public static int RoundUp(int minimum)
		{
			const int Alignment = 64;
			return (minimum + Alignment - 1) & ~(Alignment - 1);
		}

		public static byte* Bump(int bytes)
		{
			var adjusted = RoundUp(bytes);

			if(used + adjusted > capacity)
				throw new OutOfMemoryException($"Not enough capacity (used {used} required {used + adjusted}/{capacity}).");

			byte *ptr = memory + used;
			((Node*)ptr)->bytes = bytes;

			used += adjusted + sizeof(Node);

			return ptr + sizeof(Node);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void* Alloc(int bytes)
		{
			return Bump(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void* Realloc(void* ptr, int bytes)
		{
			if(ptr == null)
				return Alloc(bytes);

			var size = ((Node*)((byte*)ptr - sizeof(Node)))->bytes;
			var dest = Bump(bytes);

			Unsafe.CopyBlockUnaligned(dest, ptr, (uint)size);

			return dest;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void* Calloc(int bytes)
		{
			var ptr = Bump(bytes);
			Unsafe.InitBlockUnaligned(ptr, 0, (uint)bytes);

			return ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Free(void* ptr)
		{
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void* AlignedAlloc(int bytes, int alignment)
		{
			return Bump(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AlignedFree(void* ptr)
		{
		}

		public static int RoundToPowerOfTwo(int value)
		{
			uint uvalue = (uint)value;

			--uvalue;
			uvalue |= uvalue >> 1;
			uvalue |= uvalue >> 2;
			uvalue |= uvalue >> 4;
			uvalue |= uvalue >> 8;
			uvalue |= uvalue >> 16;
			++uvalue;

			return (int)uvalue;
		}
	}	
}
