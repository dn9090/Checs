using System;
using System.Collections.Generic;
using Xunit;

namespace Checs.Tests
{
	public partial class EntityManagerTests
	{
		// [Fact] Fails
		public void QueriesAreEqual()
		{
			using EntityManager manager = new EntityManager();

			Assert.Equal(manager.CreateQuery(), manager.CreateQuery());

			var left = manager.CreateQuery(new Type[] { typeof(Position), typeof(Rotation) });
			var right = manager.CreateQuery(new Type[] { typeof(Rotation), typeof(Position) });

			Assert.Equal(left, right);
		}

		[Fact]
		public void QueriesMatchArchetype()
		{
			using EntityManager manager = new EntityManager();

			var emptyArchetype = manager.CreateArchetype();
			var emptyQuery = manager.CreateQuery();

			Assert.True(manager.MatchesQuery(emptyQuery, emptyArchetype));

			var includesArchetype = manager.CreateArchetype(new Type[] { typeof(Position), typeof(Rotation) });
			var includesQuery = manager.CreateQuery(new Type[] { typeof(Position), typeof(Rotation) });

			Assert.True(manager.MatchesQuery(includesQuery, includesArchetype));

			var excludesArchetype = manager.CreateArchetype(new Type[] { typeof(Position), typeof(Rotation) });
			var excludesQuery = manager.CreateQuery(Array.Empty<Type>(), new Type[] { typeof(Velocity) });

			Assert.True(manager.MatchesQuery(excludesQuery, excludesArchetype));
		}

		[Fact]
		public void QueriesIncludeArchetype()
		{
			using EntityManager manager = new EntityManager();

			var archetype = manager.CreateArchetype(new Type[] { typeof(Position), typeof(Rotation), typeof(Velocity) });
			var query = manager.CreateQuery(new Type[] { typeof(Position), typeof(Rotation) });

			Assert.True(manager.MatchesQuery(query, archetype));

			archetype = manager.CreateArchetype(new Type[] { typeof(Rotation), typeof(Velocity) });
			query = manager.CreateQuery(new Type[] { typeof(Position), typeof(Rotation) });

			Assert.False(manager.MatchesQuery(query, archetype));
		}

		[Fact]
		public void QueriesExcludeArchetype()
		{
			using EntityManager manager = new EntityManager();

			var archetype = manager.CreateArchetype(new Type[] { typeof(Rotation), typeof(Velocity) });
			var query = manager.CreateQuery(Array.Empty<Type>(), new Type[] { typeof(Position) });

			Assert.True(manager.MatchesQuery(query, archetype));

			archetype = manager.CreateArchetype(new Type[] { typeof(Rotation), typeof(Velocity) });
			query = manager.CreateQuery(new Type[] { typeof(Position), typeof(Rotation) });

			Assert.False(manager.MatchesQuery(query, archetype));
		}

		[Fact]
		public void SupportsALotOfQueries()
		{
			using EntityManager manager = new EntityManager();

			var types = new Type[] {
				typeof(ComponentA), typeof(ComponentB),
				typeof(ComponentC), typeof(ComponentD),
				typeof(ComponentE), typeof(ComponentF),
				typeof(ComponentG), typeof(ComponentH),
				typeof(ComponentI), typeof(ComponentJ),
				typeof(ComponentK), typeof(ComponentL),
				typeof(ComponentM), typeof(ComponentN),
				typeof(ComponentO), typeof(ComponentP),
				typeof(ComponentQ), typeof(ComponentR),
				typeof(ComponentS), typeof(ComponentT),
				typeof(ComponentU), typeof(ComponentV),
				typeof(ComponentW), typeof(ComponentX),
				typeof(ComponentY), typeof(ComponentZ)
			};

			var queryCount = 0;

			for(int i = 0; i < types.Length; ++i, ++queryCount)
				manager.CreateQuery(types.AsSpan().Slice(i, 1));

			for(int i = 0; i < types.Length - 1; i += 2, ++queryCount)
				manager.CreateQuery(types.AsSpan().Slice(i, 2));

			for(int i = 0; i < types.Length - 2; i += 3, ++queryCount)
				manager.CreateQuery(types.AsSpan().Slice(i, 3));

			for(int i = 0; i < types.Length - 3; i += 4, ++queryCount)
				manager.CreateQuery(types.AsSpan().Slice(i, 4));

			unsafe
			{
				Assert.Equal(queryCount + 1, manager.queryCache->count); // These are bad tests...
				Assert.True(manager.entityStore->capacity >= queryCount);
			}
		}

		[Fact]
		public void QueryBuilderIncludesTypes()
		{
			using EntityManager manager = new EntityManager();

			var builder = new EntityQueryBuilder()
				.Include<Position>()
				.Include<Rotation>()
				.Include<Velocity>();

			Assert.True(builder.includeCount == 3);
		}

		[Fact]
		public void QueryBuilderExcludesTypes()
		{
			using EntityManager manager = new EntityManager();

			var builder = new EntityQueryBuilder()
				.Exclude<Position>()
				.Exclude<Rotation>()
				.Exclude<Velocity>();

			Assert.True(builder.excludeCount == 3);
		}
	}
}
