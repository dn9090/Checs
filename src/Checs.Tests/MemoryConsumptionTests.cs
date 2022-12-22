using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Checs.Tests
{
	public unsafe partial class MemoryConsumptionTests
	{
		[Fact]
		public void MovingEntitiesToSmallerArchetypesOnlyAllocatesOnce()
		{
			using var manager = new EntityManager();

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>(),
					ComponentType.Of<Velocity>()
				});
				var entities = new Entity[1000];

				manager.CreateEntity(archetype, entities);
				var chunkCount = manager.chunkStore.GetCount();
				manager.MoveEntity(entities, manager.CreateArchetype());

				Assert.Equal(chunkCount + 1, manager.chunkStore.GetCount());
			}

			{
				var srcArchetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>(),
					ComponentType.Of<Velocity>()
				});
				var dstArchetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>()
				});

				var entities = new Entity[1234];
				manager.CreateEntity(srcArchetype, entities);
				var chunkCount = manager.chunkStore.GetCount();
				manager.MoveEntity(entities, dstArchetype);

				Assert.Equal(chunkCount + 1, manager.chunkStore.GetCount());
			}
		}
	}
}
