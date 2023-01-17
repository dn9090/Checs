using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Checs
{
	/// <summary>
	/// Holds type information of a component.
	/// </summary>
	/// <remarks>
	/// Note that at the moment only unmanaged types are supported.
	/// </remarks>
	public readonly struct ComponentType : IEquatable<ComponentType>, IComparable<ComponentType>
	{
		public bool isEntity => hashCode == 0;

		/// <summary>
		/// Stable hash code based on the name of the type.
		/// </summary>
		public readonly uint hashCode;

		/// <summary>
		/// The size of the type (corresponds to the <c>sizeof</c> opcode).
		/// </summary>
		public readonly int size;

		internal ComponentType(uint hashCode, int size)
		{
			this.hashCode = hashCode;
			this.size = size;		}

		internal ComponentType(TypeInfo info)
		{
			this.hashCode = info.hashCode;
			this.size = info.size;
		}

		public static bool operator ==(ComponentType lhs, ComponentType rhs) =>
			lhs.hashCode == rhs.hashCode;

		public static bool operator !=(ComponentType lhs, ComponentType rhs) =>
			lhs.hashCode != rhs.hashCode;
		
		public override bool Equals(object other) => this == (ComponentType)other;

		public bool Equals(ComponentType other) => this == other;

		public int CompareTo(ComponentType other) => this.hashCode.CompareTo(other.hashCode);

		public override int GetHashCode() => (int)this.hashCode;

		/// <summary>
		/// Looks up the component type based on the specified type.
		/// The component type information is cached.
		/// </summary>
		/// <typeparam name="T">The type.</typeparam>
		/// <returns>The component type.</returns>
		public static ComponentType Of<T>() where T : unmanaged
		{
			var info = TypeRegistry<T>.info;
			return new ComponentType(info);
		}

		/// <summary>
		/// Looks up the component type based on the type instance.
		/// The component type information is cached.
		/// </summary>
		/// <remarks>
		/// When this function is given a previously unknown type instance,
		/// this function uses reflection to get the component type information
		/// (which may trigger the garbage collector).
		/// </remarks>
		/// <param name="type">The type instance.</param>
		/// <returns>The component type.</returns>
		public static ComponentType Of(Type type)
		{
			var info = TypeRegistry.GetTypeInfo(type);
			return new ComponentType(info);
		}

		/*public static ComponentType AsBuffer<T>() where T : unmanaged
		{
			var info = TypeRegistry<ComponentBuffer<T>>.info;
			return new ComponentType(info);
		}*/
	}
}