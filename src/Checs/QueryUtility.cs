using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;

namespace Checs
{
	internal static unsafe class QueryUtility
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint GetHashCode(uint* includeHashCodes, int includeCount,
			uint* excludeHashCodes, int excludeCount)
		{
			var includeHashCode = xxHash.GetHashCode(includeHashCodes, includeCount);
			var excludeHashCode = xxHash.GetHashCode(excludeHashCodes, excludeCount);

			// First bit is always one to avoid collisions with archetypes.
			return(includeHashCode ^ (397 * excludeHashCode)) | 0x80000000;
		}

		public static bool Matches(Query* query, Archetype* archetype)
		{
			var hashCodes = Archetype.GetComponentHashCodes(archetype);
			return Matches(query, hashCodes, archetype->componentCount);
		}

		public static bool Matches(Query* query, uint* componentHashCodes, int componentCount)
		{
			if(query->includeCount > componentCount)
				return false;

			var includeHashCodes = Query.GetIncludeHashCodes(query);
			var excludeHashCodes = Query.GetExcludeHashCodes(query);

			for(int i = 0, j = 0; i < query->includeCount; ++i)
			{
				while(includeHashCodes[i] > componentHashCodes[j] && j < componentCount)
					++j;
				
				if(includeHashCodes[i] != componentHashCodes[j])
					return false;
			}

			for(int i = 0, j = 0; i < query->excludeCount; ++i)
			{
				while(excludeHashCodes[i] > componentHashCodes[j] && j < componentCount)
					++j;
				
				if(excludeHashCodes[i] == componentHashCodes[j])
					return false;
			}

			return true;
		}

		public static bool Intersects(Query* lhs, Query* rhs)
		{
			if(lhs->includeCount < rhs->includeCount)
			{
				var temp = lhs;
				lhs = rhs;
				rhs = temp;
			}

			var lhsIncludeCount     = lhs->includeCount;
			var lhsIncludeHashCodes = Query.GetIncludeHashCodes(lhs);

			var rhsIncludeCount     = rhs->includeCount;
			var rhsExcludeCount     = rhs->excludeCount;
			var rhsIncludeHashCodes = Query.GetIncludeHashCodes(rhs);
			var rhsExcludeHashCodes = Query.GetExcludeHashCodes(rhs);

			{
				var lhsIndex = 0;
				var rhsIndex = 0;

				while(lhsIndex < lhsIncludeCount && rhsIndex < rhsIncludeCount)
				{
					if(lhsIncludeHashCodes[lhsIndex] > rhsIncludeHashCodes[rhsIndex])
						return false;

					if(lhsIncludeHashCodes[lhsIndex] < rhsIncludeHashCodes[rhsIndex])
					{
						++lhsIndex;
						continue;
					}

					++lhsIndex;
					++rhsIndex;
				}

				if(rhsIndex < rhsIncludeCount)
					return false;
			}
			
			{
				var lhsIndex = 0;
				var rhsIndex = 0;

				while(rhsIndex < rhsExcludeCount && lhsIndex < lhsIncludeCount)
				{
					if(rhsExcludeHashCodes[rhsIndex] > lhsIncludeHashCodes[lhsIndex])
					{
						++lhsIndex;
						continue;
					}

					if(rhsExcludeHashCodes[rhsIndex] == lhsIncludeHashCodes[lhsIndex])
						return false;

					++rhsIndex;
				}
			}

			return true;
		}
	}
}
