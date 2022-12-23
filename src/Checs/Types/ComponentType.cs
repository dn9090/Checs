using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Checs
{
	[Flags]
	internal enum ComponentFlags : int
	{
		None    = 0,
		Buffer  = 1 << 0
	}

	public struct ComponentType : IEquatable<ComponentType>
	{
		public bool isEntity => hashCode == 0;

		internal uint hashCode;

		internal int size;

		internal ComponentFlags flags;

		internal ComponentType(TypeInfo info, ComponentFlags flags)
		{
			this.hashCode = info.hashCode;
			this.size = info.size;
			this.flags = flags;
		}

		internal ComponentType(uint hashCode, int size, ComponentFlags flags = ComponentFlags.None)
		{
			this.hashCode = hashCode;
			this.size = size;
			this.flags = flags;
		}

		public ComponentType AsBuffer()
		{
			var componentType = this;
			componentType.flags |= ComponentFlags.Buffer;
			return componentType;
		}

		public static bool operator ==(ComponentType lhs, ComponentType rhs)
		{
			return lhs.hashCode == rhs.hashCode;
		}
		
		public static bool operator !=(ComponentType lhs, ComponentType rhs) 
		{
			return lhs.hashCode != rhs.hashCode;
		}
		
		public override bool Equals(object other)
		{
			return this == (ComponentType)other;
		}

		public bool Equals(ComponentType other) 
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			return (int)this.hashCode;
		}

		public override string ToString()
		{
			var info = TypeRegistry.GetTypeInfo(this.hashCode);
			return $"ComponentType({info.type.FullName}:{info.hashCode})";
		}

		public static ComponentType Of<T>()
		{
			var info = TypeRegistry<T>.info;
			var flags = ComponentFlags.None;
			return new ComponentType(info, flags);
		}
	}
}