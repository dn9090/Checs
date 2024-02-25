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
			var it = new EntityIterator();

			Assert.False(it.TryNext(out _));
		}

		[Fact]
		public void ThrowsOnStructuralChanges()
		{
			using var manager = new EntityManager();
			
			{
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
				var entities = new Entity[10];
				manager.CreateEntity(archetype, entities);
				
				Assert.Throws<InvalidOperationException>(() => {
					var it = manager.GetIterator(archetype);

					while(it.TryNext(out var table))
						manager.DestroyEntity(entities);
				});
			}
			
			{
				var query = manager.CreateQuery(includeType: ComponentType.Of<Position>());
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());
				var entities = new Entity[10];
				manager.CreateEntity(archetype, entities);
				
				Assert.Throws<InvalidOperationException>(() => {
					var it = manager.GetIterator(query);
					
					while(it.TryNext(out var table))
						manager.DestroyEntity(entities);
				});
			}
		}
	}
}
