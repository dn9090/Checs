using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading;

namespace Checs
{
	public unsafe partial class EntityManager
	{
		/// <summary>
		/// Checks whether an entity has a specific type of component.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <typeparam name="T">The component type to check.</typeparam>
		/// <returns>True, if the entity exists and has the component type.</returns>
		public bool HasComponentData<T>(Entity entity) where T : unmanaged
		{
			if(TryGetEntityInChunk(entity, out var entityInChunk))
			{
				var hashCode = TypeRegistry<T>.info.hashCode;
				return ArchetypeUtility.GetComponentIndex(entityInChunk.chunk->archetype, hashCode) >= 0;
			}

			return false;
		}

		/// <summary>
		/// Gets the value of a specific component type for an entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <typeparam name="T">The component type of the value.</typeparam>
		/// <returns>
		/// The value of the component type or the default value
		/// if the entity does not exist or the entity does not have the component type.
		/// </returns>
		public T GetComponentData<T>(Entity entity) where T : unmanaged
		{
			TryGetComponentData<T>(entity, out var value);
			return value;
		}

		/// <summary>
		/// Tries to get the value of a specific component type for an entity. 
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="value">The value of the component type.</param>
		/// <typeparam name="T">The component type of the value.</typeparam>
		/// <returns>True if the entity exists and has the component type.</returns>
		public bool TryGetComponentData<T>(Entity entity, out T value) where T : unmanaged
		{
			if(TryGetEntityInChunk(entity, out var entityInChunk))
			{
				var hashCode = TypeRegistry<T>.info.hashCode;
				var componentIndex = ArchetypeUtility.GetComponentIndex(entityInChunk.chunk->archetype, hashCode);

				if(componentIndex >= 0)
				{
					value = ChunkUtility.GetComponentDataPtr<T>(entityInChunk.chunk, componentIndex)[entityInChunk.index];
					return true;
				}
			}

			value = default;
			return false;
		}

		/// <summary>
		/// Set the value of a specific component type for an entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="value">The value of the component type.</param>
		/// <typeparam name="T">The component type of the value.</typeparam>
		/// <returns>True if the entity exists and has the component type.</returns>
		public bool SetComponentData<T>(Entity entity, in T value) where T : unmanaged
		{
			if(TryGetEntityInChunk(entity, out var entityInChunk))
			{
				var hashCode = TypeRegistry<T>.info.hashCode;
				var componentIndex = ArchetypeUtility.GetComponentIndex(entityInChunk.chunk->archetype, hashCode);

				if(componentIndex >= 0)
				{
					ChunkUtility.IncrementChangeVersion(entityInChunk.chunk, componentIndex);
					ChunkUtility.GetComponentDataPtr<T>(entityInChunk.chunk, componentIndex)[entityInChunk.index] = value;
					return true;
				}
			}

			return false;
		}

