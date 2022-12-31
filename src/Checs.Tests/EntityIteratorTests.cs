using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Checs.Tests
{
	public partial class EntityIteratorTests
	{
		[Fact]
		public void DefaultDoesNothing()
		{
			using var iterator = new EntityIterator();

			Assert.False(iterator.TryNext(out _));
		}

		//[Fact]
		public void ThrowsOnStructuralChanges()
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
		public void StopsAfterDispose()
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
	}
}
