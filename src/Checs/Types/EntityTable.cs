using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	/// <summary>
	/// A continuous chunk of components of an archetype.
	/// </summary>
	public readonly ref struct EntityTable
	{
		internal unsafe readonly Chunk* chunk;

		internal readonly int index;

		/// <summary>
		/// The total number of entities in the table.
		/// </summary>
		public readonly int length;

		/// <summary>
		/// Returns the archetype of the table.
		/// </summary>
		public EntityArchetype archetype
		{
			get
			{
				unsafe { return new EntityArchetype(this.chunk->archetype->index); }
			}
		}

		/// <summary>
		/// Returns the number of component types in the table.
		/// </summary>
		public int typeCount
		{
			get
			{
				unsafe { return this.chunk->archetype->componentCount; }
			}
		}

		/// <summary>
		/// Gets the entity at the specified index.
		/// </summary>
		/// <value>The entity at the specified index.</value>
		public Entity this[int index]
		{
			get
			{
				unsafe
				{
					if((uint)index >= (uint)this.length)
						throw new ArgumentOutOfRangeException(nameof(index));
					return ChunkUtility.GetEntities(chunk, this.index)[index];
				}
			}
		}

		internal unsafe EntityTable(Chunk* chunk)
		{
			this.chunk = chunk;
			this.index = 0;
			this.length = chunk->count;
		}

		internal unsafe EntityTable(Chunk* chunk, int index, int length)
		{
			this.chunk = chunk;
			this.index = index;
			this.length = length;
		}

		/// <summary>
		/// Checks whether the table has a specific component type.
		/// </summary>
		/// <typeparam name="T">The component type.</typeparam>
		/// <returns>True, if the component type is in the table.</returns>
		public bool HasComponentData<T>() where T : unmanaged
		{
			unsafe
			{
				var hashCode = TypeRegistry<T>.info.hashCode;
				return ArchetypeUtility.GetComponentIndex(this.chunk->archetype, hashCode) >= 0;
			}
		}

		/// <summary>
		/// Gets all entities in the table.
		/// </summary>
		public ReadOnlySpan<Entity> GetEntities()
		{
			unsafe
			{
				var ptr = ChunkUtility.GetEntities(this.chunk, this.index);
				return new ReadOnlySpan<Entity>(ptr, this.length);
			}
		}
		
		/// <summary>
		/// Gets all component values of a specific component type.
		/// </summary>
		/// <typeparam name="T">The component type of the values.</typeparam>
		/// <returns>
		/// Read-only access to all component values of the component type
		/// in the table or <see cref="ReadOnlySpan{T}.Empty"/> if
		/// the component type is not in the table.
		/// </returns>
		public ReadOnlySpan<T> GetComponentDataReadOnly<T>() where T : unmanaged
		{
			unsafe
			{
				var hashCode = TypeRegistry<T>.info.hashCode;
				var componentIndex = ArchetypeUtility.GetComponentIndex(this.chunk->archetype, hashCode);

				if(componentIndex >= 0)
				{
					var ptr = ChunkUtility.GetComponentDataPtr<T>(this.chunk, componentIndex);
					return new Span<T>(ptr + this.index, this.length);
				}
			}

			return Span<T>.Empty;
		}

		/// <summary>
		/// Gets all component values of a specific component type.
		/// </summary>
		/// <remarks>
		/// Increments the change version.
		/// </remarks>
		/// <typeparam name="T">The component type of the values.</typeparam>
		/// <returns>
		/// Read and write access to all component values of the component type
		/// in the table or <see cref="Span{T}.Empty"/> if
		/// the component type is not in the table.
		/// </returns>
		public Span<T> GetComponentData<T>() where T : unmanaged
		{
			unsafe
			{
				var hashCode = TypeRegistry<T>.info.hashCode;
				var componentIndex = ArchetypeUtility.GetComponentIndex(this.chunk->archetype, hashCode);

				if(componentIndex >= 0)
				{
					var ptr = ChunkUtility.GetComponentDataPtr<T>(this.chunk, componentIndex);
					ChunkUtility.MarkAsChanged(this.chunk, componentIndex);
					return new Span<T>(ptr + this.index, this.length);
				}
			}

			return Span<T>.Empty;
		}

		/// <summary>
		/// Gets the pointer to all component values of a specific component type.
		/// The address is aligned to a 16 byte boundary.
		/// </summary>
		/// <remarks>
		/// Does not increment the change version, although modification of the data is possible.
		/// If needed, call <see cref="EntityTable.GetComponentData{T}"/> to increment the change version.
		/// </remarks>
		/// <typeparam name="T">The component type of the values.</typeparam>
		/// <returns>
		/// A pointer to directly access the component values of the component type or
		/// <see cref="IntPtr.Zero"/> if the component type is not in the table.
		/// </returns>
		public IntPtr GetComponentDataPtr<T>() where T : unmanaged
		{
			unsafe
			{
				var hashCode = TypeRegistry<T>.info.hashCode;
				var componentIndex = ArchetypeUtility.GetComponentIndex(this.chunk->archetype, hashCode);

				if(componentIndex >= 0)
				{
					var ptr = ChunkUtility.GetComponentDataPtr<T>(this.chunk, componentIndex);
					return (IntPtr)(ptr + this.index);
				}
			}
			
			return IntPtr.Zero;
		}

		public bool DidChange(EntityChangeVersion changeVersion)
		{
			unsafe
			{
				return ChangeVersion.DidChange(chunk->changeVersion, changeVersion.version);
			} // TODO
		}

		public bool DidChange<T>(EntityChangeVersion changeVersion)
		{
			unsafe
			{
				var hashCode         = TypeRegistry<T>.info.hashCode;
				var chunkChanged     = ChangeVersion.DidChange(chunk->changeVersion, changeVersion.version);
				var componentChanged = ArchetypeUtility.DidChange(chunk->archetype, hashCode, changeVersion.version);
				return chunkChanged && componentChanged;
			}
		}

		public bool DidChange(EntityChangeVersion changeVersion, ComponentType type)
		{
			unsafe
			{
				var chunkChanged     = ChangeVersion.DidChange(chunk->changeVersion, changeVersion.version);
				var componentChanged = ArchetypeUtility.DidChange(chunk->archetype, type.hashCode, changeVersion.version);
				return chunkChanged && componentChanged;
			}
		}
	}
}