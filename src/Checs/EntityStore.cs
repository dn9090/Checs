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

		public uint version;

		public EntityInChunk* entitiesInChunk;

		public EntityStore(int initialCapacity)
		{
			this.capacity = initialCapacity;
			this.count = 0;
			this.reserved = 0;
			this.nextFreeIndex = -1;
			this.entitiesInChunk = (EntityInChunk*)Allocator.Calloc(sizeof(EntityInChunk) * this.capacity);
			this.version = 1;
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
			ChunkUtility.IncrementVersion(chunk);

			var entities = ChunkUtility.GetEntities(chunk, 0); // TODO: Replace with startIndex
			var registeredCount = 0;
			var freeIndex = this.nextFreeIndex;
	
			while(freeIndex >= 0 && registeredCount < count)
			{
				var index        = freeIndex;
				var indexInChunk = startIndex + registeredCount++;

				freeIndex = this.entitiesInChunk[index].index;

				this.entitiesInChunk[index] = new EntityInChunk(chunk, indexInChunk, this.version);
				entities[indexInChunk]      = new Entity(index, this.version);
			}

			this.nextFreeIndex = freeIndex;

			while(registeredCount < count)
			{
				var index        = this.reserved++;
				var indexInChunk = startIndex + registeredCount++;

				this.entitiesInChunk[index] = new EntityInChunk(chunk, indexInChunk, this.version);
				entities[indexInChunk]      = new Entity(index, this.version);
			}

			this.count += count;
		}

		public void Unregister(Chunk* chunk, int startIndex, int count)
		{
			// A unregister call needs to be followed by a update call
			// to ensure that the version of the chunk is incremented.

			++this.version;

			var entities = ChunkUtility.GetEntities(chunk, startIndex);
			var freeIndex = this.nextFreeIndex;

			for(int i = count - 1; i >= 0; --i)
			{
				this.entitiesInChunk[entities[i].index].index   = freeIndex;
				this.entitiesInChunk[entities[i].index].version = 0;
				
				freeIndex = entities[i].index;
			}

			this.nextFreeIndex = freeIndex;
			this.count -= count;
		}

		public void Update(Chunk* chunk, int startIndex, int count)
		{
			ChunkUtility.IncrementVersion(chunk);

			var entities = ChunkUtility.GetEntities(chunk, startIndex);
			
			for(int i = 0; i < count; ++i)
			{
				this.entitiesInChunk[entities[i].index].index = startIndex + i;
				this.entitiesInChunk[entities[i].index].chunk = chunk;
			}
		}
		
		public void Dispose()
		{
			Allocator.Free(this.entitiesInChunk);

			this.entitiesInChunk = null;
			this.count           = 0;
			this.reserved        = 0;
			this.capacity        = 0;
		}
	}
}