using System;
using System.Collections.Generic;
using Xunit;

namespace Checs.Tests
{
	public partial class EntityManagerTests_Iterators
	{
		[Fact]
		public void ForEachReadsAndWrites()
		{
			using EntityManager manager = new EntityManager();

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>()
				});
				var entities = new Entity[100];
				manager.CreateEntity(archetype, entities);

				var entityCount = 0;

				manager.ForEach(archetype, (table, _) => {
					var positions = table.GetComponentDataReadOnly<Position>();
					var rotations = table.GetComponentData<Rotation>();
					var scales    = table.GetComponentData<Scale>();

					for(int i = 0; i < table.length; ++i)
						rotations[i] = new Rotation(positions[i].x * scales[i].x, positions[i].y * scales[i].y, positions[i].z * scales[i].z, 0f);

					entityCount += table.length;
				});

				Assert.Equal(entities.Length, entityCount);
			}
		}

		[Fact]
		public void ForEachBlocksStructuralChanges()
		{
			using var manager = new EntityManager();
			
			var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
			var entities = new Entity[10];
			manager.CreateEntity(archetype, entities);

			Assert.Throws<InvalidOperationException>(() => {
				manager.ForEach(archetype, (table, manager) => {
					manager.DestroyEntity(entities);
				});				
			});
		}
	}
}
