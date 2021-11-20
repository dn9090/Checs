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
			using EntityManager manager = new EntityManager();

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

		[Fact]
		public void EntitiesAreDestroyable()
		{
			using EntityManager manager = new EntityManager();

			var entity = manager.CreateEntity();
			
			Assert.True(manager.IsAlive(entity));

			manager.DestroyEntity(entity);

			Assert.False(manager.IsAlive(entity));

			var entities = new Entity[100];

			manager.CreateEntity(entities);

			Assert.All(entities, (x) => Assert.True(manager.IsAlive(x)));

			manager.DestroyEntity(entities);

			Assert.All(entities, (x) => Assert.False(manager.IsAlive(x)));
		}

		[Fact]
		public void EntitiesAreRecycable()
		{
			using EntityManager manager = new EntityManager();

			var destroyedEntity = manager.CreateEntity();

			manager.DestroyEntity(destroyedEntity);

			var aliveEntity = manager.CreateEntity();
			
			Assert.False(manager.IsAlive(destroyedEntity));
			Assert.True(manager.IsAlive(aliveEntity));

			Assert.Equal(destroyedEntity.index, aliveEntity.index);
			Assert.NotEqual(destroyedEntity.version, aliveEntity.version);
			Assert.NotEqual(destroyedEntity, aliveEntity);


			var entityBatch = manager.CreateEntity(100);
			destroyedEntity = entityBatch[10];

			manager.DestroyEntity(destroyedEntity);

			aliveEntity = manager.CreateEntity();
			
			Assert.False(manager.IsAlive(destroyedEntity));
			Assert.True(manager.IsAlive(aliveEntity));

			Assert.Equal(destroyedEntity.index, aliveEntity.index);
			Assert.NotEqual(destroyedEntity.version, aliveEntity.version);
			Assert.NotEqual(destroyedEntity, aliveEntity);
		}
	}
}
