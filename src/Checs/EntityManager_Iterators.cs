using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	public unsafe partial class EntityManager
	{
		public delegate void Action<T>(EntityTable table, T context);

		/// <summary>
		/// Performs the action on each <see cref="EntityTable"/> of the
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

		public void ForEach<T>(EntityArchetype archetype, Action<T> action, T context)
		{
			var arch = GetArchetypeInternal(archetype);
			ForEachInternal(arch, action, context);
		}

		/// <summary>
		/// Performs the action on each <see cref="EntityTable"/> of the
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

		public void ForEach<T>(EntityQuery query, Action<T> action, T context)
		{
			var qry = GetQueryInternal(query);
			UpdateQueryCache(qry);

			var count      = qry->archetypeList.count;
			var archetypes = qry->archetypeList.archetypes;
			
			for(int i = 0; i < count; ++i)
				ForEachInternal(archetypes[i], action, context);
		}

		internal void ForEachInternal<T>(Archetype* archetype, Action<T> action, T context)
		{
			var chunkVersion = archetype->chunkVersion;

			var count  = archetype->chunkList.count;
			var chunks = archetype->chunkList.chunks;

			for(int i = 0; i < count; ++i)
				action(new EntityTable(chunks[i]), context);

			CheckModified(archetype, chunkVersion);
		}

		public void ForEach<T>(ReadOnlySpan<Entity> entities, Action<T> action, T context)
		{
			var count = 0;

			while(count < entities.Length)
			{
				var batch = GetFirstEntityBatch(entities.Slice(count));

				count += batch.count;

				if(batch.chunk == null)
					continue;

				action(new EntityTable(batch.chunk, batch.index, batch.count), context);
			}
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void CheckModified(Archetype* archetype, uint version)
		{
			if(version != archetype->chunkVersion)
				ThrowHelper.ThrowWriteOnReadExeception(archetype);
		}
	}
}