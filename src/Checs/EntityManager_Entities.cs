using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	// TODO: Block structural changes in query.

	public unsafe partial class EntityManager
	{
		public ReadOnlySpan<Entity> CreateEntity(int count) =>
			CreateEntity(CreateArchetype(), count);

		public ReadOnlySpan<Entity> CreateEntity(EntityArchetype archetype, int count) =>
			CreateEntityInternal(GetArchetypeInternal(archetype), count);

		internal ReadOnlySpan<Entity> CreateEntityInternal(Archetype* archetype, int count)
		{
			this.entityStore->EnsureCapacity(count);
			
			Span<Entity> entities = new Entity[count];
			this.entityStore->ReserveEntityBatch(entities);

			int allocatedEntityCount = 0;
			int chunkIndexInArray = 0;

			while(allocatedEntityCount < count)
			{
				Chunk* chunk = archetype->chunkArray->GetChunkWithEmptySlots(ref chunkIndexInArray);
				int chunkCount = chunk->count;

				Span<Entity> entitiesInChunk = ChunkUtility.AllocateEntities(chunk, count - allocatedEntityCount);

				for(int i = 0; i < entitiesInChunk.Length; ++i)
				{
					Entity entity = entities[allocatedEntityCount++];
					entitiesInChunk[i] = entity;
					this.entityStore->UpdateEntityInChunk(entity, chunk, chunkCount + i);
				}
			}
	
			return entities;
		}

		public void DestroyEntity(Entity entity)
		{
			Span<Entity> entities = stackalloc Entity[] { entity };
			DestroyEntity(entities);
		}

		public void DestroyEntity(ReadOnlySpan<Entity> entities)
		{
			int index = 0;

			while(index < entities.Length)
			{
				var entityBatchInChunk = this.entityStore->GetFirstEntityBatchInChunk(entities.Slice(index));

				index += entityBatchInChunk.count;

				if(entityBatchInChunk.chunk == null || entityBatchInChunk.count == 0)
					continue;

				this.entityStore->DestroyEntityBatchInChunk(entityBatchInChunk);
			}
		}

		public bool IsAlive(Entity entity) => this.entityStore->Exists(entity);
	
		// TODO: Batched archetype change.

		public void ChangeEntityArchetype(Entity entity, EntityArchetype archetype)
		{
			Archetype* ptr = GetArchetypeInternal(archetype);

			if(ptr != this.entityStore->GetArchetype(entity))
				this.entityStore->MoveEntityToArchetype(entity, ptr);
		}

		public ReadOnlySpan<Entity> GetEntities(EntityArchetype archetype)
		{
			Archetype* ptr = GetArchetypeInternal(archetype);
			Span<Entity> entities = new Entity[ptr->entityCount];
			ChunkUtility.WriteEntitiesToBuffer(ptr->chunkArray->chunks, ptr->chunkArray->count, entities);

			return entities;
		}

		public ReadOnlySpan<Entity> GetEntities(EntityQuery query)
		{
			int entityCount = 0;
			
			var queryData = GetUpdatedQueryData(query);
			var count = queryData->archetypeCount;
			var archetypes = queryData->archetypes;

			for(int i = 0; i < count; ++i)
				entityCount += archetypes[i]->entityCount;

			int entitiesInBuffer = 0;
			Span<Entity> entities = new Entity[entityCount];

			for(int i = 0; i < count; ++i)
			{
				var chunks = archetypes[i]->chunkArray->chunks;
				var chunkCount = archetypes[i]->chunkArray->count;
				entitiesInBuffer += ChunkUtility.WriteEntitiesToBuffer(chunks, chunkCount, entities.Slice(entitiesInBuffer));
			}

			return entities;
		}
	}
}