using System;
using System.Runtime.InteropServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct HashMap<T> : IDisposable where T : unmanaged
	{
		public int count;

		public int capacity;

		public uint* hashes;

		public T* elements;

		public HashMap(int capacity)
		{
			this.count = 0;
			this.capacity = capacity;
			this.hashes = (uint*)Allocator.Calloc(sizeof(int) * this.capacity);
			this.elements = (T*)Allocator.Alloc(sizeof(T) * this.capacity);
		}

		public void Add(uint hashCode, T element)
		{
			hashCode = hashCode != 0 ? hashCode : 1;

			var mask = this.capacity - 1;
			var offset = (int)hashCode & mask;

			while(true)
			{
				var hash = this.hashes[offset];

				// Node is empty.
				if(hash == 0)
				{
					this.hashes[offset] = hashCode;
					this.elements[offset] = element;
					++this.count;
					PossiblyResizeAndRehash();
					return;
				}

				offset = (offset + 1) & mask;
			}
		}

		public bool TryGet(uint hashCode, out T value)
		{
			value = default;

			hashCode = hashCode != 0 ? hashCode : 1;

			var mask = this.capacity - 1;
			var offset = hashCode & mask;
			var attempts = 0;

			do
			{
				var hash = this.hashes[offset];

				// Node is empty.
				if(hash == 0)
					return false;

				if(hash == hashCode)
				{
					value = this.elements[offset];
					return true; 
				}

				offset = (offset + 1) & mask;
				
				++attempts;
			} while(attempts < this.capacity);

			return false;
		}

		public T Get(uint hashCode)
		{
			TryGet(hashCode, out T value);
			return value;
		}

		public void PossiblyResizeAndRehash()
		{
			var unoccupied = this.capacity - this.count;
			
			if (unoccupied < this.capacity / 3)
			{
				var size = this.capacity * 2;
				var temp = this;
				this = new HashMap<T>(size);
				Rehash(temp);
				temp.Dispose();
			}
		}

		public void Rehash(HashMap<T> other)
		{
			for(int i = 0; i < other.capacity; ++i)
			{
				var hash = other.hashes[i];

				if(hash != 0)
					Add(hash, other.elements[i]);
			}
		}

		public void Dispose()
		{
			Allocator.Free(this.hashes);
			Allocator.Free(this.elements);

			this.count = 0;
			this.capacity = 0;
		}
	}
}
