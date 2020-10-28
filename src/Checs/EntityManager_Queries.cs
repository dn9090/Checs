using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	public unsafe partial class EntityManager
	{
		public EntityQuery CreateQuery()
		{
			Span<int> types = stackalloc int[] { TypeRegistry.emptyTypeIndex };
			return CreateQueryInternal(types);
		}

		// TODO: Excluding query.

		public EntityQuery CreateQuery(Span<Type> includeTypes)
		{
			Span<int> types = includeTypes.Length > 16
				? new int[includeTypes.Length]
				: stackalloc int[includeTypes.Length];

			for(int i = 0; i < types.Length; ++i)
				types[i] = TypeRegistry.ToTypeIndex(includeTypes[i]);

			return CreateQueryInternal(types);
		}

		internal EntityQuery CreateQueryInternal(Span<int> includeTypes)
		{
			int hashCode = 0;
			for(int i = 0; i < includeTypes.Length; ++i)
				hashCode = HashCode.Combine(hashCode, includeTypes[i]);

			if(this.queryCache->typeLookup.TryGet(hashCode, out EntityQuery query))
				return query;

			EntityQueryData queryData = new EntityQueryData();
			queryData.componentCount = includeTypes.Length;
			queryData.componentTypes = MemoryUtility.Malloc<int>(includeTypes);
			queryData.archetypeCapacity = 4;
			queryData.archetypes = MemoryUtility.MallocPtrArray<Archetype>(queryData.archetypeCapacity);

			MatchArchetypesToQueryData(&queryData);

			int index = this.queryCache->count++;

			query = new EntityQuery(index);

			this.queryCache->queries[index] = queryData;
			this.queryCache->typeLookup.Add(hashCode, query);

			return default;
		}

		internal EntityQueryData* GetUpdatedQueryData(EntityQuery query)
		{
			var queryData = &this.queryCache->queries[query.index];
			MatchArchetypesToQueryData(queryData);
			return queryData;
		}

		internal void MatchArchetypesToQueryData(EntityQueryData* queryData)
		{
			// TODO: Add SIMD intrinsics and rework the matching process.

			var count = this.archetypeStore->count;
			var archetypes = this.archetypeStore->archetypes;
			var componentTypes = new Span<int>(queryData->componentTypes, queryData->componentCount);
			var uncheckedArchetypes = this.archetypeStore->count - queryData->matchedArchetypeCount;
			var skipArchetypes = queryData->matchedArchetypeCount;

			// Update the query data to the current archetype count. This only
			// works because a created archetype cannot be removed.
			queryData->matchedArchetypeCount = count;

			if(uncheckedArchetypes < 16)
			{
				// If only a few archetypes have been added preallocate the
				// archetype array.
				if(uncheckedArchetypes > 0)
				{
					int requiredCapacity = queryData->archetypeCount + uncheckedArchetypes;
					queryData->archetypeCapacity = MemoryUtility.RoundToPowerOfTwo(requiredCapacity);
					queryData->archetypes = MemoryUtility.ReallocPtrArray(queryData->archetypes, queryData->archetypeCapacity);
				}

				for(int i = skipArchetypes; i < count; ++i)
				{
					if(ArchetypeUtility.MatchesComponentTypes(&archetypes[i], componentTypes))
					{
						var index = queryData->archetypeCount++;
						queryData->archetypes[index] = &archetypes[i];
					}
				}

				return;
			}

			for(int i = skipArchetypes; i < count; ++i)
			{
				if(ArchetypeUtility.MatchesComponentTypes(&archetypes[i], componentTypes))
				{
					var index = queryData->archetypeCount;
					var capacity = queryData->archetypeCapacity;

					if(index == capacity)
					{
						queryData->archetypeCapacity = capacity * 2;
						queryData->archetypes = MemoryUtility.ReallocPtrArray(queryData->archetypes, queryData->archetypeCapacity);
					}

					queryData->archetypes[index] = &archetypes[i];
				}
			}
		}
	}
}