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
		public void IteratorBehavesLikeForEach()
		{
			using EntityManager manager = new EntityManager();

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>(),
					ComponentType.Of<Velocity>()
				});
				var entities = new Entity[200];
				manager.CreateEntity(archetype, entities);
				
				var forEachCount = 0;
				manager.ForEach(archetype, (table, manager) => {
					forEachCount += table.length;
				});

				var iterCount = 0;
				using var iter = manager.GetIterator(archetype);

				while(iter.TryNext(out var table))
					iterCount += table.length;

				Assert.Equal(forEachCount, iterCount);
			}
		}

		[Fact]
		public void IteratorDoesNotContinueAfterDispose()
		{
			using var manager = new EntityManager();

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var entities = new Entity[100];
				manager.CreateEntity(archetype, entities);

				var it = manager.GetIterator(archetype);
				it.Dispose();

				Assert.False(it.TryNext(out _));
			}

			{
				var query = manager.CreateQuery(new[] {
					ComponentType.Of<Velocity>()
				});
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Velocity>()
				});
				
				var entities = new Entity[100];
				manager.CreateEntity(archetype, entities);

				var it = manager.GetIterator(query);
				it.Dispose();

				Assert.False(it.TryNext(out _));
			}
		}

		[Fact]
		public void IteratorBlocksStructuralChanges()
		{
			using var manager = new EntityManager();
			
			var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
			var entities = new Entity[10];
			manager.CreateEntity(archetype, entities);

			Assert.Throws<InvalidOperationException>(() => {
				using var it = manager.GetIterator(archetype);
				manager.DestroyEntity(entities);
			});
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
