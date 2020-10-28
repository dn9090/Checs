using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	internal unsafe struct FreeEntitySlotList // : IDisposable
	{
		internal int count;

		internal int capacity;

		internal int* slots;

		public void EnsureCapacity(int count)
		{
			int requiredCapacity = this.count + count;

			if(requiredCapacity > this.capacity)
			{
				this.capacity = MemoryUtility.RoundToPowerOfTwo(requiredCapacity);
				this.slots = MemoryUtility.Realloc<int>(this.slots, this.capacity);
			} else if(requiredCapacity < this.capacity / 2) {
				this.slots = MemoryUtility.Realloc<int>(this.slots, this.capacity / 2);
			}
		}

		public Span<int> Allocate(int count)
		{
			EnsureCapacity(count);
			this.count += count;
			return new Span<int>(&this.slots[this.count], count);
		}

		public Span<int> Recycle(int count)
		{
			int availableSlots = Math.Min(this.count, count);
			this.count -= availableSlots;
			return new Span<int>(&this.slots[this.count], availableSlots);
		}
	}
}