using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	public readonly struct EntityChangeVersion : IEquatable<EntityChangeVersion>, IComparable<EntityChangeVersion>
	{
		public readonly int version;

		public EntityChangeVersion(int version)
		{
			this.version = version;
		}

		public static bool operator ==(EntityChangeVersion lhs, EntityChangeVersion rhs) =>
			lhs.version == rhs.version;
		
		public static bool operator !=(EntityChangeVersion lhs, EntityChangeVersion rhs) =>
			lhs.version != rhs.version;

		public int CompareTo(EntityChangeVersion other)
		{
			return this.version - other.version;
		}

		public override bool Equals(object other)
		{
			return this == (EntityChangeVersion)other;
		}

		public bool Equals(EntityChangeVersion other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			return this.version;
		}

		public override string ToString()
		{
			return $"EntityChangeVersion({this.version})";
		}
	}
}
