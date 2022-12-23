using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	public unsafe partial class EntityManager
	{
		/// <summary>
		/// Creates the universial query matches
		/// all archetypes (including the empty archetype).
		/// </summary>
		/// <remarks>
		/// Is equivalent to <c>EntityQuery.Universial</c> or
		/// the <c>default</c> archetype.
		/// </remarks>
		/// <returns>The universial query.</returns>
		public EntityQuery CreateQuery()
		{
			return EntityQuery.Universal;
		}

		/// <summary>
		/// Creates a query that matches only archetypes
		/// that contain the specified included component type
		/// and do not contain the specified excluded component type,
		/// unless the query already exists.
		/// </summary>
		/// <param name="includeType">The component type that archetypes must have or default.</param>
		/// <param name="excludeType">The component type that archetypes may not have or default.</param>
		/// <returns>A new or existing matching query.</returns>
		public EntityQuery CreateQuery(ComponentType includeType = default,
			ComponentType excludeType = default)
		{
			var includeHashCode = includeType.hashCode;
			var excludeHashCode = excludeType.hashCode;

			var includeCount = includeType.hashCode == 0 ? 0 : 1;
			var excludeCount = excludeType.hashCode == 0 ? 0 : 1;

			return CreateQueryInternal(&includeHashCode, includeCount, &excludeHashCode, excludeCount);
		}

		/// <summary>
		/// Creates a query that matches only archetypes
		/// that contain the specified included component types
		/// and do not contain any of the specified excluded component types,
		/// unless the query already exists.
		/// </summary>
		/// <param name="includeTypes">The component types that archetypes must have.</param>
		/// <param name="excludeTypes">The component types that archetypes may not have.</param>
		/// <returns>A new or existing matching query.</returns>
		public EntityQuery CreateQuery(ReadOnlySpan<ComponentType> includeTypes = default,
			ReadOnlySpan<ComponentType> excludeTypes = default)
		{
			var includeHashCodes = stackalloc uint[includeTypes.Length];
			var excludeHashCodes = stackalloc uint[excludeTypes.Length];

			var includeCount = TypeUtility.Sort(includeTypes, includeHashCodes);
			var excludeCount = TypeUtility.Sort(excludeTypes, excludeHashCodes);

			return CreateQueryInternal(includeHashCodes, includeCount, excludeHashCodes, excludeCount);
		}

		internal EntityQuery CreateQueryInternal(uint* includeHashCodes, int includeCount,
			uint* excludeHashCodes, int excludeCount)
		{
			var includeHashCode = xxHash.GetHashCode(includeHashCodes, includeCount);
			var excludeHashCode = xxHash.GetHashCode(excludeHashCodes, excludeCount);
			var hashCode = (includeHashCode ^ (397 * excludeHashCode)) | 0x80000000;

			if(this.lookupTable.TryGet(hashCode, out var index))
				return new EntityQuery(index);

			var bufferSize = Query.SizeOfBuffer(includeCount, excludeCount);
			var query = this.queryStore.Aquire(bufferSize);

			this.lookupTable.Add(hashCode, query->index);
			
			Query.Construct(query, this.entityStore.changeVersion, includeHashCodes, includeCount, excludeHashCodes, excludeCount);
			
			return new EntityQuery(query->index);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Query* GetQueryInternal(EntityQuery query)
		{
			return this.queryStore.queries[query.index];
		}

		/// <summary>
		/// Checks if a query matches an archetype.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <param name="archetype">The archetype to check.</param>
		/// <returns>True if the archetype matches the component types in the query.</returns>
		public bool MatchesQuery(EntityQuery query, EntityArchetype archetype)
		{
			var qry = GetQueryInternal(query);
			var arch = GetArchetypeInternal(archetype);

			return QueryUtility.Matches(qry, arch);
		}

		public bool QueriesIntersect(EntityQuery lhs, EntityQuery rhs)
		{
			var qryLhs = GetQueryInternal(lhs);
			var qryRhs = GetQueryInternal(rhs);
			
			return QueryUtility.Intersects(qryLhs, qryRhs);
		}

		/// <summary>
		/// Gets current entity count of a query by
		/// a summing the entity count in all matching archetypes.
		/// </summary>
		/// <remarks>
		/// Updates the query cache.
		/// </remarks>
		/// <param name="query">The query.</param>
		/// <returns>The number of entities.</returns>
		public int GetEntityCount(EntityQuery query)
		{
			var qry = GetQueryInternal(query);

			UpdateQueryCache(qry);

			var entityCount = 0;

			for(int i = 0; i < qry->archetypeList.count; ++i)
				entityCount += qry->archetypeList.archetypes[i]->entityCount;

			return entityCount;
		}

		/// <summary>
		/// Copies entities of the specified query to the buffer.
		/// </summary>
		/// <remarks>
		/// Updates the query cache.
		/// </remarks>
		/// <param name="query">The query.</param>
		/// <param name="entities">The destination buffer.</param>
		/// <returns>The number of copied entities.</returns>
		public int GetEntities(EntityQuery query, Span<Entity> entities)
		{
			var qry = GetQueryInternal(query);
			UpdateQueryCache(qry);
			
			var count = 0;

			for(int i = 0; i < qry->archetypeList.count && count < entities.Length; ++i)
				count += GetEntitiesInternal(qry->archetypeList.archetypes[i], entities.Slice(count));

			return count;
		}

		/// <summary>
		/// Returns a single entity of the specified query.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <returns>An entity included in the query or the default entity.</returns>
		public Entity GetEntity(EntityQuery query)
		{
			Span<Entity> buffer = stackalloc Entity[1];
			var found = GetEntities(query, buffer);
			
			return found > 0 ? buffer[0] : default;
		}

		/// <summary>
		/// Gets the matching archetype count of a query.
		/// </summary>
		/// <remarks>
		/// Updates the query cache.
		/// </remarks>
		/// <param name="query">The query.</param>
		/// <returns>The number of archetypes.</returns>
		public int GetArchetypeCount(EntityQuery query)
		{
			var qry = GetQueryInternal(query);
			UpdateQueryCache(qry);

			return qry->archetypeList.count;
		}

		/// <summary>
		/// Copies matching archetypes of the specified query to the buffer.
		/// </summary>
		/// <remarks>
		/// Updates the query cache.
		/// </remarks>
		/// <param name="query">The query.</param>
		/// <param name="archetypes">The destination buffer.</param>
		/// <returns>The number of copied archetypes.</returns>
		public int GetArchetypes(EntityQuery query, Span<EntityArchetype> archetypes)
		{
			var qry = GetQueryInternal(query);
			UpdateQueryCache(qry);

			var count = archetypes.Length > qry->archetypeList.count ? qry->archetypeList.count : archetypes.Length;

			for(int i = 0; i < count; ++i)
				archetypes[i] = new EntityArchetype(qry->archetypeList.archetypes[i]->index);

			return count;
		}

		internal void CreateUniversialQuery()
		{
			CreateQueryInternal(null, 0, null, 0);
		}

		internal void UpdateQueryCache(Query* query)
		{
			if(query->index == 0) // Rework if query is stored somewhere.
			{
				query->archetypeList.archetypes = this.archetypeStore.archetypes;
				query->archetypeList.count = this.archetypeStore.count;
				query->knownArchetypeCount = this.archetypeStore.count;
				return;
			}

			if(query->knownArchetypeCount < this.archetypeStore.count)
			{
				var count      = this.archetypeStore.count - query->knownArchetypeCount;
				var archetypes = this.archetypeStore.archetypes + query->knownArchetypeCount;
				
				query->knownArchetypeCount = this.archetypeStore.count;

				for(int i = 0; i < count; ++i)
				{
					if(QueryUtility.Matches(query, archetypes[i]))
						query->archetypeList.Add(archetypes[i]);
				}
			}
		}
	}
}