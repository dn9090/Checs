using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Checs.Tests
{
	public partial class EntityPrefabTests
	{
		[Fact]
		public void Addable()
		{
			{
				var prefab = new EntityPrefab();

				Assert.False(prefab.HasComponentData<Position>());
				Assert.False(prefab.HasComponentData<Rotation>());

				prefab.SetComponentData<Position>();

				Assert.True(prefab.HasComponentData<Position>());
				Assert.False(prefab.HasComponentData<Rotation>());

				prefab.SetComponentData<Rotation>();

				Assert.True(prefab.HasComponentData<Position>());
				Assert.True(prefab.HasComponentData<Rotation>());
			}

			{
				var prefab = new EntityPrefab();

				Assert.False(prefab.HasComponentData<Position>());
				Assert.False(prefab.HasComponentData<Rotation>());

				prefab.SetComponentData<Position>();

				Assert.True(prefab.HasComponentData<Position>());
				Assert.False(prefab.HasComponentData<Rotation>());

				prefab.SetComponentData<Position>();

				Assert.True(prefab.HasComponentData<Position>());
				Assert.False(prefab.HasComponentData<Rotation>());
			}

			{
				var prefab = new EntityPrefab(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});

				Assert.True(prefab.HasComponentData<Position>());
				Assert.True(prefab.HasComponentData<Rotation>());
			}
		}

		[Fact]
		public void Settable()
		{
			{
				var prefab = new EntityPrefab();

				Assert.False(prefab.HasComponentData<Position>());
				Assert.False(prefab.HasComponentData<Rotation>());

				var position = new Position(1f, 2f, 3f);
				var rotation = new Rotation(4f, 3f, 2f, 1f);

				prefab.SetComponentData<Position>(in position);
				prefab.SetComponentData<Rotation>(in rotation);

				Assert.True(prefab.HasComponentData<Position>());
				Assert.True(prefab.HasComponentData<Rotation>());

				Assert.Equal(position, prefab.GetComponentData<Position>());
				Assert.Equal(rotation, prefab.GetComponentData<Rotation>());
			}

			{
				var prefab = new EntityPrefab(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});

				prefab.SetComponentData<Velocity>();
				prefab.SetComponentData<Scale>();

				var position = new Position(1f, 2f, 3f);
				var velocity = new Velocity(3f, 2f, 1f);
				
				prefab.SetComponentData<Position>(in position);
				prefab.SetComponentData<Velocity>(in velocity);

				Assert.Equal(position, prefab.GetComponentData<Position>());
				Assert.Equal(velocity, prefab.GetComponentData<Velocity>());
			}
		}

		[Fact]
		public void Instantiatable()
		{
			using var manager = new EntityManager();

			{
				var prefab = new EntityPrefab(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var archetype = manager.CreateArchetype(prefab.GetComponentTypes());
				var instances = new Entity[10];

				manager.Instantiate(prefab, instances);

				Assert.All(instances, x => {
					Assert.Equal(archetype, manager.GetArchetype(x));
				});
			}

			{
				var position = new Position(1f, 2f, 3f);
				var rotation = new Rotation(4f, 3f, 2f, 1f);

				var prefab = new EntityPrefab()
					.WithComponentData<Position>(in position)
					.WithComponentData<Rotation>(in rotation);
				var instances = new Entity[10];
				
				manager.Instantiate(prefab, instances);

				Assert.All(instances, x => {
					Assert.Equal(position, manager.GetComponentData<Position>(x));
					Assert.Equal(rotation, manager.GetComponentData<Rotation>(x));
				});
			}

			{
				var prefab = new EntityPrefab(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var velocity  = new Velocity(1f, 2f, 3f);
				var scale     = new Scale(3f, 2f, 1f);

				prefab.SetComponentData<Velocity>(in velocity);
				prefab.SetComponentData<Scale>(in scale);

				var entity = manager.Instantiate(prefab);

				Assert.Equal(velocity, manager.GetComponentData<Velocity>(entity));
				Assert.Equal(scale, manager.GetComponentData<Scale>(entity));
			}
		}

		[Fact]
		public void Convertable()
		{
			using var manager = new EntityManager();

			{
				var prefab = new EntityPrefab(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var entity = manager.Instantiate(prefab);

				var lhs = prefab.GetComponentTypes().ToArray();
				var rhs = manager.ToPrefab(entity).GetComponentTypes().ToArray();

				Array.Sort(lhs);
				Array.Sort(rhs);

				Assert.Equal(lhs, rhs);
			}

			{
				var position = new Position(1f, 2f, 3f);
				var rotation = new Rotation(4f, 3f, 2f, 1f);

				var prefab = new EntityPrefab()
					.WithComponentData<Position>(in position)
					.WithComponentData<Rotation>(in rotation);
				
				var entity        = manager.Instantiate(prefab);
				var entityPrefab  = manager.ToPrefab(entity);

				Assert.Equal(position, entityPrefab.GetComponentData<Position>());
				Assert.Equal(rotation, entityPrefab.GetComponentData<Rotation>());
			}
		}
	}
}
