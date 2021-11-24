using System;
using System.Collections.Generic;
using Xunit;

namespace Checs.Tests
{
	public partial class EntityManagerTests
	{
		[Fact]
		public void ForEachBatchesAllEntitesInArchetype()
		{
			using EntityManager manager = new EntityManager();

			var archetype = manager.CreateArchetype(new Type[] { typeof(Position), typeof(Rotation) });
			var entities = manager.CreateEntity(archetype, 10000);
			
			int entityCount = 0;

			manager.ForEach(archetype, (batch) => {
				entityCount += batch.length;
			});

			Assert.Equal(entities.Length, entityCount);
		}
	}
}
