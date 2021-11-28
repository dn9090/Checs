using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct EntityQueryData
	{
		public int matchedArchetypeCount;

		public int archetypeCount;

		public int includeTypesCount;

		public int excludeTypesCount;

		public int* componentTypes;

		public Archetype** archetypes;
	}

	/*
	internal unsafe struct EntityQueryData
	{
		public int matchedArchetypeCount;

		public int archetypeCount;

		public int includeTypesCount;

		public int excludeTypesCount;

		public int* componentTypes;

		public Archetype** archetypes;
	}

	public struct EntityQueryBuilder
	{
		public int includeCount;

		public int excludeCount;

		internal int[] includeTypes;

		internal int[] excludeTypes;

		internal EntityQueryBuilder(int capacity)
		{
			this.includeCount = 0;
			this.excludeCount = 0;
			this.includeTypes = new int[capacity];
			this.excludeTypes = new int[capacity];
		}

		public static EntityQueryBuilder Include<T>()
			=> new EntityQueryBuilder(4).Include<T>();

		public static EntityQueryBuilder Exclude<T>()
			=> new EntityQueryBuilder(4).Exclude<T>();

		public EntityQueryBuilder Include<T>()
		{
			...
		}

		public EntityQueryBuilder Exclude<T>()
		{
			...
		}
	}

	var builder = EntityQueryBuilder
		.Include<Position>()
		.Include<Rotation>()
		.Exclude<Layer();

	var query = world.CreateQuery(builder);

	public bool MatchesQuery(Archetype* archetype, EntityQueryData* query)
	{
		if(query->includeTypesCount > archetype->componentTypeCount)
			return false;

		// if sorted save i and j and increment...

		for(int i = 0; i < query->includeTypesCount; ++i)
		{
			for(int j = 0; j < archetype->componentTypeCount; ++j)
				if(query->includeTypes[i] == archetype->componentTypes[j])
					goto matched;

			return false;
			matched:
		}

		for(int i = 0; i < query->excludeTypesCount; ++i)
		{
			for(int j = 0; j < archetype->componentTypeCount; ++j)
				if(query->excludeTypes[i] == archetype->componentTypes[j])
					return false;
		}

		return true;
	}


	*/
}