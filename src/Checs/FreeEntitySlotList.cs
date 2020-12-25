using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct FreeEntitySlotList : IDisposable
	{
		private const int DefaultCapacity = 8;

		internal int count;

		internal int capacity;

		internal int* slots;

		public static FreeEntitySlotList Empty() => new FreeEntitySlotList(DefaultCapacity);

		private FreeEntitySlotList(int capacity)
		{
			this.count = 0;
			this.capacity = capacity;
			this.slots = MemoryUtility.Malloc<int>(this.capacity);
		}

		public void EnsureCapacity(int count)
		{
			int requiredCapacity = this.count + count;

			if(requiredCapacity > this.capacity)
			{
				this.capacity = MemoryUtility.RoundToPowerOfTwo(requiredCapacity);
				this.slots = MemoryUtility.Realloc<int>(this.slots, this.capacity);
			}
		}

		public Span<int> Allocate(int count)
		{
			EnsureCapacity(count);
			
			var slots = new Span<int>(&this.slots[this.count], count);
			this.count += count;

			return slots;
		}

		public Span<int> Recycle(int count)
		{
			int availableSlots = Math.Min(this.count, count);
			this.count -= availableSlots;
			return new Span<int>(&this.slots[this.count], availableSlots);
		}

		public void Dispose()
		{
			this.count = 0;
			this.capacity = 0;
			MemoryUtility.Free<int>(this.slots);
		}
	}
}