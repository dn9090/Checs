using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Threading;

namespace Checs
{
	internal static unsafe class ChunkUtility
	{
		public static Chunk* AllocateChunk()
		{
			Chunk* chunk = (Chunk*)MemoryUtility.Malloc(Chunk.ChunkSize);
			return chunk;
		}

		public static void ConstructChunk(Chunk* chunk, Archetype* archetype)
		{
			chunk->archetype = archetype;
			chunk->count = 0;
			chunk->capacity = archetype->chunkCapacity;
		}

		public static void AssignSequenceNumber(Chunk* chunk, ulong value)
		{
			chunk->sequenceNumber = value;
		}

		public static int CalculateChunkBufferCapacity(int blockSize)
		{
			int blockSizeWithEntity = blockSize + sizeof(Entity);
			return Chunk.BufferSize / blockSizeWithEntity;
		}
		
		public static void InitializeComponentData(Chunk* chunk, int index, int count)
		{
			var sizes = chunk->archetype->componentSizes;
			var offset = chunk->archetype->componentOffsets;
			var componentCount = chunk->archetype->componentCount;

			for(int i = 0; i < componentCount; ++i)
			{
				var offsetInBuffer = chunk->buffer + offset[i];

				// ZeroMemory would possibly be the better call, but it's internal.
				MemoryUtility.MemSet(offsetInBuffer + (sizes[i] * index), 0, sizes[i] * count);
			}
		}

		public static void CopyComponentData(Chunk* source, Chunk* destination, int sourceIndex, int destinationIndex)
		{
			// TODO: I've no clue what this is about...

			var types = source->archetype->componentTypes;
			var count = source->archetype->componentCount;
			var sizes = source->archetype->componentSizes;

			var sourceOffsets = source->archetype->componentOffsets;
			var destOffsets = destination->archetype->componentOffsets;

			for(int i = 0; i < count; ++i)
			{
				int indexInChunk = ArchetypeUtility.GetTypeIndex(destination->archetype, types[i]);

				if(indexInChunk == -1)
					continue;
			
				var offsetInSourceBuffer = source->buffer + sourceOffsets[i] + (sourceIndex * sizes[i]);
				var offsetInDestBuffer = destination->buffer + destOffsets[indexInChunk] + (destinationIndex * sizes[i]);

				Buffer.MemoryCopy(offsetInDestBuffer, offsetInSourceBuffer, (uint)sizes[indexInChunk], (uint)sizes[indexInChunk]);
				// Unsafe.CopyBlock((void*)offsetInDestBuffer, (void*)offsetInSourceBuffer, (uint)sizes[indexInChunk]);
			}
		}

		public static int AllocateEntities(Chunk* chunk, Entity* entities, int count)
		{
			var free = chunk->capacity - chunk->count;
			count = (free <= count) ? free : count;
	
			var index = chunk->count;
			var size = count * sizeof(Entity);
			
			// This is almost as fast (just as a reminder):
			// var buffer = new Span<Entity>(((Entity*)chunk->buffer) + index, count);
			// entities.Slice(0, count).CopyTo(buffer);

			// An SSE copy could be faster (not sure), because it avoids
			// the P/Invoke to memmove.
			Buffer.MemoryCopy(entities, ((Entity*)chunk->buffer) + index, size, size);

			InitializeComponentData(chunk, index, count);

			chunk->count += count;
			chunk->archetype->entityCount += count;

			return count;
		}

		public static Span<Entity> AllocateEntities_LEGACY(Chunk* chunk, int count)
		{
			int startIndex = chunk->count;
			int free = chunk->capacity - chunk->count;
			count = (free <= count) ? free : count;
	
			InitializeComponentData(chunk, startIndex, count);

			chunk->count += count;
			chunk->archetype->entityCount += count;

			Entity* buffer = (Entity*)chunk->buffer;
			return new Span<Entity>(&buffer[startIndex], count);
		}

