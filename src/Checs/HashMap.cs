using System;
using System.Runtime.InteropServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct HashMap<T> : IDisposable where T : unmanaged
	{
		private const int DefaultCapacity = 16;

		public int hashMask => capacity - 1;

		public int count;

		public int capacity;

		private int* m_Hashes;

		private T* m_Elements;

		public static HashMap<T> Empty() => new HashMap<T>(DefaultCapacity);

		private HashMap(int capacity)
		{
			this.count = 0;
			this.capacity = capacity;
			this.m_Hashes = MemoryUtility.Malloc<int>(this.capacity);
			this.m_Elements = MemoryUtility.Malloc<T>(this.capacity);

			MemoryUtility.MemSet(this.m_Hashes, 0, sizeof(int) * this.capacity);
		}

		public void Add(int hashCode, T element)
		{
			hashCode = hashCode != 0 ? hashCode : 1;

			int offset = hashCode & this.hashMask;

			while(true)
			{
				var hash = this.m_Hashes[offset];

				// Node is empty.
				if(hash == 0)
				{
					this.m_Hashes[offset] = hashCode;
					this.m_Elements[offset] = element;
					++this.count;
					PossiblyGrow();
					return;
				}

				offset = (offset + 1) & this.hashMask;
			}
		}

		public bool TryGet(int hashCode, out T value)
		{
			value = default;

			hashCode = hashCode != 0 ? hashCode : 1;

			int offset = hashCode & this.hashMask;
			int attempts = 0;

			while(true)
			{
				var hash = this.m_Hashes[offset];

				// Node is empty.
				if(hash == 0)
					return false;

				if(hash == hashCode)
				{
					value = this.m_Elements[offset];
					return true; 
				}

				offset = (offset + 1) & this.hashMask;
				++attempts;

				if(attempts == this.capacity)
					return false;
			}
		}

		public T Get(int hashCode)
		{
			if(TryGet(hashCode, out T value))
				return value;
			return default(T);
		}

		public void Dispose()
		{
			this.count = 0;
			this.capacity = 0;
			MemoryUtility.Free(this.m_Hashes);
			MemoryUtility.Free(this.m_Elements);
		}

		private void PossiblyGrow()
		{
			int unoccupied = this.capacity - this.count;
			if (unoccupied < this.capacity / 3)
				Resize(this.capacity * 2);
		}

		private void Resize(int size)
		{
			var temp = this;
			this = new HashMap<T>(size);
			Rehash(temp);
			temp.Dispose();
		}

		private void Rehash(HashMap<T> other)
		{
			for(int i = 0; i < other.capacity; ++i)
			{
				var hash = other.m_Hashes[i];

				if(hash != 0)
					Add(hash, other.m_Elements[i]);
			}
		}
	}
}
