using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential, Size = 16)]
	public struct ComponentBuffer<T> where T : unmanaged
	{
		internal unsafe Chunk* chunk;

		internal unsafe ComponentBuffer* buffer;

		internal uint chunkVersion;

		internal unsafe ComponentBuffer(Chunk* chunk, ComponentBuffer* buffer)
		{
			this.chunk = chunk;
			this.buffer = buffer;
			this.chunkVersion = chunk->version;
		}

		public void Add(T value)
		{
			CheckModified();

			unsafe
			{
				buffer->EnsureCapacity(sizeof(T));
				((T*)buffer->values)[buffer->count++] = value;
			}
		}

		public void Add(ReadOnlySpan<T> values)
		{
			CheckModified();

			unsafe
			{
				buffer->EnsureCapacity(values.Length, sizeof(T));
				values.CopyTo(new Span<T>(buffer->values, buffer->capacity).Slice(buffer->count));
				buffer->count += values.Length;
			}
		}

		public void Swapback(int index)
		{
		}

		public Span<T> AsSpan()
		{
			CheckModified();
			
			unsafe
			{
				return new Span<T>(this.buffer->values, this.buffer->count);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe void CheckModified()
		{
			if(this.chunkVersion != chunk->version)
				ThrowHelper.ThrowEntityMovedException();
		}
	}

	internal unsafe struct ComponentBuffer : IDisposable
	{
		public static int initialCapacity = 16;

		public int count;

		public int capacity;

		public void* values;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureCapacity(int requestedCapacity, int elementSize)
		{
			var requiredCapacity = this.count + requestedCapacity;

			if(requiredCapacity > this.capacity)
			{
				this.capacity = Allocator.RoundToPowerOfTwo(requiredCapacity);
				this.values   = Allocator.Realloc(this.values, this.capacity * elementSize);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureCapacity(int elementSize)
		{
			if(this.count == this.capacity)
			{
				this.capacity = this.capacity == 0 ? initialCapacity : this.capacity * 2;
				this.values   = Allocator.Realloc(this.values, this.capacity * elementSize);
			}
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