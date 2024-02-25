using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Checs
{
	/*
		https://github.com/dotnet/runtime/issues/78871
	*/

	internal unsafe static class TypeUtility
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint GetHashCode(Type type)
		{
			// The type of the entity should always have the hash value of 0
			// to guarantee that the entity type information is always at the first index.
			return xxHash.GetHashCode(type.FullName) ^ Entity.hashCode;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SizeOf(Type type)
		{
			// Workaround because Unsafe.SizeOf does not accept a type object and
			// Marshal.SizeOf gives back the wrong size (and does not work with generic types).
			var method = typeof(Unsafe).GetMethod(nameof(Unsafe.SizeOf)).MakeGenericMethod(type);
			return (int)method.Invoke(null, null);
		}

		public static bool IsZeroSized(Type type)
		{
			// Based on: https://stackoverflow.com/a/27851610
			
			if(!type.IsValueType || type.IsPrimitive)
				return false;
			
			var fields = type.GetFields((BindingFlags)0x34);

			for(int i = 0; i < fields.Length; ++i)
			{
				if(!IsZeroSized(fields[i].FieldType))
					return false;
			}

			return true;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal class RawObject
		{
			public byte data;
		}

		public static Span<byte> Unbox(object value, int size)
		{
			var raw = Unsafe.As<RawObject>(value);
			return MemoryMarshal.CreateSpan(ref raw.data, size);
		}

		public static int Sort(ReadOnlySpan<ComponentType> types, uint* hashCodes, int* sizes, int startIndex)
		{
			var count = startIndex;

			for(int i = 0; i < types.Length; ++i)
			{
				var hashCode = types[i].hashCode;
				var size     = types[i].size;
				var index = 0;

				while(index < count && hashCode > hashCodes[index])
					++index;
				
				if(hashCode == hashCodes[index])
					continue;

				var next = count++;

				while(next > index)
				{
					hashCodes[next] = hashCodes[next - 1];
					sizes[next]     = sizes[next - 1];
					--next;
				}

				hashCodes[index] = hashCode;
				sizes[index]     = size;
			}

			return count;
		}

		public static int Sort(ReadOnlySpan<ComponentType> types, uint* hashCodes)
		{
			var count = 0;

			for(int i = 0; i < types.Length; ++i)
			{
				var hashCode = types[i].hashCode;
				var index = 0;

				while(index < count && hashCode > hashCodes[index])
					++index;
				
				if(hashCode == hashCodes[index])
					continue;

				var next = count++;

				while(next > index)
				{
					hashCodes[next] = hashCodes[next - 1];
					--next;
				}

				hashCodes[index] = hashCode;
			}

			return count;
		}

		public static int Sort(uint* hashCodes, int* sizes, int count)
		{
			var actualCount = 0;

			for(int i = 0; i < count; ++i)
			{
				var hashCode = hashCodes[i];
				var size     = sizes[i];
				var index = 0;

				while(index < actualCount && hashCode > hashCodes[index])
					++index;
				
				if(hashCode == hashCodes[index])
					continue;

				var next = actualCount++;

				while(next > index)
				{
					hashCodes[next] = hashCodes[next - 1];
					sizes[next]     = sizes[next - 1];
					--next;
				}

				hashCodes[index] = hashCode;
				sizes[index]     = size;
			}

			return actualCount;
		}
	}
}
