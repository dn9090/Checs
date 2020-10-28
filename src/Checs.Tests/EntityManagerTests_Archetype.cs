using System;
using System.Collections.Generic;
using Xunit;

namespace Checs.Tests
{
	public partial class EntityManagerTests
	{
		[Fact]
		public void ArchetypesAreEqual()
		{
			EntityManager manager = new EntityManager();

			var left = manager.CreateArchetype();
			var right = manager.CreateArchetype();

			Assert.Equal(left, right);

			left = manager.CreateArchetype(typeof(Position));
			right = manager.CreateArchetype(typeof(Position));

			Assert.Equal(left, right);

			left = manager.CreateArchetype(new Type[] { typeof(Position), typeof(EulerAngles) });
			right = manager.CreateArchetype(new Type[] { typeof(Position), typeof(EulerAngles) });

			Assert.Equal(left, right);

			left = manager.CreateArchetype(new Type[] { typeof(Position), typeof(EulerAngles), typeof(Teleporter) });
			right = manager.CreateArchetype(new Type[] { typeof(Position), typeof(EulerAngles), typeof(Teleporter) });

			Assert.Equal(left, right);
		}

		[Fact]
		public void ArchetypesAreNotEqual()
		{
			EntityManager manager = new EntityManager();

			var left = manager.CreateArchetype();
			var right = manager.CreateArchetype(typeof(Position));

			Assert.NotEqual(left, right);

			left = manager.CreateArchetype(new Type[] { typeof(Position), typeof(EulerAngles) });
			right = manager.CreateArchetype(new Type[] { typeof(Position), typeof(EulerAngles), typeof(Teleporter) });

			Assert.NotEqual(left, right);
		}
	}
}
