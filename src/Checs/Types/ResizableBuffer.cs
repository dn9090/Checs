using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential, Size = 16)]
	public unsafe struct ResizableBuffer : IDisposable
	{
		public int count;

		public int capacity;

		public void* values;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Resize(int capacity, int elementSize)
		{
			this.capacity = capacity;
			this.values = Allocator.Realloc(this.values, capacity * elementSize);
		}

		public void Dispose()
		{
			Allocator.Free(this.values);

			this.values = null;
			this.count = 0;
			this.capacity = 0;
		}
	}
}