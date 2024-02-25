using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Checs.Tests
{
	public partial class EntityTableTests
	{
		[Fact]
		public void ArrayConvertable()
		{
			var manager = new EntityManager();

			{
				var archetype = manager.CreateArchetype(
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				);
				var entities = new Entity[10];

				manager.CreateEntity(archetype, entities);
				manager.SetComponentData(entities, new Position(1f, 2f, 3f));
				manager.SetComponentData(entities, new Rotation(1f, 2f, 3f, 4f));

				var it = manager.GetIterator(archetype);

				while(it.TryNext(out var table))
				{
					var positions = table.GetComponentData<Position>();
					var rotations = table.GetComponentData<Rotation>();

					var positionArray = table.ToArray(ComponentType.Of<Position>());
					var rotationArray = table.ToArray(ComponentType.Of<Rotation>());

					Assert.Equal(positions[0], (Position)positionArray.GetValue(0));
					Assert.Equal(rotations[0], (Rotation)rotationArray.GetValue(0));
				
					Assert.Equal(table.length, positionArray.Length);
					Assert.Equal(table.length, rotationArray.Length);
				}
			}

			{
				var archetype = manager.CreateArchetype(
					ComponentType.Of<Position>()
				);
				var entities = new Entity[100];

				manager.CreateEntity(archetype, entities);
				manager.SetComponentData(entities, new Position(1f, 2f, 3f));

				var it = manager.GetIterator(archetype);

				while(it.TryNext(out var table))
				{
					var positions = table.GetComponentData<Position>();

					var end = positions.Length - 1;
					
					positions[0]   = new Position(11f, 12f, 13f);
					positions[end] = new Position(21f, 22f, 23f);

					var positionArray = table.ToArray(ComponentType.Of<Position>());

					Assert.Equal(positions[0], (Position)positionArray.GetValue(0));
					Assert.Equal(positions[end], (Position)positionArray.GetValue(end));
				}
			}
		}
	}
}
