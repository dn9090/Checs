using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;

namespace Checs
{
	internal static unsafe class ChunkUtility
	{
		public static void ConstructChunk(Chunk* chunk, Archetype* archetype)
		{
			chunk->archetype     = archetype;
			chunk->capacity      = archetype->chunkCapacity;
			chunk->count         = 0;
			chunk->version       = 0;
		}

		public static int CalculateBufferCapacity(int* sizes, int count)
		{
			int blockSize = 0;

			for(int i = 0; i < count; ++i)
				blockSize += sizes[i];

			return Chunk.BufferSize / blockSize;
		}

		public static void ZeroComponentData(Chunk* chunk, int index, int count)
		{
			var componentCount   = chunk->archetype->componentCount;
			var componentSizes   = Archetype.GetComponentSizes(chunk->archetype);
			var componentOffsets = Archetype.GetComponentOffsets(chunk->archetype);

			for(int i = 1; i < componentCount; ++i)
			{
				var bufferOffset = chunk->buffer + componentOffsets[i] + componentSizes[i] * index;
				var byteCount    = componentSizes[i] * count;

				Unsafe.InitBlockUnaligned(bufferOffset, 0, (uint)byteCount);
			}
		}

		public static void ZeroComponentData(Chunk* chunk)
		{
			if(chunk->archetype->componentCount > 1)
			{
				var componentOffsets = Archetype.GetComponentOffsets(chunk->archetype);
				var bufferOffset     = chunk->buffer + componentOffsets[1];

				Unsafe.InitBlockUnaligned(bufferOffset, 0, (uint)(Chunk.BufferSize - componentOffsets[1]));
			}
		}

		public static void CloneComponentData(Chunk* srcChunk, int srcIndex, Chunk* dstChunk, int dstIndex, int count)
		{
			var componentCount   = srcChunk->archetype->componentCount;
			var componentSizes   = Archetype.GetComponentSizes(srcChunk->archetype);
			var componentOffsets = Archetype.GetComponentOffsets(srcChunk->archetype);

			for(var i = 1; i < componentCount; i++)
			{
				var src = srcChunk->buffer + (componentOffsets[i] + componentSizes[i] * srcIndex);
				var dst = dstChunk->buffer + (componentOffsets[i] + componentSizes[i] * dstIndex);

				CopyReplicate(dst, src, componentSizes[i], count);
			}
		}

		public static void Copy(Chunk* source, int sourceIndex, Chunk* dest, int destIndex, int count)
		{
			var componentCount   = source->archetype->componentCount;
			var componentSizes   = Archetype.GetComponentSizes(source->archetype);
			var componentOffsets = Archetype.GetComponentOffsets(source->archetype);

			for(var i = 0; i < componentCount; i++)
			{
				var componentSource = source->buffer + (componentOffsets[i] + componentSizes[i] * sourceIndex);
				var componentDest   = dest->buffer + (componentOffsets[i] + componentSizes[i] * destIndex);
				var byteCount = count * componentSizes[i];

				Unsafe.CopyBlockUnaligned(componentDest, componentSource, (uint)byteCount);
			}
		}

		public static void CopyReplicate(byte* destination, byte* source, int byteCount, int count)
		{
			if(byteCount == 1)
			{
				Unsafe.InitBlockUnaligned(destination, *source, (uint)count);
				return;
			}

			var absoluteByteCount = byteCount * count;
			var dst = destination;
			var offset = 0;

			do
			{
				if(offset == absoluteByteCount)
					return;

				offset += byteCount;

				var src = source;
				var remaining = byteCount;

				while((remaining & -4) != 0)
				{
					*((uint*)dst) = *((uint*)src);
					dst += 4;
					src += 4;
					remaining -= 4;
				}

				if((remaining & 2) != 0)
				{
					*((ushort*)dst) = *((ushort*)src);
					dst += 2;
					src += 2;
					remaining -= 2;
				}

				if((remaining & 1) != 0)
					*dst++ = *src;
				
			} while((offset & (2 * -Vector<byte>.Count)) == 0); // & -Vector<byte>.Count was 2x slower.
			
			var stopLoopAtOffset = absoluteByteCount - Vector<byte>.Count;
			var from = destination;

			while(offset <= stopLoopAtOffset)
			{
				Unsafe.WriteUnaligned(dst, Unsafe.ReadUnaligned<Vector<byte>>(from));

				offset += Vector<byte>.Count;
				from   += Vector<byte>.Count;
				dst    += Vector<byte>.Count;
			}

			var rep = (offset / byteCount) * byteCount; // Closest pattern end.

			if(offset != absoluteByteCount)
			{
				var repEnd = destination + rep - Vector<byte>.Count;
				var dstEnd = destination + stopLoopAtOffset;
				Unsafe.WriteUnaligned(dstEnd, Unsafe.ReadUnaligned<Vector<byte>>(repEnd));
			}
		}

