using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	public unsafe partial class EntityManager
	{
		public delegate void Action<T>(EntityTable table, T args);

		/// <summary>
		/// Performs the specified action on each <see cref="EntityTable"/> of the
		/// specified archetype.
		/// </summary>
		/// <param name="archetype">The archetype.</param>
		/// <param name="action">
		/// The <see cref="EntityManager.Action{T}" delegate
		/// to perform.
		/// </param>
		public void ForEach(EntityArchetype archetype, Action<EntityManager> action)
		{
			ForEach(archetype, action, this);
		}

		public void ForEach<T>(EntityArchetype archetype, Action<T> action, T args)
		{
			var arch = GetArchetypeInternal(archetype);
			ForEachInternal(arch, action, args);
		}

		/// <summary>
		/// Performs the specified action on each <see cref="EntityTable"/> of the
		/// specified query.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <param name="action">
		/// The <see cref="EntityManager.Action{T}" delegate
		/// to perform.
		/// </param>
		public void ForEach(EntityQuery query, Action<EntityManager> action)
		{
			ForEach(query, action, this);
		}

		public void ForEach<T>(EntityQuery query, Action<T> action, T args)
		{
			var qry = GetQueryInternal(query);
			UpdateQueryCache(qry);
			
			for(int i = 0; i < qry->archetypeList.count; ++i)
				ForEachInternal(qry->archetypeList.archetypes[i], action, args);
		}

		internal void ForEachInternal<T>(Archetype* archetype, Action<T> action, T args)
		{
			var structuralVersion = this.entityStore.changeVersion->structuralVersion;

			var count = archetype->chunkList.count;
			var chunks = archetype->chunkList.chunks;

			for(int i = 0; i < count; ++i)
				action(new EntityTable(chunks[i]), args);

			this.entityStore.changeVersion->CheckStructuralChange(structuralVersion);
		}
		
		public EntityIterator GetIterator(EntityArchetype archetype)
		{
			var arch = GetArchetypeInternal(archetype);
			return new EntityIterator(arch);
		}

		public EntityIterator GetIterator(EntityQuery query)
		{
			var qry = GetQueryInternal(query);
			UpdateQueryCache(qry);
			return new EntityIterator(qry);
		}
	}
}