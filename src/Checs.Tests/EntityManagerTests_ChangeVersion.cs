using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Checs.Tests
{
	public partial class EntityManagerTests_ChangeVersion
	{
		[Fact]
		public void CreateEntityIncrementsChangeVersion()
		{
			using EntityManager manager = new EntityManager();

			{
				var changeVersion = new EntityChangeVersion();
				var archetype = manager.CreateArchetype();

				Assert.False(manager.HasChanged(archetype, changeVersion));

				manager.CreateEntity(archetype);

				Assert.True(manager.HasChanged(archetype, changeVersion));
			}

			{
				var changeVersion = new EntityChangeVersion();
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());

				Assert.False(manager.HasChanged(archetype, changeVersion));

				manager.CreateEntity(archetype);

				Assert.True(manager.HasChanged(archetype, changeVersion));
			}
		}

		//[Fact]
		public void DestroyEntityIncrementsChangeVersion()
		{
			using EntityManager manager = new EntityManager();

			{
				var changeVersion = new EntityChangeVersion();
				var archetype = manager.CreateArchetype();
				var entity = manager.CreateEntity(archetype);

				manager.UpdateChangeVersion(ref changeVersion);

				Assert.False(manager.HasChanged(entity, changeVersion));
				Assert.False(manager.HasChanged(archetype, changeVersion));

				manager.DestroyEntity(entity);

				Assert.True(manager.HasChanged(entity, changeVersion));
				Assert.True(manager.HasChanged(archetype, changeVersion));
			}

			{
				var changeVersion = new EntityChangeVersion();
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
				var entity = manager.CreateEntity(archetype);

				manager.UpdateChangeVersion(ref changeVersion);

				Assert.False(manager.HasChanged(entity, changeVersion));
				Assert.False(manager.HasChanged(archetype, changeVersion));

				manager.DestroyEntity(entity);

				Assert.True(manager.HasChanged(entity, changeVersion));
				Assert.True(manager.HasChanged(archetype, changeVersion));
			}
		}

		[Fact]
		public void SetComponentDataIncrementsChangeVersion()
		{
			using EntityManager manager = new EntityManager();

			{
				var changeVersion = new EntityChangeVersion();
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
				var entity = manager.CreateEntity(archetype);

				manager.UpdateChangeVersion(ref changeVersion);

				Assert.False(manager.HasChanged(entity, changeVersion));
				Assert.False(manager.HasChanged<Position>(entity, changeVersion));
				Assert.False(manager.HasChanged(archetype, changeVersion));
				Assert.False(manager.HasChanged<Position>(archetype, changeVersion));


				manager.SetComponentData(entity, new Position(1f, 2f, 3f));

				Assert.True(manager.HasChanged(entity, changeVersion));
				Assert.True(manager.HasChanged<Position>(entity, changeVersion));
				Assert.True(manager.HasChanged(archetype, changeVersion));
				Assert.True(manager.HasChanged<Position>(archetype, changeVersion));
			}
		}

		[Fact]
		public void OnlyTableWriteAccessIncrementsChangeVersion()
		{
			using EntityManager manager = new EntityManager();

			{
				var changeVersion = new EntityChangeVersion();
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>(),
					ComponentType.Of<Velocity>()
				});
				manager.CreateEntity(archetype, 900);
				manager.UpdateChangeVersion(ref changeVersion);

				manager.ForEach(archetype, static (table, changeVersion) => {
					Assert.False(table.HasChanged(changeVersion));

					var positions = table.GetComponentData<Position>();
					var rotations = table.GetComponentData<Rotation>();
					var scale     = table.GetComponentDataReadOnly<Scale>();
					var Velocity  = table.GetComponentDataReadOnly<Velocity>();
				}, changeVersion);

				manager.ForEach(archetype, static (table, changeVersion) => {
					Assert.True(table.HasChanged(changeVersion));
					Assert.True(table.HasChanged<Position>(changeVersion));
					Assert.True(table.HasChanged<Rotation>(changeVersion));
					Assert.False(table.HasChanged<Scale>(changeVersion));
					Assert.False(table.HasChanged<Velocity>(changeVersion));
				}, changeVersion);
			}
		}

		[Fact]
		public void SetComponentDataLookupIncrementsChangeVersion()
		{
			using EntityManager manager = new EntityManager();
			

			{
				var changeVersion = new EntityChangeVersion();
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
				var entity = manager.CreateEntity(archetype);
				var lookup = new ComponentDataLookup<Position>(manager);

				manager.UpdateChangeVersion(ref changeVersion);

				Assert.False(manager.HasChanged(entity, changeVersion));
				Assert.False(manager.HasChanged<Position>(entity, changeVersion));
				Assert.False(manager.HasChanged(archetype, changeVersion));
				Assert.False(manager.HasChanged<Position>(archetype, changeVersion));

				lookup[entity] = new Position(1f, 2f, 3f);

				Assert.True(manager.HasChanged(entity, changeVersion));
				Assert.True(manager.HasChanged<Position>(entity, changeVersion));
				Assert.True(manager.HasChanged(archetype, changeVersion));
				Assert.True(manager.HasChanged<Position>(archetype, changeVersion));
			}

			{
				var changeVersion = new EntityChangeVersion();
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>(),
					ComponentType.Of<Velocity>()
				});
				var lookup = new ComponentDataLookup<Scale>(manager);
				var entities = new Entity[300];
				manager.CreateEntity(archetype, entities);

				manager.UpdateChangeVersion(ref changeVersion);

				Assert.False(manager.HasChanged<Scale>(entities[0], changeVersion));
				Assert.False(manager.HasChanged<Scale>(entities[entities.Length - 1], changeVersion));
				
				lookup[entities[0]] = new Scale(1f, 2f, 3f);

				Assert.True(manager.HasChanged<Scale>(entities[0], changeVersion));
				Assert.False(manager.HasChanged<Scale>(entities[entities.Length - 1], changeVersion));
			}
		}
	}
}