		public static void CopyAndConvert(Chunk* source, int sourceIndex, Chunk* dest, int destIndex, int count)
		{
			var srcAt        = source->archetype->componentCount - 1;
			var srcHashCodes = Archetype.GetComponentHashCodes(source->archetype);
			var srcOffsets   = Archetype.GetComponentOffsets(source->archetype);

			var dstAt        = dest->archetype->componentCount - 1;
			var dstHashCodes = Archetype.GetComponentHashCodes(dest->archetype);
			var dstSizes     = Archetype.GetComponentSizes(dest->archetype);
			var dstOffsets   = Archetype.GetComponentOffsets(dest->archetype);
			
			// If the source or destination archetype is the empty archetype
			// only the entity data needs to be copied.
			if(srcAt == 0 || dstAt == 0)
			{
				CopyEntities(source, sourceIndex, dest, destIndex, count);
				ZeroComponentData(dest, destIndex, count);
				return;
			}

			while(dstAt >= 0)
			{
				if(srcHashCodes[srcAt] < dstHashCodes[dstAt])
				{
					var bufferOffset = dest->buffer + (dstOffsets[dstAt] + dstSizes[dstAt] * destIndex);
					var byteCount    = dstSizes[dstAt] * count;

					Unsafe.InitBlockUnaligned(bufferOffset, 0, (uint)byteCount);

					--dstAt;
					continue;
				}

				// This branch is always true if both indices point to the entity data,
				// which decrements both indices to less than zero and exits the loop.
				if(srcHashCodes[srcAt] == dstHashCodes[dstAt])
				{
					var componentSize   = dstSizes[dstAt];
					var componentSource = source->buffer + (srcOffsets[srcAt] + componentSize * sourceIndex);
					var componentDest   = dest->buffer + (dstOffsets[dstAt] + componentSize * destIndex);
					var byteCount = (uint)(count * componentSize);

					Unsafe.CopyBlockUnaligned(componentDest, componentSource, byteCount);

					--srcAt;
					--dstAt;
					continue;
				}

				--srcAt;
			}
		}

		public static void Merge(Chunk* from, Chunk* to)
		{
			var count = from->count;

			Copy(from, 0, to, to->count, count);

			from->count = 0;
			to->count  += count;
		}

		public static void ReserveEntities(Chunk* chunk, int count)
		{
			chunk->count += count;
			chunk->archetype->entityCount += count;
		}

		public static void MoveEntities(Chunk* source, int sourceIndex, Chunk* dest, int destIndex, int count)
		{
			CopyAndConvert(source, sourceIndex, dest, destIndex, count);

			dest->count += count;
			dest->archetype->entityCount += count;
		}

		public static int PatchEntities(Chunk* chunk, int index, int count)
		{
			var moveCount = chunk->count - (index + count);  // TODO: Naming
			var patchCount = moveCount < count ? moveCount : count;

			if(patchCount > 0)
				Copy(chunk, chunk->count - patchCount, chunk, index, patchCount);

			chunk->count -= count;
			chunk->archetype->entityCount -= count;

			return patchCount;
		}

		public static void CopyEntities(Chunk* source, int sourceIndex, Chunk* dst, int destIndex, int count)
		{
			var srcEntities = source->buffer + (sourceIndex * sizeof(Entity));
			var dstEntities = dst->buffer + (destIndex * sizeof(Entity));
			var byteCount = count * sizeof(Entity);

			Unsafe.CopyBlockUnaligned(dstEntities, srcEntities, (uint)byteCount);
		}

		public static void CopyEntities(Chunk* chunk, int index, Entity* entities, int count)
		{
			var srcEntities = (Entity*)chunk->buffer + index;
			var byteCount = sizeof(Entity) * count;

			Unsafe.CopyBlockUnaligned(entities, srcEntities, (uint)byteCount);
		}

