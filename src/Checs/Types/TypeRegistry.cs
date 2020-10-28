using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Checs
{
	internal static class TypeRegistry
	{
		public static readonly int emptyTypeIndex = 0;

		public static int typeCount;

		private static Dictionary<Type, int> s_TypeToIndex;

		private static Type[] s_IndexToType;

		static TypeRegistry()
		{
			typeCount = 1;
			s_TypeToIndex = new Dictionary<Type, int>();
			s_IndexToType = new Type[8];
		}
	
		public static int ToTypeIndex(Type type)
		{
			if(s_TypeToIndex.TryGetValue(type, out int typeIndex))
				return typeIndex;

			int count = typeCount;
			typeCount += 1;

			s_TypeToIndex.Add(type, count);

			if(count == s_IndexToType.Length)
				Array.Resize(ref s_IndexToType, s_IndexToType.Length * 2);

			s_IndexToType[count]= type;

			return count;
		}

		public static Type ToType(int typeIndex) => s_IndexToType[typeIndex];
	}

	internal static class TypeRegistry<T>
	{
		public static readonly int typeIndex = TypeRegistry.ToTypeIndex(typeof(T));
	}
}
