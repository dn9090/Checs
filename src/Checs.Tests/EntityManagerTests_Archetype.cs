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
			using EntityManager manager = new EntityManager();

			var left = manager.CreateArchetype();
			var right = manager.CreateArchetype();

			Assert.Equal(left, right);

			left = manager.CreateArchetype(typeof(Position));
			right = manager.CreateArchetype(typeof(Position));

			Assert.Equal(left, right);

			left = manager.CreateArchetype(new Type[] { typeof(Position), typeof(Rotation) });
			right = manager.CreateArchetype(new Type[] { typeof(Rotation), typeof(Position) });

			Assert.Equal(left, right);

			left = manager.CreateArchetype(new Type[] { typeof(Position), typeof(Rotation), typeof(Layer) });
			right = manager.CreateArchetype(new Type[] { typeof(Layer), typeof(Position), typeof(Rotation) });

			Assert.Equal(left, right);
		}

		[Fact]
		public void ArchetypesAreEqual_Large()
		{
			using EntityManager manager = new EntityManager();

			var types = new Type[]
			{
				typeof(ComponentA), typeof(ComponentB), typeof(ComponentC), typeof(ComponentD),
				typeof(ComponentE), typeof(ComponentF), typeof(ComponentG), typeof(ComponentH),
				typeof(ComponentI), typeof(ComponentJ), typeof(ComponentK), typeof(ComponentL),
				typeof(ComponentM), typeof(ComponentN), typeof(ComponentO), typeof(ComponentP),
				typeof(ComponentQ), typeof(ComponentR), typeof(ComponentS), typeof(ComponentT)
			};

			var left = manager.CreateArchetype(types);
			var right = manager.CreateArchetype(types);

			Assert.Equal(left, right);
		}

		[Fact]
		public void ArchetypesAreNotEqual()
		{
			using EntityManager manager = new EntityManager();

			var left = manager.CreateArchetype();
			var right = manager.CreateArchetype(typeof(Position));

			Assert.NotEqual(left, right);

			left = manager.CreateArchetype(new Type[] { typeof(Position), typeof(Rotation) });
			right = manager.CreateArchetype(new Type[] { typeof(Position), typeof(Rotation), typeof(Line) });

			Assert.NotEqual(left, right);
		}

		[Fact]
		public void ArchetypesAreNotEqual_Large()
		{
			using EntityManager manager = new EntityManager();

			var lTypes = new Type[]
			{
				typeof(ComponentA), typeof(ComponentB), typeof(ComponentC), typeof(ComponentD),
				typeof(ComponentE), typeof(ComponentF), typeof(ComponentG), typeof(ComponentH),
				typeof(ComponentI), typeof(ComponentJ), typeof(ComponentK), typeof(ComponentL),
				typeof(ComponentM), typeof(ComponentN), typeof(ComponentO), typeof(ComponentP),
				typeof(ComponentQ), typeof(ComponentR), typeof(ComponentS), typeof(ComponentT)
			};

			var rTypes = new Type[]
			{
				typeof(ComponentA), typeof(ComponentB), typeof(ComponentC), typeof(ComponentD),
				typeof(ComponentE), typeof(ComponentF), typeof(ComponentG), typeof(ComponentH),
				typeof(ComponentI), typeof(ComponentJ), typeof(ComponentK), typeof(ComponentL),
				typeof(ComponentM), typeof(ComponentN), typeof(ComponentO), typeof(ComponentP),
				typeof(ComponentQ), typeof(ComponentR), typeof(ComponentS), typeof(ComponentT),
				typeof(ComponentU), typeof(ComponentV), typeof(ComponentW), typeof(ComponentX)
			};

			var left = manager.CreateArchetype(lTypes);
			var right = manager.CreateArchetype(rTypes);

			Assert.NotEqual(left, right);
		}
	}
}
