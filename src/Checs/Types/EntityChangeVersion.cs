using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	public readonly struct EntityCommit : IEquatable<EntityCommit>, IComparable<EntityCommit>
	{
		public readonly int version;

		public EntityCommit(int version)
		{
			this.version = version;
		}

		public static bool operator ==(EntityCommit lhs, EntityCommit rhs) =>
			lhs.version == rhs.version;
		
		public static bool operator !=(EntityCommit lhs, EntityCommit rhs) =>
			lhs.version != rhs.version;

		public int CompareTo(EntityCommit other)
		{
			return this.version - other.version;
		}

		public override bool Equals(object other)
		{
			return this == (EntityCommit)other;
		}

		public bool Equals(EntityCommit other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			return this.version;
		}

		public override string ToString()
		{
			return $"EntityCommit({this.version})";
		}
	}
}
