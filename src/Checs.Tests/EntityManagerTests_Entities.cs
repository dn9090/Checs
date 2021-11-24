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

			Assert.All(entities, x => Assert.True(manager.IsAlive(x)));

			manager.DestroyEntity(entities);

			Assert.All(entities, x => Assert.False(manager.IsAlive(x)));
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

		[Fact]
		public void EntitiesIndiciesAreSequentialOrRecycled()
		{
			using EntityManager manager = new EntityManager();

			var entities = new Entity[3];
			manager.CreateEntity(entities);

			Assert.Equal(0, entities[0].index);
			Assert.Equal(1, entities[1].index);
			Assert.Equal(2, entities[2].index);

			manager.DestroyEntity(entities);
			manager.CreateEntity(entities);

			Assert.Equal(0, entities[0].index);
			Assert.Equal(1, entities[1].index);
			Assert.Equal(2, entities[2].index);
			
			manager.DestroyEntity(entities);

			var batch1 = new Entity[100];
			var batch2 = new Entity[100];
			var batch3 = new Entity[101];

			manager.CreateEntity(batch1);
			manager.CreateEntity(batch2);

			Assert.Equal(0, batch1[0].index);
			Assert.Equal(100, batch2[0].index);

			manager.DestroyEntity(batch1);

			unsafe {
			Assert.Equal(100, manager.entityStore->freeSlots.count);
			Assert.Equal(200, manager.entityStore->count);}

			manager.CreateEntity(batch3);

			Assert.Equal(0, batch3[0].index);
			Assert.Equal(200, batch3[batch3.Length - 1].index);
		}

		[Fact]
		public void EntityVersionMatchesStoreVersion() // Move to EntityStoreTest?
		{
			using EntityManager manager = new EntityManager();

			var entity = manager.CreateEntity();

			unsafe
			{
				Assert.Equal(entity.version, manager.entityStore->version);
			}

			manager.DestroyEntity(entity);

			var entities = new Entity[123];
			manager.CreateEntity(entities);

			unsafe
			{
				Assert.All(entities, x => Assert.Equal(x.version, manager.entityStore->version));
			}
		}

		[Fact]
		public void DefaultEntityIsNotAlive()
		{
			using EntityManager manager = new EntityManager();

			Entity entity = default;

			Assert.False(manager.IsAlive(entity));

			var entities = manager.CreateEntity(10);

			Assert.False(manager.IsAlive(entity));

			manager.DestroyEntity(entities);

			Assert.False(manager.IsAlive(entity));
		}

		[Fact]
		public void EntityDataIsStoredInChunk()
		{
			using EntityManager manager = new EntityManager();

			var entities = manager.CreateEntity(100);

			Assert.Equal(99, entities[entities.Length - 1].index);

			manager.ForEach(manager.CreateArchetype(), (batch) => {
				Assert.Equal(100, batch.length);
				Assert.Equal(99, batch.GetEntities()[batch.length - 1].index);
			});
		}
	}
}
