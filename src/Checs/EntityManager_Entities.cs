using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	public unsafe partial class EntityManager
	{
		/// <summary>
		/// Creates an empty entity.
		/// </summary>
		/// <returns>The created entity.</returns>
		public Entity CreateEntity() 
		{
			return CreateEntity(EntityArchetype.empty);
		}

		/// <summary>
		/// Creates an entity having the specified archetype.
		/// </summary>
		/// <param name="archetype">The archetype of the entity.</param>
		/// <returns>The created entity.</returns>
		public Entity CreateEntity(EntityArchetype archetype)
		{
			var arch   = GetArchetypeInternal(archetype);
			var entity = new Entity();
			CreateEntityInternal(arch, &entity, 1);
			
			return entity;
		}

		/// <summary>
		/// Creates entities according to the size of the buffer
		/// and stores them in the buffer.
		/// </summary>
		/// <param name="entities">The destination buffer.</param>
		public void CreateEntity(Span<Entity> entities)
		{
			CreateEntity(EntityArchetype.empty, entities);
		}

		/// <summary>
		/// Creates entities according to the size of the buffer
		/// and stores them in the buffer. All entities have the specified archetype.
		/// </summary>
		/// <param name="archetype">The archetype of all entities.</param>
		/// <param name="entities">The destination buffer.</param>
		public void CreateEntity(EntityArchetype archetype, Span<Entity> entities)
		{
			var arch = GetArchetypeInternal(archetype);
			fixed(Entity* ptr = entities)
				CreateEntityInternal(arch, ptr, entities.Length);
		}

		internal void CreateEntityInternal(Archetype* archetype, Entity* entities, int count)
		{
			this.entityStore.EnsureCapacity(count);

			var startIndex = 0;

			while(count > 0)
			{
				var chunk = GetOrConstructChunk(archetype, count, ref startIndex);
				var batch = GetLargestFreeEntityBatch(chunk, count);

				AllocateEntityBatch(batch);

				ChunkUtility.CopyEntities(batch.chunk, batch.index, entities, batch.count);
				ChunkUtility.ZeroComponentData(batch.chunk, batch.index, batch.count);
				
				entities += batch.count;
				count -= batch.count;
			}
		}

		/// <summary>
		/// Creates a given number of entities.
		/// </summary>
		/// <param name="archetype">The archetype of all entities.</param>
		/// <param name="count">The number of entities to create.</param>
		public void CreateEntity(EntityArchetype archetype, int count)
		{
			var arch = GetArchetypeInternal(archetype);
			CreateEntityInternal(arch, count, null, 0);
		}

		public void CreateEntity(EntityArchetype archetype, int count, Action<EntityManager> action)
		{
			var arch = GetArchetypeInternal(archetype);
			CreateEntityInternal(arch, count, action, this);
		}

		public void CreateEntity<T>(EntityArchetype archetype, int count, Action<T> action, T args)
		{
			var arch = GetArchetypeInternal(archetype);
			CreateEntityInternal(arch, count, action, args);
		}

		internal void CreateEntityInternal<T>(Archetype* archetype, int count, Action<T> action, T args)
		{
			this.entityStore.EnsureCapacity(count);

			var startIndex = 0;

			while(count > 0)
			{
				var chunk = GetOrConstructChunk(archetype, count, ref startIndex);
				var batch = GetLargestFreeEntityBatch(chunk, count);

				AllocateEntityBatch(batch);

				ChunkUtility.ZeroComponentData(batch.chunk, batch.index, batch.count);

				count -= batch.count;

				if(action == null)
					continue;
			
				action(new EntityTable(batch.chunk, batch.index, batch.count), args);
			}
		}

		/// <summary>
		/// Destroys an entity.
		/// </summary>
		/// <param name="entity">The entity to destroy.</param>
		public void DestroyEntity(Entity entity)
		{
			if(TryGetEntityInChunk(entity, out var entityInChunk))
				DestroyEntityBatch(new EntityBatch(entityInChunk));
		}

		/// <summary>
		/// Destroys all existing entities in the buffer.
		/// </summary>
		/// <param name="entities">The buffer of entities to destroy.</param>
		public void DestroyEntity(ReadOnlySpan<Entity> entities)
		{
			var count = 0;

			while(count < entities.Length)
			{
				var batch = GetFirstEntityBatch(entities.Slice(count));

				count += batch.count;

				if(batch.chunk == null)
					continue;

				DestroyEntityBatch(batch);
			}
		}

		/// <summary>
		/// Destroys all entities in the archetype.
		/// </summary>
		/// <param name="archetype">The archetype of the entities.</param>
		public void DestroyEntity(EntityArchetype archetype)
		{
			var arch = GetArchetypeInternal(archetype);
			DestroyEntityInternal(arch);
		}

		/// <summary>
		/// Destroys all entities that match the query.
		/// </summary>
		/// <param name="query">The query that the entities must match.</param>
		public void DestroyEntity(EntityQuery query)
		{
			var qry = GetQueryInternal(query);
			UpdateQueryCache(qry);

			var count      = qry->archetypeList.count;
			var archetypes = qry->archetypeList.archetypes;

			for(int i = 0; i < count; ++i)
				DestroyEntityInternal(archetypes[i]);
		}

		internal void DestroyEntityInternal(Archetype* archetype)
		{
			for(int i = 0; i < archetype->chunkList.count; ++i)
			{
				var chunk = archetype->chunkList.chunks[i];
				DestroyEntityBatch(new EntityBatch(chunk));
			}
		}

		/// <summary>
		/// Checks whether an entity is valid.
		/// </summary>
		/// <param name="entity">The entity to check.</param>
		/// <returns>True if version matches the version of the current entity at the index.</returns>
		public bool Exists(Entity entity)
		{
			var entityInChunk = this.entityStore.entitiesInChunk[entity.index];
			return entityInChunk.version == entity.version && entityInChunk.chunk != null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool TryGetEntityInChunk(Entity entity, out EntityInChunk entityInChunk)
		{
			entityInChunk = this.entityStore.entitiesInChunk[entity.index];
			return entityInChunk.version == entity.version && entityInChunk.chunk != null;
		}

		/// <summary>
		/// Clones an entity.
		/// </summary>
		/// <param name="entity">The entity to clone.</param>
		/// <param name="count">The number of entities to instantiate.</param>
		/// <returns>True if the entity to clone exists.</returns>
		public bool Instantiate(Entity entity, int count)
		{
			if(TryGetEntityInChunk(entity, out var entityInChunk))
			{
				CloneEntityInternal(entityInChunk.chunk, entityInChunk.index, count);
				return true;
			}

			return false;
		}

		internal void CloneEntityInternal(Chunk* srcChunk, int srcIndex, int count)
		{
			this.entityStore.EnsureCapacity(count);

			var archetype = srcChunk->archetype;
			var startIndex = 0;

			while(count > 0)
			{
				var chunk = GetOrConstructChunk(archetype, count, ref startIndex);
				var batch = GetLargestFreeEntityBatch(chunk, count);

				AllocateEntityBatch(batch);

				ChunkUtility.CloneComponentData(srcChunk, srcIndex, batch.chunk, batch.index, batch.count);

				count -= batch.count;
			}
		}

		/// <summary>
		/// Clones an entity.
		/// </summary>
		/// <param name="entity">The entity to clone.</param>
		/// <returns>The clone or default if the entity does not exist.</returns>
		public Entity Instantiate(Entity entity)
		{
			Span<Entity> entities = stackalloc Entity[1];

			if(Instantiate(entity, entities))
				return entities[0];

			return default;
		}

		/// <summary>
		/// Clones an entity according to the size of the buffer
		/// and stores them in the buffer.
		/// </summary>
		/// <param name="entity">The entity to clone.</param>
		/// <param name="entities">The destination buffer.</param>
		/// <returns>True if the entity to clone exists.</returns>
		public bool Instantiate(Entity entity, Span<Entity> entities)
		{
			if(TryGetEntityInChunk(entity, out var entityInChunk))
			{
				fixed(Entity* ptr = entities)
					CloneEntityInternal(entityInChunk.chunk, entityInChunk.index, ptr, entities.Length);
				return true;
			}
			
			return false;
		}

		public Entity Instantiate(EntityPrefab prefab)
		{
			Span<Entity> entities = stackalloc Entity[1];
			Instantiate(prefab, entities);

			return entities[0];
		}

		public void Instantiate(EntityPrefab prefab, Span<Entity> entities)
		{
			if(entities.Length == 0)
				return;
			
			var archetype = CreateArchetype(prefab.GetComponentTypes());
			var entity    = CreateEntity(archetype);

			TryGetEntityInChunk(entity, out var entityInChunk);
			prefab.CopyTo(entityInChunk.chunk, entityInChunk.index);

			if(entities.Length > 1)
			{
				fixed(Entity* ptr = entities)
					CloneEntityInternal(entityInChunk.chunk, entityInChunk.index, ptr + 1, entities.Length - 1);
			}

			entities[0] = entity;
		}

		internal void CloneEntityInternal(Chunk* srcChunk, int srcIndex, Entity* entities, int count)
		{
			this.entityStore.EnsureCapacity(count);

			var archetype = srcChunk->archetype;
			var startIndex = 0;

			while(count > 0)
			{
				var chunk = GetOrConstructChunk(archetype, count, ref startIndex);
				var batch = GetLargestFreeEntityBatch(chunk, count);

				AllocateEntityBatch(batch);

				ChunkUtility.CopyEntities(batch.chunk, batch.index, entities, batch.count);
				ChunkUtility.CloneComponentData(srcChunk, srcIndex, batch.chunk, batch.index, batch.count);
				
				entities += batch.count;
				count -= batch.count;
			}
		}
		
		/// <summary>
		/// Gets the archetype of the specified entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns>The archetype of the entity.</returns>
		public EntityArchetype GetArchetype(Entity entity)
		{
			if(TryGetEntityInChunk(entity, out var entityInChunk))
				return new EntityArchetype(entityInChunk.chunk->archetype->index);
			
			return default;
		}

		/// <summary>
		/// Moves an entity to the specified archetype.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <param name="archetype">The destination archetype.</param>
		/// <returns>True if the entity to move exists and does not belong to the archetype.</returns>
		public bool MoveEntity(Entity entity, EntityArchetype archetype)
		{
			if(TryGetEntityInChunk(entity, out var entityInChunk))
			{
				var arch = GetArchetypeInternal(archetype);

				if(entityInChunk.chunk->archetype != arch)
				{
					MoveEntityBatch(new EntityBatch(entityInChunk), arch);
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Moves all entities in the buffer to the specified archetype.
		/// </summary>
		/// <param name="entities">The buffer of entities to move.</param>
		/// <param name="archetype">The destination archetype.</param>
		/// <returns>
		/// The number of moved entities. If the number is not equal to the buffer size,
		/// not all entities existed or already belong to the archetype.
		/// </returns>
		public int MoveEntity(ReadOnlySpan<Entity> entities, EntityArchetype archetype)
		{
			var arch = GetArchetypeInternal(archetype);
			var count = 0;
			var moved = 0;

			while(count < entities.Length)
			{
				var batch = GetFirstEntityBatch(entities.Slice(count));

				count += batch.count;

				if(batch.chunk == null || batch.chunk->archetype == arch)
					continue;

				moved += batch.count;

				MoveEntityBatch(batch, arch);
			}

			return moved;
		}

		/// <summary>
		/// Gets the number of components of an entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns>The number of components.</returns>
		public int GetComponentCount(Entity entity)
		{
			if(TryGetEntityInChunk(entity, out var entityInChunk))
				return entityInChunk.chunk->archetype->componentCount;
			return 0;
		}

		/// <summary>
		/// Creates a prefab instance with all component types
		/// and values of the entity.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns>
		/// A new prefab instance or <c>null</c> if the entity
		/// does not exist.
		/// </returns>
		public EntityPrefab ToPrefab(Entity entity)
		{
			if(TryGetEntityInChunk(entity, out var entityInChunk))
			{
				var prefab = new EntityPrefab(entityInChunk.chunk->archetype);
				prefab.CopyFrom(entityInChunk.chunk, entityInChunk.index);

				return prefab;
			}

			return null;
		}

		internal void AllocateEntityBatch(EntityBatch batch)
		{
			ChunkUtility.ReserveEntities(batch.chunk, batch.count);
			ChunkUtility.MarkAsChanged(batch.chunk, 0);
			this.entityStore.Register(batch.chunk, batch.index, batch.count);
		}

		internal void MoveEntityBatch(EntityBatch batch, Archetype* archetype)
		{
			var chunkIndexInArray = 0;
			var count = 0;
			
			while(count < batch.count)
			{
				var dstChunk = GetOrConstructChunkWithEmptySlots(archetype, ref chunkIndexInArray);
				var dstBatch = GetLargestFreeEntityBatch(dstChunk, batch.count - count);
				
				ChunkUtility.MoveEntities(batch.chunk, batch.index + count, dstChunk, dstBatch.index, dstBatch.count);

				this.entityStore.Update(dstChunk, dstBatch.index, dstBatch.count);
				
				count += dstBatch.count;
			}

			var patchCount = ChunkUtility.PatchEntities(batch.chunk, batch.index, batch.count);
			this.entityStore.Update(batch.chunk, batch.index, patchCount);

			ReleaseChunkIfEmpty(batch.chunk);
		}

		internal void DestroyEntityBatch(EntityBatch batch)
		{
			this.entityStore.Unregister(batch.chunk, batch.index, batch.count);
			
			var patchCount = ChunkUtility.PatchEntities(batch.chunk, batch.index, batch.count);
			this.entityStore.Update(batch.chunk, batch.index, patchCount);

			ReleaseChunkIfEmpty(batch.chunk);
		}

		internal EntityBatch GetLargestFreeEntityBatch(Chunk* chunk, int count)
		{
			var free = chunk->capacity - chunk->count;
			var actualCount = (free <= count) ? free : count;

			return new EntityBatch(chunk, chunk->count, actualCount);
		}

		internal EntityBatch GetFirstEntityBatch(ReadOnlySpan<Entity> entities)
		{
			var firstEntity = entities[0];
			var firstEntityInChunk = this.entityStore.entitiesInChunk[firstEntity.index];

			Chunk* chunk = firstEntityInChunk.version == firstEntity.version
				? firstEntityInChunk.chunk
				: null;

			var count = 1;

			if(chunk == null)
			{
				for(; count < entities.Length; ++count)
				{
					var entity = entities[count];
					var entityInChunk = this.entityStore.entitiesInChunk[entity.index];

					if(entityInChunk.version == entity.version && entityInChunk.chunk != null)
						break;
				}
			} else {
				for(; count < entities.Length; ++count)
				{
					var entity = entities[count];
					var entityInChunk = this.entityStore.entitiesInChunk[entity.index];

					if(entityInChunk.index != (firstEntityInChunk.index + count))
						break;

					if(entityInChunk.chunk != chunk || entityInChunk.version != entity.version)
						break;
				}
			}

			return new EntityBatch(chunk, firstEntityInChunk.index, count);
		}
	}
}