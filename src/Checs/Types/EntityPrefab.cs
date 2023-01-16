using System;

namespace Checs
{
	public class EntityPrefab
	{
		public static int initialCapacity = 8;

		internal ComponentType[] types;

		internal byte[] buffer;

		internal int count;

		internal int used;

		public EntityPrefab()
		{
			this.types  = new ComponentType[initialCapacity];
			this.buffer = Array.Empty<byte>();
			this.count  = 0;
		}

		public EntityPrefab(int typeCount, int byteCount)
		{
			this.types  = new ComponentType[typeCount];
			this.buffer = new byte[byteCount];
			this.count  = 0;
		}

		public EntityPrefab(ReadOnlySpan<ComponentType> types)
		{
			this.types  = new ComponentType[types.Length];
			this.buffer = new byte[GetBufferSize(types)];
			this.count  = this.types.Length;
			this.used   = this.buffer.Length;

			types.CopyTo(this.types);
		}

		public EntityPrefab(params ComponentType[] types) : this(types.AsSpan())
		{
		}

		/// <summary>
		/// Gets the component types in the prefab.
		/// </summary>
		/// <returns>The component types.</returns>
		public ReadOnlySpan<ComponentType> GetComponentTypes()
		{
			return this.types.AsSpan(0, count);
		}

		/// <summary>
		/// Checks if the prefab has a specific component type.
		/// </summary>
		/// <typeparam name="T">The component type to check.</typeparam>
		/// <returns>True if the component type was found.</returns>
		public bool HasComponentData<T>() where T : unmanaged
		{
			var hashCode = TypeRegistry<T>.info.hashCode;

			for(int i = 0; i < count; ++i)
			{
				if(types[i].hashCode == hashCode)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Gets the value of a specific component type.
		/// </summary>
		/// <typeparam name="T">The component type of the value.</typeparam>
		/// <returns>
		/// The value of the component type or the default value
		/// if the component type was not found.
		/// </returns>
		public T GetComponentData<T>() where T : unmanaged
		{
			var typeInfo = TypeRegistry<T>.info;
			var offset   = GetOffset(typeInfo.hashCode);

			if(offset >= 0)
			{
				unsafe
				{
					fixed(byte* ptr = this.buffer)
						return *((T*)(ptr + offset));
				}
			}

			return default;
		}

		public void SetComponentData<T>(in T value = default) where T : unmanaged
		{
			var typeInfo = TypeRegistry<T>.info;
			var offset   = GetOffset(typeInfo.hashCode);

			if(offset < 0)
			{
				EnsureCapacity(typeInfo.size);

				types[count++] = new ComponentType(typeInfo);

				offset = used;
				used  += Allocator.Align16(typeInfo.size);
			}

			unsafe
			{
				fixed(byte* ptr = this.buffer)
					*((T*)(ptr + offset)) = value; 
			}
		}

		internal void EnsureCapacity(int requestedCapacity)
		{
			var requiredCapacity = used + requestedCapacity;

			if(requiredCapacity > buffer.Length)
				Array.Resize(ref this.buffer, Allocator.RoundToPowerOfTwo(requiredCapacity));

			if(count == types.Length)
				Array.Resize(ref this.types, this.types.Length == 0 ? initialCapacity : this.types.Length * 2);
		}

		internal int GetOffset(uint hashCode)
		{
			var offset = 0;

			for(int i = 0; i < this.count; ++i)
			{
				if(this.types[i].hashCode == hashCode)
					return offset;
				
				offset += Allocator.Align16(this.types[i].size);
			}
				
			return -1;
		}

		internal unsafe void CopyTo(Chunk* chunk, int index)
		{
			var offset = 0;

			for(int i = 0; i < this.count; ++i)
			{
				var componentIndex = ArchetypeUtility.GetComponentIndex(chunk->archetype, this.types[i].hashCode);
				var componentPtr   = ChunkUtility.GetComponentDataPtr(chunk, index, componentIndex);

				var src = this.buffer.AsSpan(offset, this.types[i].size);
				var dst = new Span<byte>(componentPtr, this.types[i].size);

				offset += Allocator.Align16(this.types[i].size);

				src.CopyTo(dst);
			}
		}

		internal static int GetBufferSize(ReadOnlySpan<ComponentType> types)
		{
			var size = 0;

			for(int i = 0; i < types.Length; ++i)
				size += Allocator.Align16(types[i].size);
			
			return size;
		}
	}
}