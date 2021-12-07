using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	// TODO: Block structural changes in query.

	public unsafe partial class EntityManager
	{
		public Entity CreateEntity() => CreateEntity(CreateArchetype());

		public Entity CreateEntity(EntityArchetype archetype)
		{
			Span<Entity> entity = stackalloc Entity[1];
			CreateEntity(archetype, entity);
			
			return entity[0];
		}

		public ReadOnlySpan<Entity> CreateEntity(int count) =>
			CreateEntity(CreateArchetype(), count);

		public ReadOnlySpan<Entity> CreateEntity(EntityArchetype archetype, int count)
		{
			Span<Entity> entities = new Entity[count];
			CreateEntityInternal(GetArchetypeInternal(archetype), entities);
			
			return entities;
		}

		public void CreateEntity(Span<Entity> entities) =>
			CreateEntity(CreateArchetype(), entities);

		public void CreateEntity(EntityArchetype archetype, Span<Entity> entities) =>
			CreateEntityInternal(GetArchetypeInternal(archetype), entities);

		internal void CreateEntityInternal(Archetype* archetype, Span<Entity> entities)
		{
			this.entityStore->EnsureCapacity(entities.Length);
			this.entityStore->ReserveEntityBatch(entities);

			var allocatedEntityCount = 0;
			var chunkIndexInArray = 0;
			
			// This needs investigation: For some reason calling the ReserveEntityBatch
			// with the Span is faster than with the pointer..., even though the entire
			// fixed statement in the SSE branch is eliminated, as well as (possibly) the
			// ArgumentOutOfRange checks.

			// Because the count of entities and the entity size (with components) is known,
			// it should be possible to reserve the chunks beforehand, not allocating them
			// one by one in GetChunkWithEmptySlots().

			fixed(Entity* ptr = entities)
			{
				// TODO: If the size of the entities is larger than a chunk allocating a chunk directly to
				// avoid fragmenting the entities to separate chunks with GetChunkWithEmptySlots may be better.

				while(allocatedEntityCount < entities.Length)
				{
					Chunk* chunk = archetype->chunkArray->GetChunkWithEmptySlots(ref chunkIndexInArray);
					var chunkCount = chunk->count;

					var buffer = ptr + allocatedEntityCount;
					var allocatedInChunk = ChunkUtility.AllocateEntities(chunk, buffer, entities.Length - allocatedEntityCount);

					for(int i = 0; i < allocatedInChunk; ++i)
					{
						Entity entity = buffer[i];
						this.entityStore->entitiesInChunk[entity.index] = new EntityInChunk(chunk, chunkCount + i, entity.version);
					}

					allocatedEntityCount += allocatedInChunk;
				}
			}
		}

		internal void CreateEntityInternal_LEGACY(Archetype* archetype, Span<Entity> entities)
		{
			this.entityStore->EnsureCapacity(entities.Length);
			this.entityStore->ReserveEntityBatch(entities);

			var allocatedEntityCount = 0;
			var chunkIndexInArray = 0;

			while(allocatedEntityCount < entities.Length)
			{
				Chunk* chunk = archetype->chunkArray->GetChunkWithEmptySlots(ref chunkIndexInArray);
				int chunkCount = chunk->count;

				Span<Entity> entitiesInChunk = ChunkUtility.AllocateEntities_LEGACY(chunk, entities.Length - allocatedEntityCount);

				for(int i = 0; i < entitiesInChunk.Length; ++i)
				{
					Entity entity = entities[allocatedEntityCount++];
					entitiesInChunk[i] = entity;
					this.entityStore->entitiesInChunk[entity.index] = new EntityInChunk(chunk, chunkCount + i, entity.version);
				}
			}
		}

		public void DestroyEntity(Entity entity) => DestroyEntity(new ReadOnlySpan<Entity>(&entity, 1)); // Better but the loop can be skipped.

		public void DestroyEntity(ReadOnlySpan<Entity> entities)
		{
			int index = 0;

			while(index < entities.Length)
			{
				var entityBatchInChunk = this.entityStore->GetFirstEntityBatchInChunk(entities.Slice(index));

				index += entityBatchInChunk.count;

				if(entityBatchInChunk.chunk == null)
					continue;

				this.entityStore->DestroyEntityBatchInChunk(entityBatchInChunk);
			}

			entityStore->MarkIndicesAsFree(entities);
		}

		public bool IsAlive(Entity entity) => this.entityStore->Exists(entity);
	
		// TODO: Batched archetype change.

		public void ChangeEntityArchetype(Entity entity, EntityArchetype archetype)
		{
			Archetype* ptr = GetArchetypeInternal(archetype);

			if(ptr != this.entityStore->GetArchetype(entity))
				this.entityStore->MoveEntityToArchetype(entity, ptr);
		}

		public ReadOnlySpan<Entity> GetEntities(EntityArchetype archetype) // Buffered?
		{
			Archetype* ptr = GetArchetypeInternal(archetype);
			Span<Entity> entities = new Entity[ptr->entityCount];
			ChunkUtility.WriteEntitiesToBuffer(ptr->chunkArray->chunks, ptr->chunkArray->count, entities);

			return entities;
		}

		public ReadOnlySpan<Entity> GetEntities(EntityQuery query)
		{
			var queryData = GetUpdatedQueryData(query);
			var archetypes = queryData->archetypes;
			var archetypeCount = queryData->archetypeCount;
			var entityCount = 0;

			for(int i = 0; i < archetypeCount; ++i)
				entityCount += archetypes[i]->entityCount;

			int entitiesInBuffer = 0;
			Span<Entity> entities = new Entity[entityCount];

			for(int i = 0; i < archetypeCount; ++i)
			{
				var chunks = archetypes[i]->chunkArray->chunks;
				var chunkCount = archetypes[i]->chunkArray->count;
				entitiesInBuffer += ChunkUtility.WriteEntitiesToBuffer(chunks, chunkCount, entities.Slice(entitiesInBuffer));
			}

			return entities;
		}

		public int GetEntityCount(EntityQuery query)
		{
			var queryData = GetUpdatedQueryData(query);
			var count = 0;

			for(int i = 0; i < queryData->archetypeCount; ++i)
				count += queryData->archetypes[i]->entityCount;
				
			return count;
		}

		public int GetEntityCount(EntityArchetype archetype) =>
			GetArchetypeInternal(archetype)->entityCount;
	}
}