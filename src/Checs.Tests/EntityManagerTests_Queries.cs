using System;
using System.Collections.Generic;
using Xunit;

namespace Checs.Tests
{
	public partial class EntityManagerTests_Queries
	{
		[Fact]
		public void Equal()
		{
			using EntityManager manager = new EntityManager();

			{
				var lhs = manager.CreateQuery(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var rhs = manager.CreateQuery(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});

				Assert.Equal(lhs, rhs);
			}

			{
				var lhs = manager.CreateQuery(excludeTypes: new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var rhs = manager.CreateQuery(excludeTypes: new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});

				Assert.Equal(lhs, rhs);
			}

			{
				var lhs = manager.CreateQuery(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				}, new[] {
					ComponentType.Of<Scale>(),
					ComponentType.Of<Velocity>()
				});
				var rhs = manager.CreateQuery(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				}, new[] {
					ComponentType.Of<Scale>(),
					ComponentType.Of<Velocity>()
				});

				Assert.Equal(lhs, rhs);
			}

			{
				var lhs = manager.CreateQuery(new[] {
					ComponentType.Of<Position>()
				});
				var rhs = manager.CreateQuery(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Position>()
				});

				Assert.Equal(lhs, rhs);
			}

			{
				var lhs = manager.CreateQuery(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var rhs = manager.CreateQuery(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});

				Assert.Equal(lhs, rhs);
			}

			{
				var lhs = manager.CreateQuery(excludeTypes: new[] {
					ComponentType.Of<Position>()
				});
				var rhs = manager.CreateQuery(excludeTypes: new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Position>()
				});

				Assert.Equal(lhs, rhs);
			}

			{
				var lhs = manager.CreateQuery(excludeTypes: new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var rhs = manager.CreateQuery(excludeTypes: new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});

				Assert.Equal(lhs, rhs);
			}
		}

		[Fact]
		public void MatchArchetype()
		{
			using EntityManager manager = new EntityManager();

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var includeQuery = manager.CreateQuery(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var excludeQuery = manager.CreateQuery(excludeTypes: new[] {
					ComponentType.Of<Scale>(),
					ComponentType.Of<Velocity>()
				});

	
				Assert.True(manager.MatchesQuery(includeQuery, archetype));
				Assert.True(manager.MatchesQuery(excludeQuery, archetype));
			}

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var includeQuery = manager.CreateQuery(new[] {
					ComponentType.Of<Scale>(),
					ComponentType.Of<Velocity>()
				});
				var excludeQuery = manager.CreateQuery(excludeTypes: new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
	
				Assert.False(manager.MatchesQuery(includeQuery, archetype));
				Assert.False(manager.MatchesQuery(excludeQuery, archetype));
			}
		}

		[Fact]
		public void IntersectsQuery()
		{
			using EntityManager manager = new EntityManager();

			{
				var lhs = manager.CreateQuery(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var rhs = manager.CreateQuery();

	
				Assert.True(manager.QueriesIntersect(lhs, rhs));
				Assert.True(manager.QueriesIntersect(rhs, lhs));
			}

			{
				var lhs = manager.CreateQuery(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var rhs = manager.CreateQuery(new[] {
					ComponentType.Of<Position>()
				});

	
				Assert.True(manager.QueriesIntersect(lhs, rhs));
				Assert.True(manager.QueriesIntersect(rhs, lhs));
			}

			{
				var lhs = manager.CreateQuery(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Velocity>(),
					ComponentType.Of<Scale>()
				});
				var rhs = manager.CreateQuery(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Scale>()
				});

	
				Assert.True(manager.QueriesIntersect(lhs, rhs));
				Assert.True(manager.QueriesIntersect(rhs, lhs));
			}

			{
				var lhs = manager.CreateQuery(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Velocity>()
				});
				var rhs = manager.CreateQuery(includeTypes: new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Velocity>()
				}, excludeTypes: new[] {
					ComponentType.Of<Scale>()
				});

	
				Assert.True(manager.QueriesIntersect(lhs, rhs));
				Assert.True(manager.QueriesIntersect(rhs, lhs));
			}
		}

		[Fact]
		public void NotIntersectsQuery()
		{
			using EntityManager manager = new EntityManager();

			{
				var lhs = manager.CreateQuery(new[] {
					ComponentType.Of<Position>()
				});
				var rhs = manager.CreateQuery(new[] {
					ComponentType.Of<Rotation>()
				});

	
				Assert.False(manager.QueriesIntersect(lhs, rhs));
				Assert.False(manager.QueriesIntersect(rhs, lhs));
			}

			{
				var lhs = manager.CreateQuery(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var rhs = manager.CreateQuery(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Scale>()
				});

	
				Assert.False(manager.QueriesIntersect(lhs, rhs));
				Assert.False(manager.QueriesIntersect(rhs, lhs));
			}

			{
				var lhs = manager.CreateQuery(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Velocity>()
				});
				var rhs = manager.CreateQuery(includeTypes: new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				}, excludeTypes: new[] {
					ComponentType.Of<Velocity>()
				});

	
				Assert.False(manager.QueriesIntersect(lhs, rhs));
				Assert.False(manager.QueriesIntersect(rhs, lhs));
			}

			{
				var lhs = manager.CreateQuery(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>()
				});
				var rhs = manager.CreateQuery(includeTypes: new[] {
					ComponentType.Of<Position>(),
				}, excludeTypes: new[] {
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Velocity>()
				});

	
				Assert.False(manager.QueriesIntersect(lhs, rhs));
				Assert.False(manager.QueriesIntersect(rhs, lhs));
			}
		}

		[Fact]
		public void UniversalQueryMatchesAllEntities()
		{
			using EntityManager manager = new EntityManager();

			{
				var archetype = manager.CreateArchetype();
				var query = manager.CreateQuery();

				Assert.True(manager.MatchesQuery(query, archetype));
			}

			{
				var archetype = manager.CreateArchetype(new[] {
					ComponentType.Of<Position>(),
					ComponentType.Of<Rotation>(),
					ComponentType.Of<Scale>(),
					ComponentType.Of<Velocity>()
				});
				var query = manager.CreateQuery();
				
				Assert.True(manager.MatchesQuery(query, archetype));
			}
		}
	}
}