		public static int CopyEntities(Chunk* chunk, Entity* entities, int count)
		{
			count = count <= chunk->count ? count : chunk->count;
			var byteCount = sizeof(Entity) * count;

			Unsafe.CopyBlockUnaligned(entities, (Entity*)chunk->buffer, (uint)byteCount);

			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Entity* GetEntities(Chunk* chunk, int index)
		{
			return (Entity*)chunk->buffer + index;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T* GetComponentDataPtr<T>(Chunk* chunk, int componentIndex) where T : unmanaged
		{
			var componentOffsets = Archetype.GetComponentOffsets(chunk->archetype);
			var buffer = chunk->buffer;

			return (T*)(buffer + componentOffsets[componentIndex]);
		}
		
		public static byte* GetComponentDataPtr(Chunk* chunk, int index, int componentIndex)
		{
			var componentSizes   = Archetype.GetComponentSizes(chunk->archetype);
			var componentOffsets = Archetype.GetComponentOffsets(chunk->archetype);
			var buffer = chunk->buffer;
			
			return buffer + componentOffsets[componentIndex] + (index * componentSizes[componentIndex]);
		}

		public static int GetComponentDataOffset(Chunk* chunk, int componentIndex, int index)
		{
			var componentSizes   = Archetype.GetComponentSizes(chunk->archetype);
			var componentOffsets = Archetype.GetComponentOffsets(chunk->archetype);

			return componentOffsets[componentIndex] + (index * componentSizes[componentIndex]);
		}

		public static int CopyComponentData<T>(Chunk* chunk, int componentIndex, T* buffer, int count) where T : unmanaged
		{
			count = count <= chunk->count ? count : chunk->count;
			
			var ptr = ChunkUtility.GetComponentDataPtr<T>(chunk, componentIndex);
			var byteCount = count * sizeof(T);

			// Buffer.MemoryCopy is required here because Unsafe.BlockCopy is not handling overlapping
			// source and destination addresses (which is possible when the buffer points to the same chunk).
			Buffer.MemoryCopy(ptr, buffer, byteCount, byteCount);

			return count;
		}

		public static void RepeatComponentData<T>(Chunk* chunk, int index, int count, int componentIndex, in T value) where T : unmanaged
		{
			var ptr = ChunkUtility.GetComponentDataPtr<T>(chunk, componentIndex) + index;
			var temp = value;

			if(sizeof(T) == 1)
			{
				Unsafe.InitBlockUnaligned(ptr, *((byte*)&temp), (uint)count);
				return;
			}

			if(count >= 4)
			{
				do
				{
					*(ptr + 0) = temp;
					*(ptr + 1) = temp;
					*(ptr + 2) = temp;
					*(ptr + 3) = temp;
					ptr += 4;
					count -= 4;
				} while (count >= 4);
			}

			while(count-- > 0)
				*ptr++ = temp;
		}

		public static void RepeatComponentData(Chunk* chunk, int index, int count, int componentIndex, byte* source, int byteCount)
		{
			var ptr = ChunkUtility.GetComponentDataPtr(chunk, index, componentIndex);
			ChunkUtility.CopyReplicate(ptr, source, byteCount, count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void IncrementVersion(Chunk* chunk)
		{
			chunk->version = ++chunk->archetype->chunkVersion;
		}

		public static void MarkAsChanged(Chunk* chunk, int componentIndex)
		{
			// FIXME: At the moment, adding entities is a destructive operation
			//        for change versions:
			//        ChunkA = 0, ChunkB = 0    Entity = 0, CompA = 0, CompB = 0
			//        -------> GetChangeVersion = 0
			//        ChunkA = 0, ChunkB = 1    Entity = 1, CompA = 0, CompB = 0
			//        ChunkA = 2, ChunkB = 1    Entity = 1, CompA = 0, CompB = 2
			//        -------> DidChange(ChunkA) returns true but should return false.
			
			var versions = Archetype.GetComponentVersions(chunk->archetype);
			var changeVersion = chunk->archetype->changeVersion.Increment();

			chunk->changeVersion     = changeVersion;
			versions[componentIndex] = changeVersion;
		}
	}
}
