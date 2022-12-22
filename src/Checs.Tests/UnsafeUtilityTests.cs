using System;
using System.Collections.Generic;
using Xunit;

namespace Checs.Tests
{
	public partial class UnsafeUtilityTests
	{
		[Fact]
		public void RepeatCopy()
		{
			var buffer = new byte[512];

			{
				var pattern = GetBytePattern(3);
				var count = 17;

				unsafe
				{
					fixed(byte* dst = buffer, src = pattern)
						UnsafeUtility.CloneMemory3(dst, src, pattern.Length, count);
				}

				for(int i = 0; i < (pattern.Length * count); ++i)
					Assert.Equal(pattern[i % pattern.Length], buffer[i]);
				
				AssertBufferEndIsZero(buffer, pattern.Length * count);
				
				Array.Clear(buffer);
			}

			{
				var pattern = GetBytePattern(3);
				var count = 128;

				unsafe
				{
					fixed(byte* dst = buffer, src = pattern)
						UnsafeUtility.CloneMemory3(dst, src, pattern.Length, count);
				}

				for(int i = 0; i < (pattern.Length * count); ++i)
					Assert.Equal(pattern[i % pattern.Length], buffer[i]);
				
				AssertBufferEndIsZero(buffer, pattern.Length * count);
				
				Array.Clear(buffer);
			}

			{
				var pattern = GetBytePattern(8);
				var count = 29;

				unsafe
				{
					fixed(byte* dst = buffer, src = pattern)
						UnsafeUtility.CloneMemory3(dst, src, pattern.Length, count);
				}

				for(int i = 0; i < (pattern.Length * count); ++i)
					Assert.Equal(pattern[i % pattern.Length], buffer[i]);
				
				AssertBufferEndIsZero(buffer, pattern.Length * count);
				
				Array.Clear(buffer);
			}

			{
				var pattern = GetBytePattern(11);
				var count = 31;

				unsafe
				{
					fixed(byte* dst = buffer, src = pattern)
						UnsafeUtility.CloneMemory3(dst, src, pattern.Length, count);
				}

				for(int i = 0; i < (pattern.Length * count); ++i)
					Assert.Equal(pattern[i % pattern.Length], buffer[i]);
				
				AssertBufferEndIsZero(buffer, pattern.Length * count);
				
				Array.Clear(buffer);
			}

			{
				var pattern = GetBytePattern(32);
				var count = 5;

				unsafe
				{
					fixed(byte* dst = buffer, src = pattern)
						UnsafeUtility.CloneMemory3(dst, src, pattern.Length, count);
				}

				for(int i = 0; i < (pattern.Length * count); ++i)
					Assert.Equal(pattern[i % pattern.Length], buffer[i]);

				AssertBufferEndIsZero(buffer, pattern.Length * count);

				Array.Clear(buffer);
			}

			{
				var pattern = GetBytePattern(48);
				var count = 4;

				unsafe
				{
					fixed(byte* dst = buffer, src = pattern)
						UnsafeUtility.CloneMemory3(dst, src, pattern.Length, count);
				}

				for(int i = 0; i < (pattern.Length * count); ++i)
					Assert.Equal(pattern[i % pattern.Length], buffer[i]);
				
				AssertBufferEndIsZero(buffer, pattern.Length * count);
				
				Array.Clear(buffer);
			}
		}

		public static byte[] GetBytePattern(int count)
		{
			var bytes = new byte[count];

			for(int i = 0; i < bytes.Length; ++i)
				bytes[i] = (byte)(i + 1);

			return bytes;
		}

		public static void AssertBufferEndIsZero(byte[] buffer, int count)
		{
			var remaining = buffer.Length - count;
			var max = remaining > 32 ? 32 : remaining;

			for(int i = count; i < count + max; ++i)
				Assert.Equal(0, buffer[i]);
		}
	}
}
