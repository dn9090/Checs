using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	public unsafe readonly struct EntityBatch
	{
		public readonly int length;

		internal readonly Chunk* chunk;

		internal EntityBatch(Chunk* chunk)
		{
			this.chunk = chunk;
			this.length = chunk->count;
		}

		public ReadOnlySpan<Entity> GetEntities() => ChunkUtility.GetEntities(chunk);

		public ReadOnlySpan<T> GetComponentData<T>() where T : unmanaged
		{
			int typeIndex = TypeRegistry<T>.typeIndex;
			return ChunkUtility.GetComponentData<T>(this.chunk, typeIndex);
		}

		public Span<T> GetComponentDataReadWrite<T>() where T : unmanaged
		{
			int typeIndex = TypeRegistry<T>.typeIndex;
			return ChunkUtility.GetComponentData<T>(this.chunk, typeIndex);
		}
		
		public bool HasComponentData<T>() where T : unmanaged
		{
			int typeIndex = TypeRegistry<T>.typeIndex;
			return ArchetypeUtility.GetTypeIndex(this.chunk->archetype, typeIndex) != -1;
		}
	}
}