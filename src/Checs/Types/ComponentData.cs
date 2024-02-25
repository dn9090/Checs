using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	public struct ComponentData<T> where T : unmanaged
	{
		public T this[Entity entity]
		{
			get => TryGetValue(entity, out var value) ? value : default;
			set => SetValue(entity, in value);
		}

		internal EntityManager manager;

		internal uint hashCode;

		internal int cachedIndex;

		public ComponentData(EntityManager manager)
		{
			this.manager     = manager;
			this.hashCode    = TypeRegistry<T>.info.hashCode;
			this.cachedIndex = 1;
		}

		public bool Contains(Entity entity)
		{
			if(manager.TryGetEntityInChunk(entity, out var entityInChunk))
			{
				unsafe
				{
					var componentIndex = ArchetypeUtility.GetComponentIndex(entityInChunk.chunk->archetype, this.hashCode, this.cachedIndex);

					if(componentIndex >= 0)
					{
						this.cachedIndex = componentIndex;
						return true;
					}
				}
			}

			return false;
		}

		public bool SetValue(Entity entity, in T value)
		{
			if(manager.TryGetEntityInChunk(entity, out var entityInChunk))
			{
				unsafe
				{
					var componentIndex = ArchetypeUtility.GetComponentIndex(entityInChunk.chunk->archetype, this.hashCode, this.cachedIndex);

					if(componentIndex > 0)
					{
						this.cachedIndex = componentIndex;
						ChunkUtility.GetComponentDataPtr<T>(entityInChunk.chunk, componentIndex)[entityInChunk.index] = value;
						ChunkUtility.MarkAsChanged(entityInChunk.chunk, componentIndex);
						return true;
					}
				}
			}

			return false;
		}

		public bool TryGetValue(Entity entity, out T value)
		{
			if(manager.TryGetEntityInChunk(entity, out var entityInChunk))
			{
				unsafe
				{
					var componentIndex = ArchetypeUtility.GetComponentIndex(entityInChunk.chunk->archetype, this.hashCode, this.cachedIndex);

					if(componentIndex >= 0)
					{
						this.cachedIndex = componentIndex;
						value = ChunkUtility.GetComponentDataPtr<T>(entityInChunk.chunk, componentIndex)[entityInChunk.index];
						return true;
					}
				}
			}

			value = default;
			return false;
		}
	}
}
