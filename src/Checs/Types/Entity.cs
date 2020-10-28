using System;

namespace Checs
{
	public readonly struct Entity : IEquatable<Entity>, IComparable<Entity>
	{
		public readonly int index;

		public readonly int version;

		internal Entity(int index, int version)
		{
			this.index = index;
			this.version = version;
		}

		public static bool operator ==(Entity lhs, Entity rhs) =>
			lhs.index == rhs.index && lhs.version == rhs.version;
		
		public static bool operator !=(Entity lhs, Entity rhs) =>
			lhs.index != rhs.index && lhs.version != rhs.version;

		public int CompareTo(Entity other) => this.index - other.index;

		public override bool Equals(object other) => this == (Entity)other;

		public bool Equals(Entity other) => this == other;

		public override int GetHashCode() => this.index;
	}
}
