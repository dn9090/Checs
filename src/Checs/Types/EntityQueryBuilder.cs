using System;

namespace Checs
{
	public struct EntityQueryBuilder
	{
		internal const int BufferCapacity = 4;

		internal int includeCount;

		internal int excludeCount;

		internal unsafe fixed int includeTypes[BufferCapacity];

		internal unsafe fixed int excludeTypes[BufferCapacity];

		internal int[] includeTypesResizable;

		internal int[] excludeTypesResizable;
		
		public EntityQueryBuilder Include<T>() where T : unmanaged
		{
			IncludeInternal(TypeRegistry<T>.typeIndex);
			return this;
		}

		public EntityQueryBuilder Include(Type type)
		{
			IncludeInternal(TypeRegistry.ToTypeIndex(type));
			return this;
		}

		internal unsafe void IncludeInternal(int typeIndex)
		{
			if(includeCount >= BufferCapacity)
			{
				var index = includeCount - BufferCapacity;

				if(includeTypesResizable == null)
					includeTypesResizable = new int[BufferCapacity];

				if(index >= includeTypesResizable.Length)
					Array.Resize(ref includeTypesResizable, includeTypesResizable.Length * 2);

				includeTypesResizable[index] = typeIndex;
			} else {
				includeTypes[includeCount] = typeIndex;
			}

			++includeCount;
		}

		public EntityQueryBuilder Exclude<T>() where T : unmanaged
		{
			ExcludeInternal(TypeRegistry<T>.typeIndex);
			return this;
		}

		public EntityQueryBuilder Exclude(Type type)
		{
			ExcludeInternal(TypeRegistry.ToTypeIndex(type));
			return this;
		}

		internal unsafe void ExcludeInternal(int typeIndex)
		{
			if(excludeCount >= BufferCapacity)
			{
				var index = excludeCount - BufferCapacity;

				if(excludeTypesResizable == null)
					excludeTypesResizable = new int[BufferCapacity];

				if(index >= excludeTypesResizable.Length)
					Array.Resize(ref excludeTypesResizable, excludeTypesResizable.Length * 2);

				excludeTypesResizable[index] = typeIndex;
			} else {
				includeTypes[excludeCount] = typeIndex;
			}

			++excludeCount;
		}
	}
}
