using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe partial class EntityManager : IDisposable
	{
		internal EntityStore entityStore; // 32 - 32

		internal ArchetypeStore archetypeStore; // 16 - 48

		internal QueryStore queryStore; // 16- 64

		internal ChunkStore chunkStore; // 16 - 80

		internal HashMap<int> lookupTable; // 24 - 104

		internal int* testCounter;

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
			this.entityStore = new EntityStore(16);
			this.archetypeStore = new ArchetypeStore(16);
			this.queryStore = new QueryStore(16);
			this.chunkStore = new ChunkStore();
			this.lookupTable = new HashMap<int>(16);

			this.testCounter = (int*)Allocator.Alloc(sizeof(int));

			CreateEmptyArchetype();
			CreateUniversialQuery();
		}

		/*~EntityManager()
		{
			Dispose();
		}*/

		public void Dispose()
		{
			this.entityStore.Dispose();
			this.archetypeStore.Dispose();
			this.queryStore.Dispose();
			this.chunkStore.Dispose();
			this.lookupTable.Dispose();
		}
	}
}