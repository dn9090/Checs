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

			var types = new Type[] { typeof(Layer) };
			var archetype = manager.CreateArchetype(types);

			var entity = manager.CreateEntity(archetype);

			manager.SetComponentData(entity, new Layer(12345));

			Assert.Equal(12345, manager.GetComponentData<Layer>(entity).value);

			manager.ForEach(archetype, (batch) => {
				Assert.Equal(1, batch.length);
				Assert.Equal(12345, batch.GetComponentData<Layer>()[0].value);
			});
		}

		[Fact]
		public void ComponentDataIsCopied()
		{
			using EntityManager manager = new EntityManager();

			var types = new Type[] { typeof(Layer), typeof(Position), typeof(Rotation) };
			var archetype = manager.CreateArchetype(types);
			var query = manager.CreateQuery(types);

			var entities = manager.CreateEntity(archetype, 1000);

			manager.SetComponentData(entities[0], new Layer(10));
			manager.SetComponentData(entities[entities.Length - 1], new Layer(100));

			var archetypeBuffer = new Layer[manager.GetEntityCount(archetype)];
			manager.CopyComponentData<Layer>(archetype, archetypeBuffer);

			Assert.Equal(10, archetypeBuffer[0].value);
			Assert.Equal(100, archetypeBuffer[archetypeBuffer.Length - 1].value);

			var queryBuffer = new Layer[manager.GetEntityCount(query)];
			manager.CopyComponentData<Layer>(query, queryBuffer);

			Assert.Equal(10, queryBuffer[0].value);
			Assert.Equal(100, queryBuffer[queryBuffer.Length - 1].value);
		}
	}
}
