using System;
using System.Collections.Generic;
using Xunit;

namespace Checs.Tests
{
	public partial class TypeRegistryTests
	{
		[Fact]
		public void EntityHashCodeIsAlwaysZero()
		{
			var hashCode = TypeRegistry.GetTypeInfo<Entity>().hashCode;
			Assert.Equal(0u, hashCode);
		}

		[Fact]
		public void CachedInfoMatchesStoredInfo()
		{
			var storedInfo = TypeRegistry.GetTypeInfo<Position>();
			var cachedInfo = TypeRegistry<Position>.info;

			Assert.Equal(storedInfo.hashCode, cachedInfo.hashCode);
			Assert.Equal(storedInfo.size, cachedInfo.size);
			Assert.Equal(storedInfo.type, cachedInfo.type);
		}
	}
}
