//#define CHECS_DISABLE_SSE
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct EntityStore : IDisposable
	{
		private const int DefaultCapacity = 64;

		public int aliveCount => count - freeSlots.count; // Rename

		public uint version;

		public int capacity;

		public int count;

		public EntityInChunk* entitiesInChunk;

		public FreeEntitySlotList freeSlots;

		public static void Construct(EntityStore* store)
		{
			store->version = 1;
			store->capacity = DefaultCapacity;
			store->count = 0;
			store->entitiesInChunk = MemoryUtility.Malloc<EntityInChunk>(store->capacity);
			store->freeSlots = FreeEntitySlotList.Empty();

			Unsafe.InitBlock(store->entitiesInChunk, 0, (uint)(sizeof(EntityInChunk) * store->capacity));
		}

		public void EnsureCapacity(int count)
		{
			int requiredCapacity = this.count + count;

			if(requiredCapacity > this.capacity)
			{
				this.capacity = MemoryUtility.RoundToPowerOfTwo(requiredCapacity);
				this.entitiesInChunk = MemoryUtility.Realloc<EntityInChunk>(this.entitiesInChunk, this.capacity);
			}
		}

		public bool Exists(Entity entity)
		{
			var entityInChunk = this.entitiesInChunk[entity.index];
			return entityInChunk.version == entity.version && entityInChunk.chunk != null;
		}

		public void MoveEntityToArchetype(Entity entity, Archetype* archetype)
		{
			var entityInChunk = this.entitiesInChunk[entity.index];
			var chunkIndex = 0;

			Chunk* chunk = archetype->chunkArray->GetChunkWithEmptySlots(ref chunkIndex);
			int indexInChunk = chunk->count;

			Span<Entity> allocated = ChunkUtility.AllocateEntities_LEGACY(chunk, 1);
			allocated[0] = entity;

			ChunkUtility.CopyComponentData(entityInChunk.chunk, chunk, entityInChunk.index, indexInChunk);
			ChunkUtility.PatchEntityData(entityInChunk.chunk, entityInChunk.index, 1);

			this.entitiesInChunk[entity.index] = new EntityInChunk(chunk, indexInChunk, entityInChunk.version);
		}

		public Archetype* GetArchetype(Entity entity) => GetChunk(entity)->archetype;

		public Chunk* GetChunk(Entity entity) => this.entitiesInChunk[entity.index].chunk;

#if CHECS_DISABLE_SSE
		public void ReserveEntityBatch(Span<Entity> buffer)
		{
			Span<int> slots = this.freeSlots.Recycle(buffer.Length);

			for(int i = 0; i < slots.Length; ++i)
				buffer[i] = new Entity(slots[i], this.version);

			for(int i = slots.Length; i < buffer.Length; ++i)
				buffer[i] = new Entity(this.count++, this.version);
		}
#else
		public void ReserveEntityBatch(Entity* buffer, int count)
		{
			Span<int> slots = this.freeSlots.Recycle(count);

			for(int i = 0; i < slots.Length; ++i)
				buffer[i] = new Entity(slots[i], this.version);

			var index = slots.Length;

			/*if(Sse2.IsSupported)
			{
				Vector128<uint> data = Vector128.Create((uint)this.count, this.version, (uint)this.count + 1, this.version);
				Vector128<uint> elem = Vector128.Create((uint)2, (uint)0, (uint)2, (uint)0);

				for(; index < count - 1; index += 2)
				{
					Sse2.Store((uint*)(buffer + index), data);
					data = Sse2.Add(data, elem);
				}

				this.count += (index - slots.Length);
			}*/

			while(index < count)
				buffer[index++] = new Entity(this.count++, this.version);
		}

		public void ReserveEntityBatch(Span<Entity> buffer)
		{
			Span<int> slots = this.freeSlots.Recycle(buffer.Length);

			for(int i = 0; i < slots.Length; ++i)
				buffer[i] = new Entity(slots[i], this.version);

			var index = slots.Length;

			if(Sse2.IsSupported)
			{
				Vector128<uint> data = Vector128.Create((uint)this.count, this.version, (uint)this.count + 1, this.version);
				Vector128<uint> elem = Vector128.Create((uint)2, (uint)0, (uint)2, (uint)0);

				fixed(Entity* ptr = buffer)
				{
					for(; index < buffer.Length - 1; index += 2)
					{
						Sse2.Store((uint*)(ptr + index), data);
						data = Sse2.Add(data, elem);
					}
				}

				this.count += (index - slots.Length);
			}

			while(index < buffer.Length)
				buffer[index++] = new Entity(this.count++, this.version);
		}
#endif

		public EntityBatchInChunk GetFirstEntityBatchInChunk(ReadOnlySpan<Entity> entities)
		{
			var baseEntity = entities[0];
			var baseEntityInChunk = this.entitiesInChunk[baseEntity.index];

			Chunk* chunk = baseEntityInChunk.version == baseEntity.version
				? baseEntityInChunk.chunk
				: null;
			int indexInChunk = baseEntityInChunk.index;
			int count = 1;

			for(; count < entities.Length; ++count) // Vector???
			{
				var entity = entities[count];
				var entityInChunk = this.entitiesInChunk[entity.index];

				Chunk* entityChunk = entityInChunk.chunk;
				int entityIndexInChunk = entityInChunk.index;

				if(entityInChunk.version == entity.version)
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
			ChunkUtility.PatchEntityData(entityBatchInChunk.chunk, entityBatchInChunk.index, entityBatchInChunk.count);

			// TODO: This thing needs to be more efficent.
			var movedEntities = ChunkUtility.GetEntities(entityBatchInChunk.chunk);

			if(entityBatchInChunk.index < movedEntities.Length) // Copied from end?
			{
				movedEntities = movedEntities.Slice(entityBatchInChunk.index, Math.Min(movedEntities.Length, entityBatchInChunk.count));

				for(int i = 0; i < movedEntities.Length; ++i)
				{
					var entityIndex = movedEntities[i].index;
					this.entitiesInChunk[entityIndex].chunk = entityBatchInChunk.chunk;
					this.entitiesInChunk[entityIndex].index = entityBatchInChunk.index + i;
				}
			}

			// TODO: Free chunks if chunk->count == 0 after patching entities
			
			++this.version;
		}

		public void MarkIndicesAsFree(ReadOnlySpan<Entity> entities)
		{
			// What happens if entities are deleted from holes that are in the slot list?

			// Blockwise from end, decrease entity count.
			var blockEnd = count;

			for(int i = entities.Length - 1; i >= 0; --i)
			{
				if(entities[i].index != (this.count - 1))
					break;
				--this.count;
			}

			var blockCount = blockEnd - this.count;
			Unsafe.InitBlock(this.entitiesInChunk + this.count, 0, (uint)(sizeof(EntityInChunk) * blockCount));

			// Recycle inner indices.
			Span<int> slots = this.freeSlots.Allocate(entities.Length - blockCount);

			for(int i = 0; i < slots.Length; ++i)
			{
				slots[i] = entities[i].index;
				this.entitiesInChunk[entities[i].index].version = 0;
			}
		}

		public void Dispose()
		{
			this.count = 0;
			this.capacity = 0;
			this.freeSlots.Dispose();
			MemoryUtility.Free<EntityInChunk>(this.entitiesInChunk);
		}
	}
}