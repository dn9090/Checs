using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct EntityStore : IDisposable
	{
		private const int DefaultCapacity = 64;

		public int version;

		public int capacity;

		public int count;

		public EntityInChunk* entitiesInChunk;

		public int* entityVersions;

		public FreeEntitySlotList freeSlots;

		public static void Construct(EntityStore* store)
		{
			store->version = 1; // To avoid that the default entity is valid.
			store->capacity = DefaultCapacity;
			store->count = 0;
			store->entitiesInChunk = MemoryUtility.Malloc<EntityInChunk>(store->capacity);
			store->entityVersions = MemoryUtility.Malloc<int>(store->capacity);
			store->freeSlots = FreeEntitySlotList.Empty();
		}

		public void EnsureCapacity(int count)
		{
			int requiredCapacity = this.count + count;

			if(requiredCapacity > this.capacity)
			{
				this.capacity = MemoryUtility.RoundToPowerOfTwo(requiredCapacity);
				this.entitiesInChunk = MemoryUtility.Realloc<EntityInChunk>(this.entitiesInChunk, this.capacity);
				this.entityVersions = MemoryUtility.Realloc<int>(this.entityVersions, this.capacity);
			}
		}

		public bool Exists(Entity entity) => this.entityVersions[entity.index] == entity.version;

		public void MoveEntityToArchetype(Entity entity, Archetype* archetype)
		{
			var entityInChunk = this.entitiesInChunk[entity.index];
			var chunkIndex = 0;

			Chunk* chunk = archetype->chunkArray->GetChunkWithEmptySlots(ref chunkIndex);
			int indexInChunk = chunk->count;

			Span<Entity> allocated = ChunkUtility.AllocateEntities(chunk, 1);
			allocated[0] = entity;

			ChunkUtility.CopyComponentData(entityInChunk.chunk, chunk, entityInChunk.index, indexInChunk);
			ChunkUtility.PatchEntityData(entityInChunk.chunk, entityInChunk.index, 1);

			this.entitiesInChunk[entity.index] = new EntityInChunk(chunk, indexInChunk);
		}

		public Archetype* GetArchetype(Entity entity) => GetChunk(entity)->archetype;

		public Chunk* GetChunk(Entity entity) => this.entitiesInChunk[entity.index].chunk;

		public void UpdateEntityInChunk(Entity entity, Chunk* chunk, int indexInChunk)
		{
			this.entitiesInChunk[entity.index] = new EntityInChunk(chunk, indexInChunk);
		}

		public void ReserveEntityBatch(Span<Entity> buffer)
		{
			Span<int> slots = this.freeSlots.Recycle(buffer.Length);

			for(int i = 0; i < slots.Length; ++i)
			{
				int index = slots[i];
				buffer[i] = new Entity(index, this.version);
				this.entityVersions[index] = this.version;
			}

			for(int i = slots.Length; i < buffer.Length; ++i)
			{
				int index = this.count++;
				buffer[i] = new Entity(index, this.version);
				this.entityVersions[index] = this.version;
			}
		}

		public EntityBatchInChunk GetFirstEntityBatchInChunk(ReadOnlySpan<Entity> entities)
		{
			Entity baseEntity = entities[0];
			var entityInChunk = this.entitiesInChunk[baseEntity.index];

			Chunk* chunk = this.entityVersions[baseEntity.index] == baseEntity.version
				? entityInChunk.chunk
				: null;
			int indexInChunk = entityInChunk.index;
			int count = 1;

			for(; count < entities.Length; ++count)
			{
				Entity entity = entities[count];
				Chunk* entityChunk = this.entitiesInChunk[entity.index].chunk;
				int entityIndexInChunk = this.entitiesInChunk[entity.index].index;

				if(this.entityVersions[entity.index] == entity.version)
				{
					if(entityChunk != chunk || entityIndexInChunk != (indexInChunk + count))
						break;
				} else {
					if(chunk != null)
						break;
				}
			}

			return new EntityBatchInChunk(chunk, indexInChunk, count);
		}

		public void DestroyEntityBatchInChunk(EntityBatchInChunk entityBatchInChunk)
		{
			var entities = ChunkUtility.GetEntities(entityBatchInChunk.chunk)
				.Slice(entityBatchInChunk.index, entityBatchInChunk.count);
			
			var lastCount = this.count;
			
			for(int i = entities.Length - 1; i >= 0; --i)
			{
				if(entities[i].index != this.count - 1)
					break;
				
				--this.count;
				this.entityVersions[entities[i].index] = -1;
			}

			Span<int> slots = this.freeSlots.Allocate(entities.Length - (lastCount - this.count));

			for(int i = 0; i < slots.Length; ++i)
			{
				slots[i] = entities[i].index;
				this.entityVersions[entities[i].index] = -1;
			}

			//this.count -= slots.Length;

			ChunkUtility.PatchEntityData(entityBatchInChunk.chunk, entityBatchInChunk.index, entityBatchInChunk.count);

			// TODO: This thing needs to be more efficent.
			var movedEntities = ChunkUtility.GetEntities(entityBatchInChunk.chunk);

			if(entityBatchInChunk.index < movedEntities.Length) // Copied from end?
			{
				movedEntities = movedEntities.Slice(entityBatchInChunk.index, Math.Min(movedEntities.Length, entityBatchInChunk.count));

				for(int i = 0; i < movedEntities.Length; ++i)
					UpdateEntityInChunk(movedEntities[i], entityBatchInChunk.chunk, entityBatchInChunk.index + i);
			}
			
			++this.version;
		}

		public void Dispose()
		{
			this.count = 0;
			this.capacity = 0;
			this.freeSlots.Dispose();
			MemoryUtility.Free<EntityInChunk>(this.entitiesInChunk);
			MemoryUtility.Free<int>(this.entityVersions);
		}
	}
}