using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct TypeInfo // TODO: Can be read-only.
	{
		public uint hashCode;

		public int size;

		public Type type;
	}

	internal static class TypeRegistry
	{
		public static Dictionary<uint, TypeInfo> infos = new Dictionary<uint, TypeInfo>(16);

		public static object _lock_ = new object();

		public static TypeInfo GetTypeInfo<T>()
		{
			// This method should only be called from a thread-safe context
			// like a static constructor.

			var type = typeof(T);
			var hashCode = TypeUtility.GetHashCode(type);

			if(infos.TryGetValue(hashCode, out TypeInfo info))
				return info;
			
			info = new TypeInfo
			{
				hashCode = hashCode,
				size = Unsafe.SizeOf<T>(),
				type = type
			};

			infos.TryAdd(hashCode, info);

			return info;
		}

		public static TypeInfo GetTypeInfo(Type type) 
		{
			var hashCode = TypeUtility.GetHashCode(type);
			var info     = new TypeInfo();

			lock(_lock_)
			{
				if(infos.TryGetValue(hashCode, out info))
					return info;
			}
			
			info = new TypeInfo
			{
				hashCode = hashCode,
				size = TypeUtility.SizeOf(type),
				type = type
			};

			lock(_lock_)
			{
				// At this point, another thread may have already added
				// the type information.
				infos.TryAdd(hashCode, info);
			}

			return info;
		}
		
		public static TypeInfo GetTypeInfo(uint hashCode)
		{
			if(infos.TryGetValue(hashCode, out TypeInfo info))
				return info;

			return default;
		}

		public static unsafe int GetTypes(uint* hashCodes, int count, Span<Type> types)
		{
			count = types.Length < count ? types.Length : count;

			for(int i = 0; i < count; ++i)
				types[i] = GetTypeInfo(*hashCodes++).type;

			return count;
		}
	}

	internal static class TypeRegistry<T>
	{
		public static TypeInfo info = TypeRegistry.GetTypeInfo<T>();
	}
}