		public bool SetComponentData_NOVERSION<T>(Entity entity, in T value) where T : unmanaged
		{
			if(TryGetEntityInChunk(entity, out var entityInChunk))
			{
				var hashCode = TypeRegistry<T>.info.hashCode;
				var componentIndex = ArchetypeUtility.GetComponentIndex(entityInChunk.chunk->archetype, hashCode);

				if(componentIndex >= 0)
				{
					ChunkUtility.GetComponentDataPtr<T>(entityInChunk.chunk, componentIndex)[entityInChunk.index] = value;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Sets the value of a specific component type for all entities in the buffer.
		/// </summary>
		/// <param name="entities">The buffer of entities.</param>
		/// <param name="value">The value of the component type.</param>
		/// <typeparam name="T">The component type of the value.</typeparam>
		/// <returns>
		/// The number of entities for which the value was set.
		/// If the number is not equal to the buffer size,
		/// not all entities existed or not all entities have the specific component type.
		/// </returns>
		public int SetComponentData<T>(ReadOnlySpan<Entity> entities, in T value) where T : unmanaged
		{
			var hashCode = TypeRegistry<T>.info.hashCode;
			var count = 0;
			var touched = 0;

			while(count < entities.Length)
			{
				var entityBatch = GetFirstEntityBatch(entities.Slice(count));

				count += entityBatch.count;

				if(entityBatch.chunk == null)
					continue;

				var componentIndex = ArchetypeUtility.GetComponentIndex(entityBatch.chunk->archetype, hashCode);

				if(componentIndex >= 0)
				{
					ChunkUtility.IncrementChangeVersion(entityBatch.chunk, componentIndex);
					ChunkUtility.RepeatComponentData<T>(entityBatch.chunk, entityBatch.index, entityBatch.count, componentIndex, in value);
					touched += entityBatch.count;
				}
			}

			return touched;
		}
		
		/// <summary>
		/// Adds a specific component type to the entity and
		/// sets the value of the component.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="value">The value of the component type.</param>
		/// <typeparam name="T">The component type of the value.</typeparam>
		/// <returns>True if the entity exists.</returns>
		public bool SetOrAddComponentData<T>(Entity entity, in T value) where T : unmanaged
		{
			if(TryGetEntityInChunk(entity, out var entityInChunk))
			{
				var typeInfo = TypeRegistry<T>.info;
				var componentIndex = ArchetypeUtility.GetComponentIndex(entityInChunk.chunk->archetype, typeInfo.hashCode);

				if(componentIndex < 0)
				{
					var archetype = CreateArchetypeInternal(entityInChunk.chunk->archetype, &typeInfo.hashCode, &typeInfo.size, 1);
					MoveEntityBatch(new EntityBatch(entityInChunk), GetArchetypeInternal(archetype));

					entityInChunk = this.entityStore.entitiesInChunk[entity.index];
					componentIndex = ArchetypeUtility.GetComponentIndex(entityInChunk.chunk->archetype, typeInfo.hashCode);	
				} else {
					ChunkUtility.IncrementChangeVersion(entityInChunk.chunk, componentIndex);
				}

				ChunkUtility.GetComponentDataPtr<T>(entityInChunk.chunk, componentIndex)[entityInChunk.index] = value;
				return true;
			}

			return false;
		}
		
		/// <summary>
		/// Adds a specific component type to the entity
		/// and sets the value of the component.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="value">The value of the component type.</param>
		/// <typeparam name="T">The component type of the value.</typeparam>
		/// <returns>True if the entity exists and does not have the component type.</returns>
		public bool AddComponentData<T>(Entity entity, in T value = default) where T : unmanaged
		{
			if(TryGetEntityInChunk(entity, out var entityInChunk))
			{
				var typeInfo = TypeRegistry<T>.info;
				var componentIndex = ArchetypeUtility.GetComponentIndex(entityInChunk.chunk->archetype, typeInfo.hashCode);

				if(componentIndex < 0)
				{
					var archetype = CreateArchetypeInternal(entityInChunk.chunk->archetype, &typeInfo.hashCode, &typeInfo.size, 1);
					MoveEntityBatch(new EntityBatch(entityInChunk), GetArchetypeInternal(archetype));

					entityInChunk = this.entityStore.entitiesInChunk[entity.index];
					componentIndex = ArchetypeUtility.GetComponentIndex(entityInChunk.chunk->archetype, typeInfo.hashCode);
					ChunkUtility.GetComponentDataPtr<T>(entityInChunk.chunk, componentIndex)[entityInChunk.index] = value;

					return true;
				}
			}

			return false;
		}
		
		/// <summary>
		/// Removes a specific component type from the entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <typeparam name="T">The component type to remove.</typeparam>
		/// <returns>True if the entity exists and has the component type.</returns>
		public bool RemoveComponentData<T>(Entity entity) where T : unmanaged
		{
			if(TryGetEntityInChunk(entity, out var entityInChunk))
			{
				var typeInfo = TypeRegistry<T>.info;
				var componentIndex = ArchetypeUtility.GetComponentIndex(entityInChunk.chunk->archetype, typeInfo.hashCode);

				if(componentIndex >= 0)
				{
					var archetype = CreateArchetypeWithoutInternal(entityInChunk.chunk->archetype, &typeInfo.hashCode, 1);
					MoveEntityBatch(new EntityBatch(entityInChunk), GetArchetypeInternal(archetype));
					return true;
				}
			}

			return false;
		}

		public int CopyComponentData<T>(EntityArchetype archetype, Span<T> buffer) where T : unmanaged
		{
			var arch = GetArchetypeInternal(archetype);
			var hashCode = TypeRegistry<T>.info.hashCode;

			return CopyComponentDataInternal(arch, buffer, hashCode);
		}

		public int CopyComponentData<T>(EntityQuery query, Span<T> buffer) where T : unmanaged
		{
			var qry = GetQueryInternal(query);

			UpdateQueryCache(qry);

			var hashCode = TypeRegistry<T>.info.hashCode;
			var count = 0;

			for(int i = 0; i < qry->archetypeList.count && count < buffer.Length; ++i)
				count += CopyComponentDataInternal(qry->archetypeList.archetypes[i], buffer.Slice(count), hashCode);

			return count;
		}

		internal int CopyComponentDataInternal<T>(Archetype* archetype, Span<T> buffer, uint hashCode)  where T : unmanaged
		{
			var componentIndex = ArchetypeUtility.GetComponentIndex(archetype, hashCode);

			if(componentIndex < 0)
				return 0;

			var chunkCount = archetype->chunkList.count;
			var chunks = archetype->chunkList.chunks;
			var count = 0;
			
			fixed(T* ptr = buffer)
			{
				for(int i = 0; i < chunkCount && count < buffer.Length; ++i)
					count += ChunkUtility.CopyComponentData<T>(chunks[i], componentIndex, ptr + count, buffer.Length - count);
			}

			return count;
		}

		internal void WriteComponentData(ReadOnlySpan<Entity> entities, byte* value, int size, uint hashCode)
		{
			var count = 0;

			while(count < entities.Length)
			{
				var entityBatch = GetFirstEntityBatch(entities.Slice(count));

				count += entityBatch.count;

				if(entityBatch.chunk == null)
					continue;

				var componentIndex = ArchetypeUtility.GetComponentIndex(entityBatch.chunk->archetype, hashCode);

				if(componentIndex >= 0)
				{
					ChunkUtility.IncrementChangeVersion(entityBatch.chunk, componentIndex);
					ChunkUtility.RepeatComponentData(entityBatch.chunk, entityBatch.index, entityBatch.count,
						componentIndex, value, size);
				}
			}
		}
	}
}