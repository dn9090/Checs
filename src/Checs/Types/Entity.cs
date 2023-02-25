using System;
using System.Runtime.InteropServices;

namespace Checs
{
	/// <summary>
	/// Identifies a set of component data.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe readonly struct Entity : IEquatable<Entity>, IComparable<Entity>
	{
		internal static Type type = typeof(Entity);

		internal static uint hashCode = xxHash.GetHashCode(type.FullName);

		internal static TypeInfo info = TypeRegistry<Entity>.info;

		public readonly int index;

		public readonly uint version;
		
		public bool isNull => version == 0;

		internal Entity(int index, uint version)
		{
			this.index = index;
			this.version = version;
		}

		public static bool operator ==(Entity lhs, Entity rhs) =>
			lhs.index == rhs.index && lhs.version == rhs.version;
		
		public static bool operator !=(Entity lhs, Entity rhs) =>
			lhs.index != rhs.index || lhs.version != rhs.version;

		public int CompareTo(Entity other) => this.index - other.index;

		public override bool Equals(object other) => this == (Entity)other;

		public bool Equals(Entity other) => this == other;

		public override int GetHashCode() => this.index;

		public override string ToString()
		{
			return $"Entity({this.index}:{this.version})";
		}
	}
}
