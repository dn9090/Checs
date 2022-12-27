/*
xxHash implementation based on the code published by Yann Collet:
https://raw.githubusercontent.com/Cyan4973/xxHash/5c174cfa4e45a42f94082dc0d4539b39696afea1/xxhash.c

  xxHash - Fast Hash algorithm
  Copyright (C) 2012-2016, Yann Collet

  BSD 2-Clause License (http://www.opensource.org/licenses/bsd-license.php)

  Redistribution and use in source and binary forms, with or without
  modification, are permitted provided that the following conditions are
  met:

  * Redistributions of source code must retain the above copyright
  notice, this list of conditions and the following disclaimer.
  * Redistributions in binary form must reproduce the above
  copyright notice, this list of conditions and the following disclaimer
  in the documentation and/or other materials provided with the
  distribution.

  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
  OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
  LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
  OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

  You can contact the author at :
  - xxHash homepage: http://www.xxhash.com
  - xxHash source repository : https://github.com/Cyan4973/xxHash

With additional modifications based on the xxHash implementation of the .NET Foundation:
https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/HashCode.cs

  The MIT License (MIT)
  
  Copyright (c) .NET Foundation and Contributors
  
  All rights reserved.
  
  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files (the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:
  
  The above copyright notice and this permission notice shall be included in all
  copies or substantial portions of the Software.
  
  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
  SOFTWARE.
*/

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	/// <summary>
	/// Hash code generation based on the xxHash algorithm.
	/// </summary>
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

		public static unsafe uint GetHashCode(byte* values, int count, uint seed = 0)
		{
			uint hash = MixEmptyState(seed);

			int pos = 0;

			if(count >= 16)
			{
				int end = count - 16;

				uint v1 = seed + Prime1 + Prime2;
				uint v2 = seed + Prime2;
				uint v3 = seed;
				uint v4 = seed - Prime1;

				do
				{
					v1 = Round(v1, Unsafe.Read<uint>(values + pos));
					v2 = Round(v2, Unsafe.Read<uint>(values + pos + 4));
					v3 = Round(v3, Unsafe.Read<uint>(values + pos + 8));
					v4 = Round(v4, Unsafe.Read<uint>(values + pos + 12));

					pos += 16;
				} while(pos <= end);

				hash = MixState(v1, v2, v3, v4);
			}

			hash += (uint)count;

			while(pos + 4 <= count)
			{
				hash = QueueRound(hash, Unsafe.Read<uint>(values + pos));
				pos += 4;
			}

			while(pos < count)
				hash = QueueRound(hash, values[pos++]);
		
			return MixFinal(hash);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe static uint GetHashCode(string value, uint seed = 0)
		{
			fixed(char* ptr = value)
				return GetHashCode((byte*)ptr, value.Length * sizeof(char), seed);
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
		private static uint QueueRound(uint hash, byte queuedValue)
		{
			return BitOperations.RotateLeft(hash + queuedValue * Prime5, 11) * Prime1;
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
