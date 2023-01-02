using System;

namespace Checs
{
	/// <summary>
	/// Identifier for one unique set of component types.
	/// </summary>
	public readonly struct EntityArchetype : IEquatable<EntityArchetype>, IComparable<EntityArchetype>
	{
		public bool isEmpty => this.index == 0;

		public readonly int index; // This could actually be a pointer, because Archetypes can not be removed.

		internal EntityArchetype(int index)
		{
			this.index = index;
		}

		public static EntityArchetype empty => new EntityArchetype();

		public static bool operator ==(EntityArchetype lhs, EntityArchetype rhs) =>
			lhs.index == rhs.index;
		
		public static bool operator !=(EntityArchetype lhs, EntityArchetype rhs) =>
			lhs.index != rhs.index;

		public int CompareTo(EntityArchetype other) => this.index - other.index;

		public override bool Equals(object other) => this == (EntityArchetype)other;

		public bool Equals(EntityArchetype other) => this == other;

		public override int GetHashCode() => this.index;

		public override string ToString()
		{
			return Equals(empty) ? "EntityArchetype.empty" : $"EntityArchetype({this.index})";
		}
	}
}
