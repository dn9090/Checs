using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;

namespace Checs
{
	internal unsafe static class TypeUtility
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint GetHashCode(Type type)
		{
			// The type of the entity should always have the hash value of 0
			// to guarantee that the entity type information is always at the first index.
			return (uint)type.GetHashCode() ^ Entity.hashCode;
		}

		public static int Sort(ReadOnlySpan<ComponentType> types, uint* hashCodes, int* sizes, int startIndex)
		{
			var count = startIndex;

			for(int i = 0; i < types.Length; ++i)
			{
				var hashCode = types[i].hashCode;
				var size     = types[i].size;
				var index = 0;

				while(index < count && hashCode > hashCodes[index])
					++index;
				
				if(hashCode == hashCodes[index])
					continue;

				var next = count++;

				while(next > index)
				{
					hashCodes[next] = hashCodes[next - 1];
					sizes[next]     = sizes[next - 1];
					--next;
				}

				hashCodes[index] = hashCode;
				sizes[index]     = size;
			}

			return count;
		}

		public static int Sort(ReadOnlySpan<ComponentType> types, uint* hashCodes)
		{
			var count = 0;

			for(int i = 0; i < types.Length; ++i)
			{
				var hashCode = types[i].hashCode;
				var index = 0;

				while(index < count && hashCode > hashCodes[index])
					++index;
				
				if(hashCode == hashCodes[index])
					continue;

				var next = count++;

				while(next > index)
				{
					hashCodes[next] = hashCodes[next - 1];
					--next;
				}

				hashCodes[index] = hashCode;
			}

			return count;
		}

		public static int Sort(uint* hashCodes, int* sizes, int count)
		{
			var actualCount = count;

			for(int i = 1; i < count; ++i)
			{
				var hashCode = hashCodes[i];
				var size = sizes[i];
				var index = i;
				var isDuplicate = 0;

				for(; index > 0 && hashCode.CompareTo(hashCodes[index - 1]) <= 0; --index)
				{
					if(hashCode == hashCodes[index - 1])
					{
						hashCode = 0;
						isDuplicate = 1;
					}

					hashCodes[index] = hashCodes[index - 1];
					sizes[index] = sizes[index - 1];
				}
				
				actualCount -= isDuplicate;
				hashCodes[index] = hashCode;
				sizes[index] = size;
			}

			return actualCount;
		}
	}
}