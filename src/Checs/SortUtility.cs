using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading;

namespace Checs
{
	internal unsafe static class SortUtility
	{
		public static int InsertSorted(Span<int> values, int item)
		{
			var index = values.Length;

			while(index > 0 && item < values[index - 1])
				values[index] = values[--index];

			values[index] = item;

			return index;
		}

		public static void Sort(Span<int> values)
		{
			for(int i = 1; i < values.Length; ++i)
			{
				var type = values[i];
				var index = i;

				for(; index > 0 && type.CompareTo(values[index - 1]) <= 0; --index)
				{
					values[index] = values[index - 1];
				}
					
				values[index] = type;
			}
		}

		public static void Sort(Span<int> values, Span<int> other)
		{
			for(int i = 1; i < values.Length; ++i)
			{
				var type = values[i];
				var size = other[i];
				var index = i;

				for(; index > 0 && type.CompareTo(values[index - 1]) <= 0; --index)
				{
					values[index] = values[index - 1];
					other[index] = other[index - 1];
				}
					
				values[index] = type;
				other[index] = size;
			}
		}
	}
}