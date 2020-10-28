using System;
using System.Collections.Generic;
using Xunit;

namespace Checs.Tests
{
	public partial class EntityManagerTests
	{
		[Fact]
		public void EntitiesAreUnique()
		{
			EntityManager manager = new EntityManager();

			var entities = manager.CreateEntity(100);
			var distinct = new HashSet<Entity>(entities.ToArray());

			Assert.Equal(entities.Length, distinct.Count);

			var entities2 = manager.CreateEntity(100);
			distinct.UnionWith(entities2.ToArray());

			Assert.Equal(entities.Length + entities2.Length, distinct.Count);

			var entities3 = manager.CreateEntity(manager.CreateArchetype(typeof(Position)), 100);
			distinct.UnionWith(entities3.ToArray());

			Assert.Equal(entities.Length + entities2.Length + entities3.Length, distinct.Count);
		}
	}
}
