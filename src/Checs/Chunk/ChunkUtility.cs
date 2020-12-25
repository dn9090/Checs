using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
				MemoryUtility.MemSet(offsetInBuffer + (sizes[i] * index), 0, sizes[i] * count);
			}
		}

		public static void CopyComponentData(Chunk* source, Chunk* destination, int sourceIndex, int destinationIndex)
		{
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

				Unsafe.CopyBlock((void*)offsetInDestBuffer, (void*)offsetInSourceBuffer, (uint)sizes[indexInChunk]);
			}
		}

		public static Span<Entity> AllocateEntities(Chunk* chunk, int count)
		{
			int startIndex = chunk->count;
			int free = chunk->capacity - chunk->count;
			count = Math.Min(free, count);
	
			InitializeComponentData(chunk, startIndex, count);

			chunk->count += count;
			chunk->archetype->entityCount += count;

			Entity* buffer = (Entity*)chunk->buffer;
			return new Span<Entity>(&buffer[startIndex], count);
		}

		public static void PatchEntityData(Chunk* chunk, int index, int count)
		{
			var sizes = chunk->archetype->componentSizes;
			var offsets = chunk->archetype->componentOffsets;
			var componentCount = chunk->archetype->componentCount;

			var startIndex = chunk->count - count;

			var source = chunk->buffer + (startIndex * sizeof(Entity));
			var dest = chunk->buffer + (index * sizeof(Entity));

			Unsafe.CopyBlock((void*)dest, (void*)source, (uint)(count * sizeof(Entity)));

			for(int i = 0; i < componentCount; ++i)
			{
				var offsetInBuffer = chunk->buffer + offsets[i];
				source = offsetInBuffer + (startIndex * sizes[i]);
				dest = offsetInBuffer + (index * sizes[i]);
				Unsafe.CopyBlock((void*)dest, (void*)source, (uint)(count * sizes[i]));
			}

			chunk->count -= count;
			chunk->archetype->entityCount -= count;
		}

		/*
		public static void PatchEntityData(Chunk* chunk, int index, int count)
		{
			int startIndex = index + count;

			var sizes = chunk->archetype->componentSizes;
			var offsets = chunk->archetype->componentOffsets;
			var componentCount = chunk->archetype->componentCount;

			var source = chunk->buffer + (startIndex * sizeof(Entity));
			var dest = chunk->buffer + (index * sizeof(Entity));

			//Unsafe.CopyBlock((void*)dest, (void*)source, (uint)((chunk->count - startIndex) * sizeof(Entity)));
			Unsafe.CopyBlock((void*)dest, (void*)source, (uint)(count * sizeof(Entity)));

			for(int i = 0; i < componentCount; ++i)
			{
				var offsetInBuffer = chunk->buffer + offsets[i];
				source = offsetInBuffer + (startIndex * sizes[i]);
				dest = offsetInBuffer + (index * sizes[i]);
				Unsafe.CopyBlock((void*)dest, (void*)source, (uint)(count * sizes[i]));
			}

			chunk->count -= count;
			chunk->archetype->entityCount -= count;
		}
		*/

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
	}
}
