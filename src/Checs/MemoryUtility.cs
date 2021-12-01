using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	public static unsafe class MemoryUtility
	{
		public static void* Malloc(int size)
		{
			return (void*)Marshal.AllocHGlobal(size);
		}

		public static void* Realloc(void* ptr, int size)
		{
			return (void*)Marshal.ReAllocHGlobal((IntPtr)ptr, (IntPtr)size);
		}

		public static T* Malloc<T>() where T : unmanaged
		{
			return (T*)Malloc(sizeof(T));
		}

		public static T* Malloc<T>(int count) where T : unmanaged
		{
			return (T*)Malloc(sizeof(T) * count);
		}

		public static T* Malloc<T>(Span<T> source) where T : unmanaged
		{
			T* dest = Malloc<T>(source.Length);

			var size = source.Length * sizeof(T);

			fixed(T* ptr = source)
				Buffer.MemoryCopy((void*)ptr, (void*)dest, size, size);

			return dest;
		}

		public static T* Realloc<T>(T* ptr, int count) where T : unmanaged
		{
			return (T*)Realloc((void*)ptr, sizeof(T) * count);
		}

		public static void Free(void* ptr)
		{
			Marshal.FreeHGlobal((IntPtr)ptr);
		}

		public static void Free<T>(T* ptr) where T : unmanaged
		{
			Free((void*)ptr);
		}

		public static void MemSet(void* ptr, byte value, int size)
		{
			Unsafe.InitBlock(ptr, value, (uint)(size));
		}

		public static T** MallocPtrArray<T>(int count) where T : unmanaged
		{
			return (T**)Marshal.AllocHGlobal(sizeof(T*) * count);
		}

		public static T** ReallocPtrArray<T>(T** ptr, int count) where T : unmanaged
		{
			return (T**)Marshal.ReAllocHGlobal((IntPtr)ptr, (IntPtr)(sizeof(T*) * count));
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