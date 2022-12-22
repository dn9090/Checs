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
		public int capacity;

		public int count;

		public int reserved;

		public int nextFreeIndex;

		public EntityInChunk* entitiesInChunk;

		public ChangeVersion* changeVersion;

		public EntityStore(int initialCapacity)
		{
			this.capacity = initialCapacity;
			this.count = 0;
			this.reserved = 0;
			this.nextFreeIndex = -1;
			this.entitiesInChunk = (EntityInChunk*)Allocator.Calloc(sizeof(EntityInChunk) * this.capacity);
			this.changeVersion = (ChangeVersion*)Allocator.Alloc(sizeof(ChangeVersion));
			this.changeVersion->Reset();
		}

		public void EnsureCapacity(int requestedCapacity)
		{
			if(requestedCapacity < 0)
				throw new ArgumentOutOfRangeException(nameof(requestedCapacity));

			var requiredCapacity = this.reserved + requestedCapacity;

			if(requiredCapacity > this.capacity) // TODO: Cap max capacity.
			{
				this.capacity = Allocator.RoundToPowerOfTwo(requiredCapacity);
				this.entitiesInChunk = (EntityInChunk*)Allocator.Realloc(this.entitiesInChunk, sizeof(EntityInChunk) * this.capacity);
			}
		}

		public void Register(Chunk* chunk, int startIndex, int count)
		{
			ChunkUtility.IncrementStructuralVersion(chunk);

			var entities = ChunkUtility.GetEntities(chunk, 0);
			var version = this.changeVersion->entityVersion;
			var registeredCount = 0;
	
			while(this.nextFreeIndex >= 0 && registeredCount < count)
			{
				var index = this.nextFreeIndex;
				this.nextFreeIndex = this.entitiesInChunk[this.nextFreeIndex].index;

				var indexInChunk = startIndex + registeredCount++; 

				this.entitiesInChunk[index] = new EntityInChunk(chunk, indexInChunk, version);
				entities[indexInChunk] = new Entity(index, version);
			}

			while(registeredCount < count)
			{
				var index = this.reserved++;
				var indexInChunk = startIndex + registeredCount++;

				this.entitiesInChunk[index] = new EntityInChunk(chunk, indexInChunk, version);
				entities[indexInChunk] = new Entity(index, version);
			}

			this.count += count;
		}

		public void Unregister(Chunk* chunk, int startIndex, int count)
		{
			ChunkUtility.IncrementStructuralVersion(chunk);
			++this.changeVersion->entityVersion;

			var entities = ChunkUtility.GetEntities(chunk, startIndex);
			var freeIndex = this.nextFreeIndex;

			for(int i = count - 1; i >= 0; --i)
			{
				this.entitiesInChunk[entities[i].index].index = freeIndex;
				this.entitiesInChunk[entities[i].index].version = 0;
				freeIndex = entities[i].index;
			}

			this.nextFreeIndex = freeIndex;
			this.count -= count;
		}

		public void Update(Chunk* chunk, int startIndex, int count)
		{
			ChunkUtility.IncrementStructuralVersion(chunk);
			
			var entities = ChunkUtility.GetEntities(chunk, startIndex);
			
			for(int i = 0; i < count; ++i)
			{
				this.entitiesInChunk[entities[i].index].index = startIndex + i;
				this.entitiesInChunk[entities[i].index].chunk = chunk;
			}
		}
		
		public void Dispose()
		{
			this.count = 0;
			this.reserved = 0;
			this.capacity = 0;
			Allocator.Free(this.entitiesInChunk);
			Allocator.Free(this.changeVersion);
		}
	}
}