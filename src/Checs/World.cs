using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	public class World : IDisposable
	{
		public EntityManager entityManager;

		public World()
		{
			this.entityManager = new EntityManager();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EntityArchetype CreateArchetype() => this.entityManager.CreateArchetype();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EntityArchetype CreateArchetype(Type type) => this.entityManager.CreateArchetype(type);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EntityArchetype CreateArchetype(params Type[] types) => this.entityManager.CreateArchetype(types);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EntityArchetype CreateArchetype(Span<Type> types) => this.entityManager.CreateArchetype(types);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EntityArchetype GetArchetype(Entity entity) => this.entityManager.GetArchetype(entity);
	
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Entity CreateEntity() => this.entityManager.CreateEntity(1)[0]; // @Todo: Seperate method...

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Entity CreateEntity(EntityArchetype archetype) => this.entityManager.CreateEntity(archetype, 1)[0];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<Entity> CreateEntity(int count) => this.entityManager.CreateEntity(count);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<Entity> CreateEntity(EntityArchetype archetype, int count) => this.entityManager.CreateEntity(archetype, count);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DestroyEntity(Entity entity) => this.entityManager.DestroyEntity(entity);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DestroyEntity(ReadOnlySpan<Entity> entities) => this.entityManager.DestroyEntity(entities);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<Entity> GetEntities(EntityArchetype archetype) => this.entityManager.GetEntities(archetype);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<Entity> GetEntities(EntityQuery query) => this.entityManager.GetEntities(query);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsAlive(Entity entity) => this.entityManager.IsAlive(entity);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasComponentData<T>(Entity entity) where T : unmanaged, IComponentData => this.entityManager.HasComponentData<T>(entity);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T GetComponentData<T>(Entity entity) where T : unmanaged, IComponentData => this.entityManager.GetComponentData<T>(entity);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetComponentData<T>(Entity entity, out T value) where T : unmanaged, IComponentData => this.entityManager.TryGetComponentData<T>(entity, out value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T RefComponentData<T>(Entity entity) where T : unmanaged, IComponentData => ref this.entityManager.RefComponentData<T>(entity);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool SetComponentData<T>(Entity entity, T value) where T : unmanaged, IComponentData => this.entityManager.SetComponentData<T>(entity, value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool AddComponentData<T>(Entity entity) where T : unmanaged, IComponentData => this.entityManager.AddComponentData<T>(entity);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void DestroyComponentData<T>(Entity entity) where T : unmanaged, IComponentData => this.entityManager.DestroyComponentData<T>(entity);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EntityQuery CreateQuery() => this.entityManager.CreateQuery();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EntityQuery CreateQuery(params Type[] includeTypes) => this.entityManager.CreateQuery(includeTypes);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public EntityQuery CreateQuery(Span<Type> includeTypes) => this.entityManager.CreateQuery(includeTypes);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ForEach(EntityArchetype archetype, Action<EntityBatch> action) => this.entityManager.ForEach(archetype, action);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ForEach(EntityQuery query, Action<EntityBatch> action) => this.entityManager.ForEach(query, action);

		public void Dispose()
		{
			this.entityManager.Dispose();
		}
	}
}