using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	public class World
	{
		internal EntityManager entityManager;

		public World()
		{
			this.entityManager = new EntityManager();
		}

		public EntityArchetype CreateArchetype() => default;

		public EntityArchetype CreateArchetype(Type type) => default;

		public EntityArchetype CreateArchetype(params Type[] types) => default;

		public EntityArchetype CreateArchetype(Span<Type> types) => default;

		public EntityArchetype GetArchetype(Entity entity) => default;
	
		public Entity CreateEntity() => default;

		public Entity CreateEntity(EntityArchetype archetype) => default;

		public ReadOnlySpan<Entity> CreateEntity(int count) => default;

		public ReadOnlySpan<Entity> CreateEntity(EntityArchetype archetype, int count) => default;

		public void DestroyEntity(Entity entity) {}

		public void DestroyEntity(ReadOnlySpan<Entity> entities) {}

		public bool HasComponentData<T>(Entity entity) where T : unmanaged => default;

		public T GetComponentData<T>(Entity entity) where T : unmanaged => default;

		public bool TryGetComponentData<T>(Entity entity, out T value) where T : unmanaged { value = default; return default; }

		public bool SetComponentData<T>(Entity entity, T value) where T : unmanaged => default;

		// ....
	}
}