using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Checs
{
	public readonly struct ComponentType : IEquatable<ComponentType>
	{
		public bool isEntity => hashCode == 0;

		public readonly uint hashCode;

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

		public static ComponentType Of<T>() where T : unmanaged
		{
			var info = TypeRegistry<T>.info;
			return new ComponentType(info);
		}
	
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