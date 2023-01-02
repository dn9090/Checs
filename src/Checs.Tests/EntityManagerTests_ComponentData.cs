using System;
using System.Collections.Generic;
using Xunit;

namespace Checs.Tests
{
	public partial class EntityManagerTests_ComponentData
	{
		[Fact]
		public void MatchArchetype()
		{
			using EntityManager manager = new EntityManager();

			{
				var lhsArchetype = manager.CreateArchetype(ComponentType.Of<Position>());
				var rhsArchetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});

				var lhsEntity = manager.CreateEntity(lhsArchetype);
				var rhsEntity = manager.CreateEntity(rhsArchetype);

				Assert.True(manager.HasComponentData<Position>(lhsEntity));
				Assert.False(manager.HasComponentData<Rotation>(lhsEntity));

				Assert.True(manager.HasComponentData<Position>(rhsEntity));
				Assert.True(manager.HasComponentData<Rotation>(rhsEntity));
			}
		}

		[Fact]
		public void Modifiable()
		{
			using EntityManager manager = new EntityManager();

			{
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
				var entity = manager.CreateEntity(archetype);
				var value = new Position(1f, 2f, 3f);

				Assert.True(manager.SetComponentData(entity, value));
				Assert.Equal(value, manager.GetComponentData<Position>(entity));
			}

			{
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
				var entities = new Entity[3];
				var values = new Position[] {
					new Position(1f, 2f, 3f),
					new Position(4f, 5f, 6f),
					new Position(7f, 8f, 9f)
				};

				manager.CreateEntity(archetype, entities);
				manager.SetComponentData(entities[0], values[0]);
				manager.SetComponentData(entities[1], values[1]);
				manager.SetComponentData(entities[2], values[2]);

				Assert.Equal(values[0], manager.GetComponentData<Position>(entities[0]));
				Assert.Equal(values[1], manager.GetComponentData<Position>(entities[1]));
				Assert.Equal(values[2], manager.GetComponentData<Position>(entities[2]));
			}

			{
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
				var entity = manager.CreateEntity(archetype);
				var value = new Position(1f, 2f, 3f);

				manager.SetComponentData(entity, value);
				manager.CreateEntity(archetype, 10);

				Assert.Equal(value, manager.GetComponentData<Position>(entity));
			}
		}

		[Fact]
		public void Addable()
		{
			using EntityManager manager = new EntityManager();

			{
				var entity = manager.CreateEntity();

				Assert.False(manager.AddComponentData<Entity>(entity));
			}

			{
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
				var entity = manager.CreateEntity(archetype);

				Assert.True(manager.AddComponentData<Rotation>(entity));
				Assert.True(manager.HasComponentData<Rotation>(entity));
			}

			{
				var archetype = manager.CreateArchetype();
				var entity = manager.CreateEntity(archetype);

				Assert.True(manager.AddComponentData<Position>(entity));
				Assert.False(manager.AddComponentData<Position>(entity));
			}

			{
				var entity = manager.CreateEntity();

				Assert.True(manager.AddComponentData<Position>(entity));
				Assert.True(manager.AddComponentData<Rotation>(entity));
				Assert.True(manager.AddComponentData<Velocity>(entity));
				Assert.True(manager.AddComponentData<Scale>(entity));

				Assert.False(manager.AddComponentData<Position>(entity));
				Assert.False(manager.AddComponentData<Rotation>(entity));
				Assert.False(manager.AddComponentData<Velocity>(entity));
				Assert.False(manager.AddComponentData<Scale>(entity));

				Assert.True(manager.HasComponentData<Position>(entity));
				Assert.True(manager.HasComponentData<Rotation>(entity));
				Assert.True(manager.HasComponentData<Velocity>(entity));
				Assert.True(manager.HasComponentData<Scale>(entity));
			}
		}

		[Fact]
		public void Removable()
		{
			using EntityManager manager = new EntityManager();

			{
				var entity = manager.CreateEntity();

				Assert.False(manager.RemoveComponentData<Entity>(entity));
			}

			{
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
				var entity = manager.CreateEntity(archetype);

				Assert.True(manager.RemoveComponentData<Position>(entity));
				Assert.False(manager.HasComponentData<Position>(entity));
			}

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var entity = manager.CreateEntity(archetype);

				Assert.True(manager.RemoveComponentData<Position>(entity));
				Assert.False(manager.RemoveComponentData<Position>(entity));
			}

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Velocity>(),
					ComponentType.Of<Scale>()
				});
				var entity = manager.CreateEntity(archetype);

				Assert.True(manager.RemoveComponentData<Position>(entity));
				Assert.True(manager.RemoveComponentData<Rotation>(entity));
				Assert.True(manager.RemoveComponentData<Velocity>(entity));
				Assert.True(manager.RemoveComponentData<Scale>(entity));

				Assert.False(manager.RemoveComponentData<Position>(entity));
				Assert.False(manager.RemoveComponentData<Rotation>(entity));
				Assert.False(manager.RemoveComponentData<Velocity>(entity));
				Assert.False(manager.RemoveComponentData<Scale>(entity));

				Assert.False(manager.HasComponentData<Position>(entity));
				Assert.False(manager.HasComponentData<Rotation>(entity));
				Assert.False(manager.HasComponentData<Velocity>(entity));
				Assert.False(manager.HasComponentData<Scale>(entity));
			}
		}

		[Fact]
		public void Copyable()
		{
			using EntityManager manager = new EntityManager();

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var entities = new Entity[100];
				manager.CreateEntity(archetype, entities);

				var buffer = new Position[200];
				var copyCount = manager.CopyComponentData<Position>(archetype, buffer);

				Assert.Equal(entities.Length, copyCount);
			}

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var entities = new Entity[100];
				manager.CreateEntity(archetype, entities);

				var buffer = new Scale[200];
				var copyCount = manager.CopyComponentData<Scale>(archetype, buffer);

				Assert.Equal(0, copyCount);
			}
		}

		[Fact]
		public void Batchable()
		{
			using EntityManager manager = new EntityManager();

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var entities = new Entity[20];
				manager.CreateEntity(archetype, entities);

				var count = manager.SetComponentData<Position>(entities, new Position(1f, 2f, 3f));

				Assert.Equal(entities.Length, count);
			}

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var entities = new Entity[20];
				manager.CreateEntity(entities.AsSpan(0, entities.Length / 2));
				manager.CreateEntity(archetype, entities.AsSpan(entities.Length / 2));

				var count = manager.SetComponentData<Position>(entities, new Position(1f, 2f, 3f));

				Assert.Equal(entities.Length / 2, count);
			}
		}
	}
}
