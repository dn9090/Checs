using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	internal unsafe struct EntityQueryData
	{
		public int matchedArchetypeCount;

		public int archetypeCount;

		public int archetypeCapacity;

		public int componentCount;

		public int* componentTypes;

		public Archetype** archetypes;
	}
}