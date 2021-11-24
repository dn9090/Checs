using System;
using System.Collections.Generic;
using Xunit;

namespace Checs.Tests
{
	public partial class EntityManagerTests
	{
		[Fact]
		public void ComponentDataTypesMatchArchetype()
		{
			using EntityManager manager = new EntityManager();

			var positionArchetype = manager.CreateArchetype(typeof(Position));
			var positionAndRotationArchetype = manager.CreateArchetype(new Type[] { typeof(Position), typeof(Rotation) });

			var emptyEntity = manager.CreateEntity();
			var positionEntity = manager.CreateEntity(positionArchetype);
			var positionAndRotationEntity = manager.CreateEntity(positionAndRotationArchetype);
			
			Assert.False(manager.HasComponentData<Position>(emptyEntity));
			Assert.False(manager.HasComponentData<Rotation>(emptyEntity));

			Assert.True(manager.HasComponentData<Position>(positionEntity));
			Assert.False(manager.HasComponentData<Rotation>(positionEntity));

			Assert.True(manager.HasComponentData<Position>(positionAndRotationEntity));
			Assert.True(manager.HasComponentData<Rotation>(positionAndRotationEntity));
		}

		[Fact]
		public void ComponentDataIsStoredInChunk()
		{
			using EntityManager manager = new EntityManager();

			var types = new Type[] { typeof(Id) };
			var archetype = manager.CreateArchetype(types);

			var entity = manager.CreateEntity(archetype);

			manager.SetComponentData(entity, new Id(12345));

			Assert.Equal(12345, manager.GetComponentData<Id>(entity).value);

			manager.ForEach(archetype, (batch) => {
				Assert.Equal(1, batch.length);
				Assert.Equal(12345, batch.GetComponentData<Id>()[0].value);
			});
		}

		[Fact]
		public void ComponentDataIsCopied()
		{
			using EntityManager manager = new EntityManager();

			var types = new Type[] { typeof(Layer), typeof(Position), typeof(Rotation) };
			var archetype = manager.CreateArchetype(types);

			var entities = manager.CreateEntity(archetype, 1000);

			manager.SetComponentData(entities[0], new Layer(10));
			manager.SetComponentData(entities[entities.Length - 1], new Layer(100));

			var buffer = new Layer[manager.GetEntityCount(archetype)];
			manager.CopyComponentData<Layer>(archetype, buffer);

			Assert.Equal(10, buffer[0].value);
			Assert.Equal(100, buffer[buffer.Length - 1].value);

			// TODO: Test with query.
		}
	}
}
