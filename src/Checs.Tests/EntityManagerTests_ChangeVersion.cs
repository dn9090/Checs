using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Checs.Tests
{
	public partial class EntityManagerTests_ChangeVersion
	{
		[Fact]
		public void CreateEntityIncrements()
		{
			using EntityManager manager = new EntityManager();

			{
				var archetype = manager.CreateArchetype();
				
				var changeVersion = manager.GetChangeVersion();
				
				Assert.False(manager.DidChange(changeVersion));
				Assert.False(manager.DidChange(changeVersion, archetype));

				manager.CreateEntity(archetype);

				Assert.True(manager.DidChange(changeVersion));
				Assert.True(manager.DidChange(changeVersion, archetype));
			}

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});

				var changeVersion = manager.GetChangeVersion();
				
				Assert.False(manager.DidChange(changeVersion));
				Assert.False(manager.DidChange(changeVersion, archetype));

				manager.CreateEntity(archetype);

				Assert.True(manager.DidChange(changeVersion));
				Assert.True(manager.DidChange(changeVersion, archetype));

				changeVersion = manager.GetChangeVersion();
				
				Assert.False(manager.DidChange(changeVersion));
				Assert.False(manager.DidChange(changeVersion, archetype));

				manager.CreateEntity(archetype);

				Assert.True(manager.DidChange(changeVersion));
				Assert.True(manager.DidChange(changeVersion, archetype));
			}
		}

		[Fact]
		public void DestroyEntityDoesNotIncrement()
		{
			using EntityManager manager = new EntityManager();

			{
				var archetype = manager.CreateArchetype();
				var entity = manager.CreateEntity(archetype);

				var changeVersion = manager.GetChangeVersion();

				Assert.False(manager.DidChange(changeVersion));
				Assert.False(manager.DidChange(changeVersion, archetype));

				manager.DestroyEntity(entity);

				Assert.False(manager.DidChange(changeVersion));
				Assert.False(manager.DidChange(changeVersion, archetype));
			}
		}

		[Fact]
		public void SetComponentDataIncrements()
		{
			using EntityManager manager = new EntityManager();

			{
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
				var entity = manager.CreateEntity(archetype);

				var changeVersion = manager.GetChangeVersion();

				Assert.False(manager.DidChange(changeVersion));
				Assert.False(manager.DidChange(changeVersion, entity, ComponentType.Of<Position>()));
				Assert.False(manager.DidChange(changeVersion, archetype));

				manager.SetComponentData(entity, new Position(1f, 2f, 3f));

				Assert.True(manager.DidChange(changeVersion));
				Assert.True(manager.DidChange(changeVersion, entity, ComponentType.Of<Position>()));
				Assert.True(manager.DidChange(changeVersion, archetype));

				changeVersion = manager.GetChangeVersion();

				Assert.False(manager.DidChange(changeVersion));
				Assert.False(manager.DidChange(changeVersion, entity, ComponentType.Of<Position>()));
				Assert.False(manager.DidChange(changeVersion, archetype));
			}

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var entity = manager.CreateEntity(archetype);

				var changeVersion = manager.GetChangeVersion();

				Assert.False(manager.DidChange(changeVersion));
				Assert.False(manager.DidChange(changeVersion, entity));
				Assert.False(manager.DidChange(changeVersion, archetype));

				manager.SetComponentData(entity, new Position(1f, 2f, 3f));
				manager.SetOrAddComponentData(entity, new Rotation(4f, 3f, 2f, 1f));

				Assert.True(manager.DidChange(changeVersion));
				Assert.True(manager.DidChange(changeVersion, entity));
				Assert.True(manager.DidChange(changeVersion, archetype));

				changeVersion = manager.GetChangeVersion();

				Assert.False(manager.DidChange(changeVersion));
				Assert.False(manager.DidChange(changeVersion, entity));
				Assert.False(manager.DidChange(changeVersion, archetype));

				manager.SetComponentData(entity, new Position(3f, 2f, 1f));

				Assert.True(manager.DidChange(changeVersion));
				Assert.True(manager.DidChange(changeVersion, entity));
				Assert.True(manager.DidChange(changeVersion, archetype));
			}
		}
	}
}
