using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	internal static unsafe class ArchetypeUtility
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint GetHashCode(uint* hashCodes, int count)
		{
			// First bit is always zero to avoid collisions with queries.
			return xxHash.GetHashCode(hashCodes, count) & 0x7FFFFFFF;
		}

		/*[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CalculateComponentOffsets(int* sizes, int* offsets, int count, int chunkCapacity)
		{
			offsets[0] = 0;

			for(int i = 1; i < count; ++i)
				offsets[i] = offsets[i - 1] + (chunkCapacity * sizes[i - 1]);
		}*/

		public static int GetComponentIndex(Archetype* archetype, uint hashCode)
		{
			var hashCodes = Archetype.GetComponentHashCodes(archetype);
			var count = archetype->componentCount;

			return GetComponentIndex(hashCodes, count, hashCode);
		}

		public static int GetComponentIndex(Archetype* archetype, uint hashCode, int cachedComponentIndex)
		{
			var hashCodes = Archetype.GetComponentHashCodes(archetype);
			var count = archetype->componentCount;

			if((uint)cachedComponentIndex < (uint)count
				&& hashCodes[cachedComponentIndex] == hashCode)
				return cachedComponentIndex;

			return GetComponentIndex(hashCodes, count, hashCode);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetComponentIndex(uint* hashCodes, int count, uint hashCode)
		{
			for(int i = 0; i < count; ++i)
			{
				if(hashCodes[i] == hashCode)
					return i;
			}

			return -1;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CalculateFreeChunkSlots(Archetype* archetype)
		{
			var capacity = archetype->chunkCapacity * archetype->chunkList.count;
			return capacity - archetype->entityCount;
		}
		
		public static void Copy(Archetype* archetype, uint* hashCodes, int* sizes)
		{
			var componentHashCodes = Archetype.GetComponentHashCodes(archetype);
			var componentSizes     = Archetype.GetComponentSizes(archetype);
			var byteCount = archetype->componentCount * sizeof(int);
		
			Unsafe.CopyBlockUnaligned(hashCodes, componentHashCodes, (uint)byteCount);
			Unsafe.CopyBlockUnaligned(sizes, componentSizes, (uint)byteCount);
		}

		public static void CopyInsert(Archetype* archetype, uint* dstHashCodes, int* dstSizes, uint hashCode, int size)
		{
			var hashCodes = Archetype.GetComponentHashCodes(archetype);
			var sizes     = Archetype.GetComponentSizes(archetype);

			var count = archetype->componentCount;
			var index = 0;

			while(index < count && hashCode > hashCodes[index])
				++index;
			
			Unsafe.CopyBlockUnaligned(dstHashCodes, hashCodes, (uint)(sizeof(uint) * index));
			Unsafe.CopyBlockUnaligned(dstSizes, sizes, (uint)(sizeof(int) * index));

			dstHashCodes[index] = hashCode;
			dstSizes[index]     = size;
			
			var startIndex = index + 1;
			var remaining = count - index;

			Unsafe.CopyBlockUnaligned(dstHashCodes + startIndex, hashCodes + index, (uint)(sizeof(uint) * remaining));
			Unsafe.CopyBlockUnaligned(dstSizes + startIndex, sizes + index, (uint)(sizeof(int) * remaining));
		}

		public static void CopyRemoveAt(Archetype* archetype, uint* dstHashCodes, int* dstSizes, int componentIndex)
		{
			var hashCodes = Archetype.GetComponentHashCodes(archetype);
			var sizes     = Archetype.GetComponentSizes(archetype);

			var count = archetype->componentCount;
			
			Unsafe.CopyBlockUnaligned(dstHashCodes, hashCodes, (uint)(sizeof(uint) * componentIndex));
			Unsafe.CopyBlockUnaligned(dstSizes, sizes, (uint)(sizeof(int) * componentIndex));
			
			var startIndex = componentIndex + 1;
			var remaining = count - componentIndex;

			Unsafe.CopyBlockUnaligned(dstHashCodes + componentIndex, hashCodes + startIndex, (uint)(sizeof(uint) * remaining));
			Unsafe.CopyBlockUnaligned(dstSizes + componentIndex, sizes + startIndex, (uint)(sizeof(int) * remaining));
		}

		public static int Union(Archetype* lhs, Archetype* rhs, uint* hashCodes, int* sizes)
		{
			var lhsHashCodes = Archetype.GetComponentHashCodes(lhs);
			var lhsSizes     = Archetype.GetComponentSizes(lhs);

			var rhsHashCodes = Archetype.GetComponentHashCodes(rhs);
			var rhsSizes     = Archetype.GetComponentSizes(rhs);

			// Entity is at the first position.
			var lhsIndex = 1;
			var rhsIndex = 1;
			var count = 1;

			hashCodes[0] = lhsHashCodes[0];
			sizes[0]     = lhsSizes[0];

			while(lhsIndex < lhs->componentCount && rhsIndex < rhs->componentCount)
			{
				if(lhsHashCodes[lhsIndex] < rhsHashCodes[rhsIndex])
				{
					hashCodes[count] = lhsHashCodes[lhsIndex];
					sizes[count] = lhsSizes[lhsIndex];
					++lhsIndex;
					++count;
					continue;
				}

				if(lhsHashCodes[lhsIndex] > rhsHashCodes[rhsIndex])
				{
					hashCodes[count] = rhsHashCodes[rhsIndex];
					sizes[count] = rhsSizes[rhsIndex];
					++rhsIndex;
					++count;
					continue;
				}

				hashCodes[count] = lhsHashCodes[lhsIndex];
				sizes[count] = lhsSizes[lhsIndex];

				++lhsIndex;
				++rhsIndex;
				++count;
			}

			while(lhsIndex < lhs->componentCount)
			{
				hashCodes[count] = lhsHashCodes[lhsIndex];
				sizes[count] = lhsSizes[lhsIndex];

				++lhsIndex;
				++count;
			}

			while(rhsIndex < rhs->componentCount)
			{
				hashCodes[count] = rhsHashCodes[rhsIndex];
				sizes[count] = rhsSizes[rhsIndex];

				++rhsIndex;
				++count;
			}

			return count;
		}

		public static int Difference(Archetype* lhs, uint* rhsHashCodes, int rhsCount,
			uint* hashCodes, int* sizes)
		{
			var lhsHashCodes = Archetype.GetComponentHashCodes(lhs);
			var lhsSizes     = Archetype.GetComponentSizes(lhs);

			// Entity is at the first position.
			var lhsIndex = 1; 
			var rhsIndex = 0;
			var count = 1;

			hashCodes[0] = lhsHashCodes[0];
			sizes[0]     = lhsSizes[0];

			while(lhsIndex < lhs->componentCount && rhsIndex < rhsCount)
			{
				if(lhsHashCodes[lhsIndex] > rhsHashCodes[rhsIndex])
				{
					++rhsIndex;
					continue;
				}

				if(lhsHashCodes[lhsIndex] == rhsHashCodes[rhsIndex])
				{
					++lhsIndex;
					++rhsIndex;
					continue;
				}

				hashCodes[count] = lhsHashCodes[lhsIndex];
				sizes[count] = lhsSizes[lhsIndex];

				++count;
				++lhsIndex;
			}

			while(lhsIndex < lhs->componentCount)
			{
				hashCodes[count] = lhsHashCodes[lhsIndex];
				sizes[count] = lhsSizes[lhsIndex];

				++lhsIndex;
				++count;
			}

			return count;
		}

		public static int GetComponentTypes(Archetype* archetype, int startIndex, Span<ComponentType> types)
		{
			var hashCodes = Archetype.GetComponentHashCodes(archetype) + startIndex;
			var sizes     = Archetype.GetComponentSizes(archetype)     + startIndex;
			var count     = archetype->componentCount - startIndex;
			var typeCount = count > types.Length ? types.Length : count;
		
			for(int i = 0; i < typeCount; ++i)
				types[i] = new ComponentType(hashCodes[i], sizes[i]);

			return count;
		}

		public static bool DidChange(Archetype* archetype, uint changeVersion)
		{
			var versions = Archetype.GetComponentVersions(archetype);

			for(int i = 0; i < archetype->componentCount; ++i)
			{
				if(ChangeVersion.DidChange(versions[i], changeVersion))
					return true;
			}

			return false;
		}

		public static bool DidChange(Archetype* archetype, uint hashCode, uint changeVersion)
		{
			var index    = ArchetypeUtility.GetComponentIndex(archetype, hashCode);
			var versions = Archetype.GetComponentVersions(archetype);

			if(index > 0)
			{
				var entityChanged    = ChangeVersion.DidChange(versions[0], changeVersion);
				var componentChanged = ChangeVersion.DidChange(versions[index], changeVersion);
				
				return entityChanged || componentChanged;
			}
			
			return false;
		}
	}
}