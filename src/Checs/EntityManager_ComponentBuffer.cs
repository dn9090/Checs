using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Checs
{
	public unsafe partial class EntityManager
	{
		public ComponentBuffer<T> GetComponentBuffer<T>(Entity entity) where T : unmanaged
		{
			TryGetComponentBuffer<T>(entity, out var value);
			return value;
		}

		public bool TryGetComponentBuffer<T>(Entity entity, out ComponentBuffer<T> buffer) where T : unmanaged
		{
			if(TryGetEntityInChunk(entity, out var entityInChunk))
			{
				var hashCode = TypeRegistry<ComponentBuffer<T>>.info.hashCode;
				var componentIndex = ArchetypeUtility.GetComponentIndex(entityInChunk.chunk->archetype, hashCode);

				if(componentIndex >= 0)
				{
					var offset = ChunkUtility.GetComponentDataOffset(entityInChunk.chunk, componentIndex, entityInChunk.index);
					buffer = new ComponentBuffer<T>(entityInChunk.chunk, offset);
					return true;
				}
			}

			buffer = default;
			return false;
		}
	}
}