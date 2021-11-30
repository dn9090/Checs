using System;

namespace Checs
{
	public readonly struct EntityQuery : IEquatable<EntityQuery>, IComparable<EntityQuery>
	{
		public readonly int index;

		internal EntityQuery(int index)
		{
			this.index = index;
		}

		public static bool operator ==(EntityQuery lhs, EntityQuery rhs) =>
			lhs.index == rhs.index;
		
		public static bool operator !=(EntityQuery lhs, EntityQuery rhs) =>
			lhs.index != rhs.index;

		public int CompareTo(EntityQuery other) => this.index - other.index;

		public override bool Equals(object other) => this == (EntityQuery)other;

		public bool Equals(EntityQuery other) => this == other;

		public override int GetHashCode() => this.index;
	}
}
