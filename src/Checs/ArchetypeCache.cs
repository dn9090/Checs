using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Checs
{
	[StructLayout(LayoutKind.Explicit, Size = 64)]
	internal unsafe struct ArchetypeCache
	{
		public enum Op : byte
		{
			None   = 0,
			Add    = 1,
			Remove = 2
		}

		[FieldOffset(0)]
		public Archetype* archetypes;

		[FieldOffset(32)]
		public uint hashCodes;

		[FieldOffset(48)]
		public Op ops;

		[FieldOffset(52)]
		public byte states;

		[FieldOffset(56)]
		public int hand;

		public const int Capacity = 4;

		public const int Mask = Capacity - 1;

		public static Archetype* Get(ArchetypeCache* cache, uint hashCode, Op op)
		{
			var archetypes  = &cache->archetypes;
			var hashCodes   = &cache->hashCodes;
			var ops         = &cache->ops;
			var states      = &cache->states;

			var index = cache->hand;

			do
			{
				if(hashCodes[index] == hashCode && ops[index] == op)
				{
					states[index] |= 1;
					return archetypes[index];
				}

				index = (index + 1) & Mask;
			} while(index != cache->hand);

			return null;
		}

		public static void Insert(ArchetypeCache* cache, Archetype* archetype, uint hashCode, Op op)
		{
			var archetypes  = &cache->archetypes;
			var hashCodes   = &cache->hashCodes;
			var ops         = &cache->ops;
			var states      = &cache->states;

			var index = cache->hand;

			while(states[index] != 0)
			{
				states[index] = 0;
				index = (index + 1) & Mask;
			}
			
			archetypes[index] = archetype;
			hashCodes[index]  = hashCode;
			ops[index]        = op;
			states[index]     = 1;
		}
	}
}