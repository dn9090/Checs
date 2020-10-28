using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	internal static class ManagedStore
	{
		public static int count => s_Objects.Count;

		private struct ObjectReference
		{
			public int refCount;

			public object value;

			public ObjectReference(object obj)
			{
				this.refCount = 0;
				this.value = obj;
			}
		}

		private static Dictionary<int, ObjectReference> s_Objects;

		static ManagedStore()
		{
			s_Objects = new Dictionary<int, ObjectReference>();
		}

		public static int Store<T>(T value)
		{
			object obj = value;
			int hashCode = RuntimeHelpers.GetHashCode(obj);

			if(!s_Objects.TryGetValue(hashCode, out ObjectReference objReference))
				objReference = new ObjectReference(obj);

			++objReference.refCount;
			s_Objects[hashCode] = objReference;

			return hashCode;
		}

		public static T Get<T>(int hashCode)
		{
			if(s_Objects.TryGetValue(hashCode, out ObjectReference objReference))
				return (T)objReference.value;

			return default;
		}

		public static void Destroy(int hashCode)
		{
			if(s_Objects.TryGetValue(hashCode, out ObjectReference objReference))
			{
				--objReference.refCount;

				if(objReference.refCount == 0)
					s_Objects.Remove(hashCode);
				else
					s_Objects[hashCode] = objReference;
			}
		}
	}

}