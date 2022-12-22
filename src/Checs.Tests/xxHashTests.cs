using System;
using System.Collections.Generic;
using Xunit;

namespace Checs.Tests
{
	public partial class xxHashTests
	{
		[Fact]
		public unsafe void HashCodesMatch()
		{
			var types = new int[]
			{
				typeof(Position).GetHashCode(),
				typeof(Rotation).GetHashCode(),
				typeof(Velocity).GetHashCode(),
				typeof(Health).GetHashCode()
			};

			fixed(int* ptr = types)
			{
				var h0 = xxHash.GetHashCode((uint*)ptr, types.Length, 0);
				var h1 = xxHash.GetHashCode((uint*)ptr, types.Length, 0);

				Assert.Equal(h0, h1);

				var h2 = xxHash.GetHashCode((uint*)ptr, types.Length, 999);
				var h3 = xxHash.GetHashCode((uint*)ptr, types.Length, 999);

				Assert.Equal(h2, h3);
			}
		}

		[Fact]
		public unsafe void HashCodesDiffer()
		{
			var types = new int[]
			{
				typeof(Position).GetHashCode(),
				typeof(Rotation).GetHashCode(),
				typeof(Velocity).GetHashCode(),
				typeof(Health).GetHashCode()
			};

			fixed(int* ptr = types)
			{
				var h0 = xxHash.GetHashCode((uint*)ptr, 0, 0);
				var h1 = xxHash.GetHashCode((uint*)ptr, 1, 0);
				var h2 = xxHash.GetHashCode((uint*)ptr, 2, 0);
				var h3 = xxHash.GetHashCode((uint*)ptr, 3, 0);
				var h4 = xxHash.GetHashCode((uint*)ptr, 4, 0);

				Assert.NotEqual(h0, h1);
				Assert.NotEqual(h1, h2);
				Assert.NotEqual(h2, h3);
				Assert.NotEqual(h3, h4);
			}
		}
	}
}
