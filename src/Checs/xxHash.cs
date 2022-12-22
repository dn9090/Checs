using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	/*
		Based on:
			https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/HashCode.cs
			https://richardstartin.github.io/posts/xxhash
			https://github.com/cespare/xxhash/blob/main/xxhash_other.go
	
	*/
	internal static class xxHash
	{
		public const uint Prime1 = 2654435761U;
		public const uint Prime2 = 2246822519U;
		public const uint Prime3 = 3266489917U;
		public const uint Prime4 = 668265263U;
		public const uint Prime5 = 374761393U;
	
		public static unsafe uint GetHashCode(uint* values, int count, uint seed = 0)
		{
			uint hash = MixEmptyState(seed);

			int pos = 0;

			if(count >= 4)
			{
				int end = count - 4;

				uint v1 = seed + Prime1 + Prime2;
				uint v2 = seed + Prime2;
				uint v3 = seed;
				uint v4 = seed - Prime1;

				do
				{
					v1 = Round(v1, values[pos]);
					v2 = Round(v2, values[pos + 1]);
					v3 = Round(v3, values[pos + 2]);
					v4 = Round(v4, values[pos + 3]);

					pos += 4;
				} while(pos <= end);

				hash = MixState(v1, v2, v3, v4);
			}

			hash += (uint)count;

			while(pos < count)
				hash = QueueRound(hash, values[pos++]);

			return MixFinal(hash);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint Round(uint hash, uint input)
		{
			return BitOperations.RotateLeft(hash + input * Prime2, 13) * Prime1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint QueueRound(uint hash, uint queuedValue)
		{
			return BitOperations.RotateLeft(hash + queuedValue * Prime3, 17) * Prime4;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint MixState(uint v1, uint v2, uint v3, uint v4)
		{
			return BitOperations.RotateLeft(v1, 1) + BitOperations.RotateLeft(v2, 7) + BitOperations.RotateLeft(v3, 12) + BitOperations.RotateLeft(v4, 18);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint MixEmptyState(uint seed)
		{
			return seed + Prime5;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint MixFinal(uint hash)
		{
			hash ^= hash >> 15;
			hash *= Prime2;
			hash ^= hash >> 13;
			hash *= Prime3;
			hash ^= hash >> 16;
			return hash;
		}
	}
}
