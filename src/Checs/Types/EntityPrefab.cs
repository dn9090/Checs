using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	/// <summary>
	/// Stores component types and values that act as templates
	/// from which entities can be instantiated.
	/// </summary>
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
			if(typeCount < 0)
				throw new ArgumentOutOfRangeException(nameof(typeCount));

			if(byteCount < 0)
				throw new ArgumentOutOfRangeException(nameof(byteCount));

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

		internal unsafe EntityPrefab(Archetype* archetype)
		{
			this.count = archetype->componentCount - 1; // Skip entity.
			this.types = new ComponentType[this.count];

			ArchetypeUtility.GetComponentTypes(archetype, 1, this.types);

			this.buffer = new byte[GetBufferSize(this.types)];
		}

		/// <summary>
		/// Gets the component types in the prefab.
		/// </summary>
		/// <returns>The component types.</returns>
		public ReadOnlySpan<ComponentType> GetComponentTypes()
		{
			return this.types.AsSpan(0, this.count);
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
				return Unsafe.ReadUnaligned<T>(ref this.buffer[offset]);

			return default;
		}

		/// <summary>
		/// Sets the value of the component type or adds the component type
		/// if it was not found.
		/// </summary>
		/// <param name="value">The value of the component type.</param>
		/// <typeparam name="T">The component type of the value.</typeparam>
		public void SetComponentData<T>(in T value = default) where T : unmanaged
		{
			var typeInfo = TypeRegistry<T>.info;
			var offset   = GetOffset(typeInfo.hashCode);

			if(offset < 0)
			{
				EnsureCapacity(typeInfo.size);

				types[count++] = new ComponentType(typeInfo);

				offset = used;
				used  += ChunkUtility.Align(typeInfo.size);
			}

			Unsafe.WriteUnaligned(ref this.buffer[offset], value);
		}

		/// <summary>
		/// Fluently sets the value of the component type or adds the component type
		/// if it was not found.
		/// </summary>
		/// <param name="value">The value of the component type.</param>
		/// <typeparam name="T">The component type of the value.</typeparam>
		/// <returns>The prefab instance.</returns>
		public EntityPrefab WithComponentData<T>(in T value = default) where T : unmanaged
		{
			SetComponentData<T>(in value);
			return this;
		}

		/// <summary>
		/// Writes the boxed value of the component type or adds the component type
		/// if it was not found.
		/// </summary>
		/// <param name="value">The boxed value.</param>
		public void WriteComponentData(object value)
		{
			var typeInfo = TypeRegistry.GetTypeInfo(value.GetType());
			var offset   = GetOffset(typeInfo.hashCode);

			if(offset < 0)
			{
				EnsureCapacity(typeInfo.size);

				types[count++] = new ComponentType(typeInfo);

				offset = used;
				used  += ChunkUtility.Align(typeInfo.size);
			}

			var src = TypeUtility.Unbox(value, typeInfo.size);
			var dst = this.buffer.AsSpan(offset);
			
			src.CopyTo(dst);
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
				
				offset += ChunkUtility.Align(this.types[i].size);
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

				offset += ChunkUtility.Align(this.types[i].size);

				src.CopyTo(dst);
			}
		}

		internal unsafe void CopyFrom(Chunk* chunk, int index)
		{
			var offset = 0;
			
			for(int i = 0; i < this.count; ++i)
			{
				var componentIndex = ArchetypeUtility.GetComponentIndex(chunk->archetype, this.types[i].hashCode);
				var componentPtr   = ChunkUtility.GetComponentDataPtr(chunk, index, componentIndex);

				var src = new Span<byte>(componentPtr, this.types[i].size);
				var dst = this.buffer.AsSpan(offset, this.types[i].size);

				offset += ChunkUtility.Align(this.types[i].size);

				src.CopyTo(dst);
			}
		}

		internal static int GetBufferSize(ReadOnlySpan<ComponentType> types)
		{
			var size = 0;

			for(int i = 0; i < types.Length; ++i)
				size += ChunkUtility.Align(types[i].size);
			
			return size;
		}
	}
}