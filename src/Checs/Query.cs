using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Checs
{
	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct Query : IDisposable
	{
		[FieldOffset(0)]
		public int includeCount;

		[FieldOffset(4)]
		public int excludeCount;

		[FieldOffset(8)]
		public ArchetypeList archetypeList;

		[FieldOffset(24)]
		public int knownArchetypeCount;

		[FieldOffset(28)]
		public int index;

		[FieldOffset(64)]
		public fixed byte buffer[4];

		public const int Size = 64;

		public const int Alignment = 64;

		public void Dispose()
		{
			if(index == 0) // TODO: ~~ This looks like shit. ~~
				this.archetypeList = default;
			else
				this.archetypeList.Dispose();
		}

		public static void Construct(Query* query, uint* includeHashCodes, int includeCount,
			uint* excludeHashCodes, int excludeCount)
		{
			query->includeCount        = includeCount;
			query->excludeCount        = excludeCount;
			query->archetypeList       = new ArchetypeList();
			query->knownArchetypeCount = 0;
			
			Unsafe.CopyBlockUnaligned(GetIncludeHashCodes(query), includeHashCodes, (uint)(sizeof(uint) * includeCount));
			Unsafe.CopyBlockUnaligned(GetExcludeHashCodes(query), excludeHashCodes, (uint)(sizeof(uint) * excludeCount));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SizeOfBuffer(int includeCount, int excludeCount)
		{
			return sizeof(uint) * (includeCount + excludeCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint* GetIncludeHashCodes(Query* query)
		{
			return (uint*)query->buffer;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint* GetExcludeHashCodes(Query* query)
		{
			return (uint*)query->buffer + query->includeCount;
		}
	}
}
