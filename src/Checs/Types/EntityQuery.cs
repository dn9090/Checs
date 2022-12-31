using System;

namespace Checs
{
	public readonly struct EntityQuery : IEquatable<EntityQuery>, IComparable<EntityQuery>
	{
		public bool isUniversal => this.index == 0;

		public readonly int index;

		internal EntityQuery(int index)
		{
			this.index = index;
		}

		public static EntityQuery universal => new EntityQuery();

		public static bool operator ==(EntityQuery lhs, EntityQuery rhs) =>
			lhs.index == rhs.index;
		
		public static bool operator !=(EntityQuery lhs, EntityQuery rhs) =>
			lhs.index != rhs.index;

		public int CompareTo(EntityQuery other) => this.index - other.index;

		public override bool Equals(object other) => this == (EntityQuery)other;

		public bool Equals(EntityQuery other) => this == other;

		public override int GetHashCode() => this.index;

		public override string ToString()
		{
			return $"EntityQuery({this.index})";
		}
	}
}
