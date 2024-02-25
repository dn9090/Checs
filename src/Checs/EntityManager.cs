using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Checs
{
	/// <summary>
	/// The central data structure that manages all entities, components,
	/// archetypes and queries.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Size = 128)]
	public unsafe partial class EntityManager : IDisposable
	{
		internal EntityStore entityStore;

		internal ArchetypeStore archetypeStore;

		internal QueryStore queryStore;

		internal ChunkStore chunkStore;

		internal HashMap<int> lookupTable;

		internal ChangeVersion changeVersion;

		/// <summary>
		/// Returns the number of entities.
		/// </summary>
		public int entityCount => this.entityStore.count;

		/// <summary>
		/// Returns the number of archetypes.
		/// </summary>
		public int archetypeCount => this.archetypeStore.count;

		/// <summary>
		/// Returns the number of queries.
		/// </summary>
		public int queryCount => this.queryStore.count;

		public EntityManager()
		{
			this.entityStore    = new EntityStore(16);
			this.archetypeStore = new ArchetypeStore(16);
			this.queryStore     = new QueryStore(16);
			this.chunkStore     = new ChunkStore();
			this.lookupTable    = new HashMap<int>(16);
			this.changeVersion  = new ChangeVersion(0);

			CreateEmptyArchetype();
			CreateUniversialQuery();
		}

		~EntityManager()
		{
			Dispose();
		}

		public void Dispose()
		{
			this.entityStore.Dispose();
			this.archetypeStore.Dispose();
			this.queryStore.Dispose();
			this.chunkStore.Dispose();
			this.lookupTable.Dispose();
			this.changeVersion.Dispose();

			GC.SuppressFinalize(this);
		}
	}
}