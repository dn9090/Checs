using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Checs.Tests
{
	public partial class EntityManagerTests_Archetype
	{
		[Fact]
		public void Creatable()
		{
			using EntityManager manager = new EntityManager();

			{
				var archetypeCount = manager.archetypeCount;
				var archetype = manager.CreateArchetype(ComponentType.Of<Position>());

				Assert.Equal(archetypeCount + 1, manager.archetypeCount);
				Assert.True(archetype.index > 0);
				Assert.Equal(2, manager.GetComponentCount(archetype));
			}

			{
				var archetypeCount = manager.archetypeCount;
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				
				Assert.Equal(archetypeCount + 1, manager.archetypeCount);
				Assert.True(archetype.index > 0);
				Assert.Equal(3, manager.GetComponentCount(archetype));
			}

			{
				var archetypes = new HashSet<EntityArchetype>();
				var types = new[]
				{
					ComponentType.Of<ComponentA>(), ComponentType.Of<ComponentB>(), ComponentType.Of<ComponentC>(), ComponentType.Of<ComponentD>(),
					ComponentType.Of<ComponentE>(), ComponentType.Of<ComponentF>(), ComponentType.Of<ComponentG>(), ComponentType.Of<ComponentH>(),
					ComponentType.Of<ComponentI>(), ComponentType.Of<ComponentJ>(), ComponentType.Of<ComponentK>(), ComponentType.Of<ComponentL>(),
					ComponentType.Of<ComponentM>(), ComponentType.Of<ComponentN>(), ComponentType.Of<ComponentO>(), ComponentType.Of<ComponentP>(),
					ComponentType.Of<ComponentQ>(), ComponentType.Of<ComponentR>(), ComponentType.Of<ComponentS>(), ComponentType.Of<ComponentT>(),
					ComponentType.Of<ComponentU>(), ComponentType.Of<ComponentV>(), ComponentType.Of<ComponentW>(), ComponentType.Of<ComponentX>(),
					ComponentType.Of<ComponentY>(), ComponentType.Of<ComponentZ>()
				};

				foreach(var type in types)
					archetypes.Add(manager.CreateArchetype(type));
			
				Assert.Equal(types.Length, archetypes.Count);
			}
		}

		[Fact]
		public void Equal()
		{
			using EntityManager manager = new EntityManager();

			{
				var lhs = manager.CreateArchetype(ComponentType.Of<Position>());
				var rhs = manager.CreateArchetype(ComponentType.Of<Position>());

				Assert.Equal(lhs, rhs);
			}

			{
				var lhs = manager.CreateArchetype(ComponentType.Of<Position>());
				var rhs = manager.CreateArchetype(new[] { ComponentType.Of<Position>() });

				Assert.Equal(lhs, rhs);
			}

			{
				var lhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>(),
					ComponentType.Of<Velocity>()
				});
				var rhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>(),
					ComponentType.Of<Velocity>()
				});

				Assert.Equal(lhs, rhs);
			}

			{
				var lhs = manager.CreateArchetype(ComponentType.Of<Position>());
				var rhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Position>()
				});

				Assert.Equal(lhs, rhs);
			}

			{
				var lhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var rhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});

				Assert.Equal(lhs, rhs);
			}

			{
				var lhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var rhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Entity>(),
					ComponentType.Of<Rotation>()
				});

				Assert.Equal(lhs, rhs);
			}

			{
				var types = new[]
				{
					ComponentType.Of<ComponentA>(), ComponentType.Of<ComponentB>(), ComponentType.Of<ComponentC>(), ComponentType.Of<ComponentD>(),
					ComponentType.Of<ComponentE>(), ComponentType.Of<ComponentF>(), ComponentType.Of<ComponentG>(), ComponentType.Of<ComponentH>(),
					ComponentType.Of<ComponentI>(), ComponentType.Of<ComponentJ>(), ComponentType.Of<ComponentK>(), ComponentType.Of<ComponentL>(),
					ComponentType.Of<ComponentM>(), ComponentType.Of<ComponentN>(), ComponentType.Of<ComponentO>(), ComponentType.Of<ComponentP>(),
					ComponentType.Of<ComponentQ>(), ComponentType.Of<ComponentR>(), ComponentType.Of<ComponentS>(), ComponentType.Of<ComponentT>(),
					ComponentType.Of<ComponentU>(), ComponentType.Of<ComponentV>(), ComponentType.Of<ComponentW>(), ComponentType.Of<ComponentX>(),
					ComponentType.Of<ComponentY>(), ComponentType.Of<ComponentZ>()
				};

				var lhs = manager.CreateArchetype(types);
				var rhs = manager.CreateArchetype(types);

				Assert.Equal(lhs, rhs);
			}
		}

		[Fact]
		public void NotEqual()
		{
			using EntityManager manager = new EntityManager();

			{
				var lhs = manager.CreateArchetype();
				var rhs = manager.CreateArchetype(ComponentType.Of<Position>());

				Assert.NotEqual(lhs, rhs);
			}

			{
				var lhs = manager.CreateArchetype(ComponentType.Of<Position>());
				var rhs = manager.CreateArchetype(ComponentType.Of<Rotation>());

				Assert.NotEqual(lhs, rhs);
			}

			{
				var lhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>()
				});
				var rhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>(),
					ComponentType.Of<Velocity>()
				});

				Assert.NotEqual(lhs, rhs);
			}

			{
				var lhsTypes = new[]
				{
					ComponentType.Of<ComponentA>(), ComponentType.Of<ComponentB>(), ComponentType.Of<ComponentC>(),
					ComponentType.Of<ComponentD>(), ComponentType.Of<ComponentE>(), ComponentType.Of<ComponentF>(),
					ComponentType.Of<ComponentG>(), ComponentType.Of<ComponentH>(), ComponentType.Of<ComponentI>(),
					ComponentType.Of<ComponentJ>(), ComponentType.Of<ComponentK>(), ComponentType.Of<ComponentL>(),
					ComponentType.Of<ComponentM>()
				};

				var rhsTypes = new[]
				{
					ComponentType.Of<ComponentN>(), ComponentType.Of<ComponentO>(), ComponentType.Of<ComponentP>(),
					ComponentType.Of<ComponentQ>(), ComponentType.Of<ComponentR>(), ComponentType.Of<ComponentS>(),
					ComponentType.Of<ComponentT>(), ComponentType.Of<ComponentU>(), ComponentType.Of<ComponentV>(),
					ComponentType.Of<ComponentW>(), ComponentType.Of<ComponentX>(), ComponentType.Of<ComponentY>(),
					ComponentType.Of<ComponentZ>()
				};

				var lhs = manager.CreateArchetype(lhsTypes);
				var rhs = manager.CreateArchetype(rhsTypes);

				Assert.NotEqual(lhs, rhs);
			}
		}

		[Fact]
		public void CanConsistOfGenericTypes()
		{
			using EntityManager manager = new EntityManager();

			{
				var lhs = manager.CreateArchetype(ComponentType.Of<Generic<int>>());
				var rhs = manager.CreateArchetype(ComponentType.Of<Generic<int>>());

				Assert.Equal(lhs, rhs);
			}

			{
				var lhs = manager.CreateArchetype(ComponentType.Of<Generic<int>>());
				var rhs = manager.CreateArchetype(ComponentType.Of<Generic<long>>());

				Assert.NotEqual(lhs, rhs);
			}
		}

		[Fact]
		public void Extendable()
		{
			using EntityManager manager = new EntityManager();

			{
				var original = manager.CreateArchetype();
				var extended = manager.CreateArchetype(original, new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var actual = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});

				Assert.NotEqual(original, extended);
				Assert.Equal(actual, extended);
			}

			{
				var original = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var extended = manager.CreateArchetype(original, new[] {
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>()
				});
				var actual = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>()
				});

				Assert.NotEqual(original, extended);
				Assert.Equal(actual, extended);
			}

			{
				var original = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var extended = manager.CreateArchetype(original, new[] {
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>(),
					ComponentType.Of<Scale>(),
					ComponentType.Of<Scale>(),
					ComponentType.Of<Entity>()
				});
				var actual = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>()
				});

				Assert.NotEqual(original, extended);
				Assert.Equal(actual, extended);
			}
		}

		[Fact]
		public void Shrinkable()
		{
			using EntityManager manager = new EntityManager();

			{
				var original = manager.CreateArchetype(ComponentType.Of<Position>());
				var excluded = manager.CreateArchetypeWithout(original, new[] {
					ComponentType.Of<Position>()
				});
				var actual = manager.CreateArchetype();

				Assert.NotEqual(original, excluded);
				Assert.Equal(actual, excluded);
			}

			{
				var original = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var excluded = manager.CreateArchetypeWithout(original, new[] {
					ComponentType.Of<Rotation>()
				});
				var actual = manager.CreateArchetype(ComponentType.Of<Position>());

				Assert.NotEqual(original, excluded);
				Assert.Equal(actual, excluded);
			}

			{
				var original = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>()
				});
				var excluded = manager.CreateArchetypeWithout(original, new[]{
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Velocity>()
				});
				var actual = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Scale>()
				});

				Assert.NotEqual(original, excluded);
				Assert.Equal(actual, excluded);
			}
		}

		[Fact]
		public void Combinable()
		{
			using EntityManager manager = new EntityManager();

			{
				var lhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var rhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});

				var combined = manager.CombineArchetypes(lhs, rhs);

				Assert.Equal(lhs, rhs);
				Assert.Equal(lhs, combined);
			}

			{
				var lhs = manager.CreateArchetype();
				var rhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});

				var combined = manager.CombineArchetypes(lhs, rhs);

				Assert.Equal(rhs, combined);
			}

			{
				var lhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var rhs = manager.CreateArchetype();

				var combined = manager.CombineArchetypes(lhs, rhs);

				Assert.Equal(lhs, combined);
			}

			{
				var lhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var rhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Scale>(),
					ComponentType.Of<Velocity>()
				});

				var combined = manager.CombineArchetypes(lhs, rhs);
				var actual = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>(),
					ComponentType.Of<Velocity>()
				});

				Assert.Equal(actual, combined);
			}

			{
				var lhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>(),
				});
				var rhs = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Scale>(),
					ComponentType.Of<Velocity>()
				});

				var combined = manager.CombineArchetypes(lhs, rhs);
				var actual = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>(),
					ComponentType.Of<Velocity>()
				});

				Assert.Equal(actual, combined);
			}
		}
	}
}
