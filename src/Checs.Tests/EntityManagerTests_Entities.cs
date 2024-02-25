using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Checs.Tests
{
	public partial class EntityManagerTests_Entities
	{
		[Fact]
		public void DefaultEntityIsNotAlive()
		{
			using EntityManager manager = new EntityManager();

			var entity = new Entity();

			Assert.False(manager.Exists(entity));
			
			manager.CreateEntity();

			Assert.False(manager.Exists(entity));
		}

		[Fact]
		public void Creatable()
		{
			using EntityManager manager = new EntityManager();

			{
				var entity = manager.CreateEntity();

				Assert.True(manager.Exists(entity));
				Assert.Equal(0, entity.index);
			}

			{
				var entityCount = manager.entityCount;
				var entities = new Entity[100];
				manager.CreateEntity(entities);

				Assert.All(entities, x => Assert.True(manager.Exists(x)));
				Assert.Equal(entityCount + entities.Length, manager.entityCount);
			}

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var entity = manager.CreateEntity(archetype);

				Assert.True(manager.Exists(entity));
				Assert.Equal(archetype, manager.GetArchetype(entity));
			}

			{
				var entities = new Entity[20_000];
				manager.CreateEntity(entities);

				Assert.Equal(entities[entities.Length - 1].index, entities[0].index + entities.Length - 1);
			}

			{
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
				Assert.Throws<ArgumentOutOfRangeException>(() => manager.CreateEntity(archetype, -1));
			}
		}

		[Fact]
		public void Destroyable()
		{
			using EntityManager manager = new EntityManager();

			{
				var entity = manager.CreateEntity();
							
				manager.DestroyEntity(entity);

				Assert.False(manager.Exists(entity));
			}

			{
				var entities = new Entity[100];

				manager.CreateEntity(entities);

				var entityCount = manager.entityCount;

				manager.DestroyEntity(entities);

				Assert.All(entities, x => Assert.False(manager.Exists(x)));
				Assert.Equal(entityCount - entities.Length, manager.entityCount);
			}

			{
				var entities = new Entity[10];
				var archetype = manager.CreateArchetype();

				manager.CreateEntity(archetype, entities);
				manager.DestroyEntity(archetype);

				Assert.All(entities, x => Assert.False(manager.Exists(x)));
				Assert.Equal(0, manager.GetEntityCount(archetype));
			}

			{
				var entities = new Entity[10];
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
				var query = manager.CreateQuery(new[] {
					ComponentType.Of<Position>()
				});

				manager.CreateEntity(archetype, entities);
				manager.DestroyEntity(query);

				Assert.All(entities, x => Assert.False(manager.Exists(x)));
				Assert.Equal(0, manager.GetEntityCount(query)); 
			}

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Velocity>()
				});

				manager.CreateEntity(archetype, 5000);
				manager.DestroyEntity(archetype);
				Assert.Equal(0, manager.GetEntityCount(archetype));
			}
		}

		[Fact]
		public void Instantiatable()
		{
			using EntityManager manager = new EntityManager();

			{
				var archetype = manager.CreateArchetype();
				var entity = manager.CreateEntity(archetype);
				var entityCount = manager.GetEntityCount(archetype);
				
				manager.Instantiate(entity, 10);

				Assert.Equal(entityCount + 10, manager.GetEntityCount(archetype));
			}

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>()
				});
				var entity = manager.CreateEntity(archetype);

				var position = new Position(1000f, 2000f, 3000f);
				var rotation = new Rotation(10f, 100f, 1000f, 10000f);
				var scale = new Scale(-100f, -10f, -1f);
			
				manager.SetComponentData<Position>(entity, position);
				manager.SetComponentData<Rotation>(entity, rotation);
				manager.SetComponentData<Scale>(entity, scale);

				var instances = new Entity[100];
				manager.Instantiate(entity, instances);
			
				Assert.All(instances, x => {
					Assert.Equal(position, manager.GetComponentData<Position>(x));
					Assert.Equal(rotation, manager.GetComponentData<Rotation>(x));
					Assert.Equal(scale, manager.GetComponentData<Scale>(x));
				});
			}
		}

		[Fact]
		public void Movable()
		{
			using EntityManager manager = new EntityManager();

			{
				var entity = manager.CreateEntity();
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
							
				manager.MoveEntity(entity, archetype);

				Assert.True(manager.Exists(entity));
				Assert.Equal(archetype, manager.GetArchetype(entity));
			}

			{
				var entities = new Entity[100];
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());

				manager.CreateEntity(entities);

				var entityCount = manager.entityCount;

				manager.MoveEntity(entities, archetype);

				Assert.All(entities, x => Assert.Equal(archetype, manager.GetArchetype(x)));
				Assert.Equal(entityCount, manager.entityCount);
			}

			{
				var lhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var rhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Scale>()
				});

				var entity = manager.CreateEntity(lhs);

				manager.MoveEntity(entity, rhs);

				Assert.Equal(rhs, manager.GetArchetype(entity));
			}

			{
				var lhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var rhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Scale>()
				});

				var entities      = new Entity[20];
				var movedEntities = new Entity[20];

				manager.CreateEntity(lhs, entities.AsSpan(0, 10));
				manager.CreateEntity(rhs, entities.AsSpan(10));

				var count = manager.MoveEntity(entities, movedEntities, rhs);

				Assert.Equal(10, count);
				Assert.Equal(entities.Take(10), movedEntities.Take(count));
				Assert.All(entities, x => Assert.Equal(rhs, manager.GetArchetype(x)));
			}
		}

		[Fact]
		public void Unique()
		{
			using EntityManager manager = new EntityManager();

			{
				var lhs = manager.CreateEntity();
				var rhs = manager.CreateEntity();

				Assert.NotEqual(lhs, rhs);
			}

			{
				var entities = new Entity[100];
				manager.CreateEntity(entities);

				Assert.Equal(entities.Length, entities.Distinct().Count());
			}

			{
				var lhs = new Entity[20];
				var rhs = new Entity[30];
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());

				manager.CreateEntity(lhs);
				manager.CreateEntity(archetype, rhs);

				Assert.Equal(lhs.Length + rhs.Length, lhs.Union(rhs).Count());
			}
		}

		[Fact]
		public void Recycable()
		{
			using EntityManager manager = new EntityManager();

			{
				var dead = manager.CreateEntity();
				manager.DestroyEntity(dead);

				var alive = manager.CreateEntity();

				Assert.Equal(dead.index, alive.index);
				Assert.NotEqual(dead.version, alive.version);
			}

			{
				var entities = new Entity[100];

				manager.CreateEntity(entities);

				var dead = entities[entities.Length - 1];

				manager.DestroyEntity(entities);
				manager.CreateEntity(entities);

				var alive = entities[entities.Length - 1];

				Assert.Equal(dead.index, alive.index);
				Assert.NotEqual(dead.version, alive.version);
			}
		}
	}
}
