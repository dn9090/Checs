using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential, Size = 16)]
	public struct ComponentBuffer<T> : IDisposable where T : unmanaged
	{
		public static int initialCapacity = 16;

		public bool isAccessible
		{
			get
			{
				unsafe { return chunk != null || version == chunk->structuralVersion; }
			}
		}

		public int count
		{
			get
			{
				unsafe { return isAccessible ? GetBuffer()->count : 0; }
			}
		}

		public int capacity
		{
			get
			{
				unsafe { return isAccessible ? GetBuffer()->capacity : 0; }
			}
		}

		internal unsafe Chunk* chunk;

		internal int offset;

		internal int version;

		internal unsafe ComponentBuffer(Chunk* chunk, int offset)
		{
			this.chunk = chunk;
			this.offset = offset;
			this.version = chunk->structuralVersion;
		}

		public void Add(T value)
		{
			unsafe
			{
				if(version == chunk->structuralVersion)
				{
					var buffer = GetBuffer();

					if(buffer->count == buffer->capacity)
					{
						var capacity = buffer->capacity == 0 ? initialCapacity : buffer->capacity * 2;
						buffer->Resize(capacity, sizeof(T));
					}

					var elements = (T*)buffer->values;
					elements[buffer->count++] = value;
				}
			}
		}

		public void Add(ReadOnlySpan<T> values)
		{
			unsafe
			{
				if(version == chunk->structuralVersion)
				{
					var buffer = GetBuffer();
					var requiredCapacity = buffer->count + values.Length;

					if(requiredCapacity > buffer->capacity)
					{
						var capacity = Allocator.RoundToPowerOfTwo(requiredCapacity);
						buffer->Resize(capacity, sizeof(T));
					}

					values.CopyTo(new Span<T>(buffer->values, values.Length).Slice(buffer->count));
				}
			}
		}

		public void Swapback(int index)
		{
			unsafe
			{
				if(version == chunk->structuralVersion)
				{
					var buffer = GetBuffer();
				
					if((uint)index >= (uint)buffer->count)
						throw new ArgumentOutOfRangeException(nameof(index));

					var elements = (T*)buffer->values;
					elements[index] = elements[--buffer->count];
				}
			}
		}

		public void Clear()
		{
			unsafe
			{
				if(version == chunk->structuralVersion)
					GetBuffer()->count = 0;
			}
		}

		public void Dispose()
		{
			unsafe
			{
				if(version == chunk->structuralVersion)
					GetBuffer()->Dispose();
			}
		}

		internal unsafe ResizableBuffer* GetBuffer()
		{
			return (ResizableBuffer*)(this.chunk->buffer + this.offset);
		}
	}

	

	/*



		var test = manager.CreateArchetype(new[] {
			ComponentType.Of<Position>(),
			ComponentType.Of<Rotation>(),
			ComponentType.Of<Collision>(ComponentFlags.Buffer)
		});

		var test = manager.CreateArchetype(new[] {
			ComponentType.Of<Position>(),
			ComponentType.Of<Rotation>(),
			ComponentType.Of<Collision>(isBuffer: true)
		});

		var test = manager.CreateArchetype(new[] {
			ComponentData.Of<Position>(),
			ComponentData.Of<Rotation>(),
			ComponentBuffer.Of<Collision>()
		});

		var test = manager.CreateArchetype(new[] {
			new Type<Position>(),
			new Type<Rotation>(),
			new Type<Collision>(isBuffer: true)
		});

		var test = manager.CreateArchetype(new[] {
			Component.Data<Position>(),
			Component.Data<Rotation>(),
			Component.Buffer<Collision>()
		}); 
	
		if(archetype->flags != 0)
		{
			for(int i = 0; i < archetype->componentCount; ++i)
			{
				if(archetype->componentFlags[i] & Flags.Buffer)
				{
					foreach(entity) buffer.Dispose();
				}

				if(archetype->componentFlags[i] & Flags.Dispose)
				{
					var dispose = TypeRegistry.GetDisposeFunction()

					foreach(entity) dispose(component);
				}
			}
		}


	*/
}