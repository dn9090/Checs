using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	public unsafe partial class EntityManager
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static int GetQueryTypeHash(Span<int> includeTypes, Span<int> excludeTypes)
		{
			int hashCode = 0;

			for(int i = 0; i < includeTypes.Length; ++i)
				hashCode = HashCode.Combine(hashCode, includeTypes[i]);

			for(int i = 0; i < excludeTypes.Length; ++i)
				hashCode = HashCode.Combine(hashCode, -excludeTypes[i]);

			return hashCode;
		}

		public bool MatchesQuery(EntityQuery query, EntityArchetype archetype)
		{
			var queryData = &this.queryCache->queries[query.index];
			return ArchetypeMatchesQueryFilter(GetArchetypeInternal(archetype), queryData);
		}

		public EntityQuery CreateQuery() => CreateQueryInternal(Span<int>.Empty, 0, 0);

		// TODO: Excluding query.

		public EntityQuery CreateQuery(Span<Type> includeTypes) => CreateQuery(includeTypes, Span<Type>.Empty);

		public EntityQuery CreateQuery(Span<Type> includeTypes, Span<Type> excludeTypes)
		{
			var typeCount = includeTypes.Length + excludeTypes.Length;

			Span<int> types = typeCount > 16
				? new int[typeCount]
				: stackalloc int[typeCount];

			for(int i = 0; i < includeTypes.Length; ++i)
				types[i] = TypeRegistry.ToTypeIndex(includeTypes[i]);
			
			for(int i = 0; i < excludeTypes.Length; ++i)
				types[i + includeTypes.Length] = TypeRegistry.ToTypeIndex(excludeTypes[i]);

			// TODO: Sort types.
			// TODO: Same as archetypes: Allow only distinct types.

			return CreateQueryInternal(types, includeTypes.Length, excludeTypes.Length);
		}

		internal EntityQuery CreateQueryInternal(Span<int> types, int includeTypesCount, int excludeTypesCount)
		{
			int hashCode = GetQueryTypeHash(types.Slice(0, includeTypesCount), types.Slice(includeTypesCount));

			if(this.queryCache->typeLookup.TryGet(hashCode, out EntityQuery query))
				return query;

			this.queryCache->EnsureCapacity();

			int index = this.queryCache->count++;

			// InitBlock the struct?
			EntityQueryData* queryData = this.queryCache->queries + index;
			queryData->matchedArchetypeCount = 0;
			queryData->archetypeCount = 0;
			queryData->includeTypesCount = includeTypesCount;
			queryData->excludeTypesCount = excludeTypesCount;
			queryData->componentTypes = null;
			queryData->archetypes = MemoryUtility.MallocPtrArray<Archetype>(0); // This is dumb but works, otherwise realloc throws.

			if(types.Length > 0) // Because Marshal.AllocHGlobal(0) does not return IntPtr.Zero.
			{
				queryData->componentTypes = MemoryUtility.Malloc<int>(types.Length);
				types.CopyTo(new Span<int>(queryData->componentTypes, types.Length));
			}

			MatchArchetypesToQueryData(queryData); // Lazy?

			query = new EntityQuery(index);

			this.queryCache->typeLookup.Add(hashCode, query);

			return query;
		}

		internal EntityQueryData* GetUpdatedQueryData(EntityQuery query)
		{
			var queryData = &this.queryCache->queries[query.index];
			MatchArchetypesToQueryData(queryData);
			return queryData;
		}

		internal static void AppendArchetypesToQueryData(EntityQueryData* queryData, Archetype** archetypes, int count)
		{
			var size = count * sizeof(Archetype*);
			queryData->archetypes = MemoryUtility.ReallocPtrArray<Archetype>(queryData->archetypes, queryData->archetypeCount + count);
			Buffer.MemoryCopy(archetypes, queryData->archetypes + queryData->archetypeCount, size, size);
			queryData->archetypeCount += count;
		}

		internal void MatchArchetypesToQueryData(EntityQueryData* queryData)
		{
			var count = this.archetypeStore->count;
			var unmatchedArchetypeCount = count - queryData->matchedArchetypeCount;
			var archetypes = this.archetypeStore->archetypes + queryData->matchedArchetypeCount;

			// Update the query data to the current archetype count. This only
			// works because a created archetype cannot be removed.
			queryData->matchedArchetypeCount = count;

			if(unmatchedArchetypeCount == 0) // Can this be done better?
				return;

			var matchCount = 0;

			if(unmatchedArchetypeCount <= 16)
			{
				var matchingArchetypes = stackalloc Archetype*[unmatchedArchetypeCount];

				for(int i = 0; i < unmatchedArchetypeCount; ++i, ++archetypes) // This loop can be written shorter and better!
				{
					if(ArchetypeMatchesQueryFilter(archetypes, queryData))
						matchingArchetypes[matchCount++] = archetypes;
				}
				
				AppendArchetypesToQueryData(queryData, matchingArchetypes, matchCount);
			} else {
				var matchingArchetypes = MemoryUtility.MallocPtrArray<Archetype>(unmatchedArchetypeCount);

				for(int i = 0; i < unmatchedArchetypeCount; ++i, ++archetypes)
				{
					if(ArchetypeMatchesQueryFilter(archetypes, queryData))
						matchingArchetypes[matchCount++] = archetypes;
				}
				
				AppendArchetypesToQueryData(queryData, matchingArchetypes, matchCount);

				MemoryUtility.Free(matchingArchetypes);
			}
		}

		internal static bool ArchetypeMatchesQueryFilter(Archetype* archetype, EntityQueryData* query)
		{
			if(query->includeTypesCount > archetype->componentCount)
				return false;

			var includeTypes = query->componentTypes;
			
			// TODO: SSE2
			// if sorted save i and j and increment...

			for(int i = 0; i < query->includeTypesCount; ++i)
			{
				for(int j = 0; j < archetype->componentCount; ++j)
					if(includeTypes[i] == archetype->componentTypes[j])
						goto matched;

				return false;
				matched:
					continue;
			}

			var excludeTypes = query->componentTypes + query->includeTypesCount;

			for(int i = 0; i < query->excludeTypesCount; ++i)
			{
				for(int j = 0; j < archetype->componentCount; ++j)
					if(excludeTypes[i] == archetype->componentTypes[j])
						return false;
			}

			return true;
		}

		/*
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

			if(uncheckedArchetypes <= 16)
			{
				// If only a few archetypes have been added preallocate the
				// archetype array.
				if(uncheckedArchetypes > 0)
				{
					int requiredCapacity = queryData->archetypeCount + uncheckedArchetypes;
					queryData->archetypeCapacity = MemoryUtility.RoundToPowerOfTwo(requiredCapacity); // I think this could be = requiredCapacity
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
					var index = queryData->archetypeCount++;
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
		*/
	}
}