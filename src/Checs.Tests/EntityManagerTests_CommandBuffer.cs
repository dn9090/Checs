using System;
using System.Collections.Generic;
using Xunit;

namespace Checs.Tests
{
	public partial class EntityManagerTests_CommandBuffer
	{
		[Fact]
		public void DestroysEntities()
		{
			using EntityManager manager = new EntityManager();

			{
				var entities = new Entity[10];
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
				manager.CreateEntity(archetype, entities);
				var entityCount = manager.entityCount;

				using var buffer = manager.CreateCommandBuffer();
				buffer.DestroyEntity(entities);

				manager.Playback(buffer);

				Assert.Equal(entityCount - entities.Length, manager.entityCount);
			}

			{
				var entities = new Entity[3072];
				manager.CreateEntity(entities);
				var entityCount = manager.entityCount;

				using var buffer = manager.CreateCommandBuffer();
				buffer.DestroyEntity(entities);

				manager.Playback(buffer);

				Assert.Equal(entityCount - entities.Length, manager.entityCount);
			}

			{
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
				var entities = new Entity[10];
				manager.CreateEntity(archetype, entities);
				var entityCount = manager.entityCount;

				using var buffer = manager.CreateCommandBuffer();
				buffer.DestroyEntity(archetype);

				manager.Playback(buffer);

				Assert.Equal(entityCount - entities.Length, manager.entityCount);
			}

			{
				var query = manager.CreateQuery();
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
				manager.CreateEntity(archetype, 10);

				using var buffer = manager.CreateCommandBuffer();
				buffer.DestroyEntity(query);

				manager.Playback(buffer);

				Assert.Equal(0, manager.entityCount);
			}
		}

		[Fact]
		public void MovesEntities()
		{
			using EntityManager manager = new EntityManager();

			{
				var entities = new Entity[10];
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
				var entityCount = manager.GetEntityCount(archetype);
				manager.CreateEntity(entities);

				using var buffer = manager.CreateCommandBuffer();
				buffer.MoveEntity(entities, archetype);

				manager.Playback(buffer);

				Assert.Equal(entityCount + entities.Length, manager.GetEntityCount(archetype));
			}
		}

		[Fact]
		public void InstantiatesEntities()
		{
			using EntityManager manager = new EntityManager();

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var entity = manager.CreateEntity(archetype);
				var entityCount = manager.entityCount;

				using var buffer = manager.CreateCommandBuffer();
				buffer.Instantiate(entity, 10);

				manager.Playback(buffer);

				Assert.Equal(entityCount + 10, manager.entityCount);
				Assert.Equal(entityCount + 10, manager.GetEntityCount(archetype));
			}

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var entity = manager.CreateEntity(archetype);
				var entityCount = manager.entityCount;

				using var buffer = manager.CreateCommandBuffer();
				buffer.Instantiate(entity, 1);
				buffer.Instantiate(entity, 1);
				buffer.Instantiate(entity, 1);

				manager.Playback(buffer);

				Assert.Equal(entityCount + 3, manager.entityCount);
				Assert.Equal(entityCount + 3, manager.GetEntityCount(archetype));
			}
		}

		/*[Fact]
		public void SetsComponentData()
		{
			using EntityManager manager = new EntityManager();

			{
				var entities = new Entity[10];
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
				manager.CreateEntity(archetype, entities);

				var position = new Position(1f, 2f, 3f);

				using var buffer = manager.CreateCommandBuffer();
				buffer.SetComponentData(entities, position);

				manager.Playback(buffer);

				Assert.All(entities, x => Assert.Equal(position, manager.GetComponentData<Position>(x)));
			}

			{
				var entities = new Entity[3072];
				var archetype = manager.CreateArchetype(ComponentType.Of<Rotation>());
				manager.CreateEntity(archetype, entities);

				var rotation = new Rotation(1f, 2f, 3f, 4f);

				using var buffer = manager.CreateCommandBuffer();
				buffer.SetComponentData(entities, rotation);

				manager.Playback(buffer);

				Assert.Equal(rotation, manager.GetComponentData<Rotation>(entities[0]));
				Assert.Equal(rotation, manager.GetComponentData<Rotation>(entities[entities.Length / 2]));
				Assert.Equal(rotation, manager.GetComponentData<Rotation>(entities[entities.Length - 1]));
			}
		}*/

		[Fact]
		public void ClearedAfterPlayback()
		{
			using EntityManager manager = new EntityManager();

			var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
			var entities = new Entity[2000];
			manager.CreateEntity(entities);

			using var buffer = manager.CreateCommandBuffer();
			buffer.DestroyEntity(entities.AsSpan(0, entities.Length / 2));
			buffer.CreateEntity(archetype, 100);
			buffer.DestroyEntity(entities.AsSpan(entities.Length / 2));
			buffer.CreateEntity(archetype, 100);
			buffer.DestroyEntity(archetype);

			manager.Playback(buffer);

			Assert.Equal(0, buffer.count);
			Assert.Equal(0, manager.entityCount);
		}
	}
}