		public static void PatchEntityData(Chunk* chunk, int index, int count)
		{
			// There is probably some better way to do this...

			//          index = 2  count = 3
			//                v-----|
			// chunk -> 0, 1, 2, 3, 4, 5, 6, 7, 8, 9
			//                               ^
			//              chunk->count - count = 7
			//
			//                      index = 6  count = 4
			//                            v--------|
			// chunk -> 0, 1, 2, 3, 4, 5, 6, 7, 8, 9
			//                            ^
			//              chunk->count - count = 6
			//
			//                  index = 5  count = 4
			//                         v--------|
			// chunk -> 0, 1, 2, 3, 4, 5, 6, 7, 8, 9
			//                            ^
			//              chunk->count - count = 6
			//

			var sizes = chunk->archetype->componentSizes;
			var offsets = chunk->archetype->componentOffsets;
			var componentCount = chunk->archetype->componentCount;

			var movableCount = chunk->count - (index + count);
			var patchCount = movableCount < count ? movableCount : count;
			var sourceIndex = chunk->count - patchCount;

			var source = chunk->buffer + (sourceIndex * sizeof(Entity));
			var dest = chunk->buffer + (index * sizeof(Entity));

			Unsafe.CopyBlock(dest, source, (uint)(patchCount * sizeof(Entity)));

			for(int i = 0; i < componentCount; ++i) // Vector?
			{
				var offsetInBuffer = chunk->buffer + offsets[i];
				source = offsetInBuffer + (sourceIndex * sizes[i]);
				dest = offsetInBuffer + (index * sizes[i]);
				Unsafe.CopyBlock(dest, source, (uint)(patchCount * sizes[i]));
			}

			chunk->count -= count;
			chunk->archetype->entityCount -= count;
		}

		public static Span<Entity> GetEntities(Chunk* chunk)
		{
			Entity* buffer = (Entity*)chunk->buffer;
			return new Span<Entity>(buffer, chunk->count);
		}

		public static int WriteEntitiesToBuffer(Chunk** chunks, int count, Span<Entity> buffer)
		{
			int position = 0;

			for(int i = 0; i < count; ++i)
			{
				var entities = GetEntities(chunks[i]);
				entities.CopyTo(buffer.Slice(position));
				position += entities.Length;
			}

			return position;
		}

		public static T* GetComponentPtrInBuffer<T>(Chunk* chunk, int componentIndex) where T : unmanaged
		{
			var offset = chunk->archetype->componentOffsets[componentIndex];
			var buffer = chunk->buffer;

			return (T*)(buffer + offset);
		}

		public static bool SetComponentData<T>(Chunk* chunk, int index, int typeIndex, T value) where T : unmanaged
		{
			var indexInArchetype = ArchetypeUtility.GetTypeIndex(chunk->archetype, typeIndex);

			if(indexInArchetype < 0)
				return false;

			T* data = GetComponentPtrInBuffer<T>(chunk, indexInArchetype);
			data[index] = value;

			return true;
		}

		public static bool TryGetComponentData<T>(Chunk* chunk, int index, int typeIndex, out T value) where T : unmanaged
		{
			var indexInArchetype = ArchetypeUtility.GetTypeIndex(chunk->archetype, typeIndex);

			if(indexInArchetype < 0)
			{
				value = default;
				return false;
			}
			
			value = GetComponentPtrInBuffer<T>(chunk, indexInArchetype)[index];
			return true;
		}

		public static T* GetComponentDataPtr<T>(Chunk* chunk, int index, int typeIndex) where T : unmanaged
		{
			var indexInArchetype = ArchetypeUtility.GetTypeIndex(chunk->archetype, typeIndex);

			if(indexInArchetype < 0)
				return null;
			
			return &GetComponentPtrInBuffer<T>(chunk, indexInArchetype)[index];
		}

		public static Span<T> GetComponentData<T>(Chunk* chunk, int typeIndex) where T : unmanaged
		{
			var indexInArchetype = ArchetypeUtility.GetTypeIndex(chunk->archetype, typeIndex);

			if(indexInArchetype < 0)
				return Span<T>.Empty;

			return new Span<T>(GetComponentPtrInBuffer<T>(chunk, indexInArchetype), chunk->count);
		}

		public static int CopyComponentData<T>(Chunk* chunk, int typeIndex, T* buffer, int count) where T : unmanaged
		{
			count = (count <= chunk->count) ? count : chunk->count;

			var indexInArchetype = ArchetypeUtility.GetTypeIndex(chunk->archetype, typeIndex); // Make sure that the type is in the archetype beforehand...
			var size = count * sizeof(T);

			Buffer.MemoryCopy(ChunkUtility.GetComponentPtrInBuffer<T>(chunk, indexInArchetype), buffer, size, size);

			return count;
		}
	}
}
