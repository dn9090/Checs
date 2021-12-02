using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	// TODOs:
	// [ ] Rework the query matching algorithm.
	// [ ] Query and archetype component types need to be distinct.
	// [ ] Better free heuristics for ChunkPool.
	// [ ] Allocate / recycle multiple chunks for batch processing like CreateEntities(1000000).
	// [ ] (API like GetMaxEntitiesPerBatchCount(EntityArchetype archetype))
	// [ ] Cleanup EntityManager, HashMap and Archetype initialization.
	// [ ] Keep track of structural changes.
	// [ ] ChunkPool needs to be thread-safe.
	// [ ] TypeRegistry needs to be thread-safe.
	// [ ] Generate public API for World?

	public unsafe partial class EntityManager : IDisposable
	{
		internal ArchetypeStore* archetypeStore;

		internal EntityStore* entityStore;

		internal EntityQueryCache* queryCache;

		internal EntityManager()
		{
			Construct();
			
			// Create the empty archetype to avoid that the index
			// of the default EntityArchetype is out of range.
			CreateArchetype();

			// Create the empty query to avoid that the index
			// of the default EntityQuery is out of range.
			CreateQuery();
		}

		private void Construct()
		{
			// Revisit with threading (cache invalidation).
			byte* ptr = MemoryUtility.Malloc<byte>(
				sizeof(ArchetypeStore) +
				sizeof(EntityStore) +
				sizeof(EntityQueryCache));

			this.archetypeStore = (ArchetypeStore*)ptr;
			this.entityStore = (EntityStore*)(ptr += sizeof(ArchetypeStore));
			this.queryCache = (EntityQueryCache*)(ptr += sizeof(EntityStore));
		
			ArchetypeStore.Construct(this.archetypeStore);
			EntityStore.Construct(this.entityStore);
			EntityQueryCache.Construct(this.queryCache);
		}

		public void Dispose()
		{
			this.archetypeStore->Dispose();
			this.entityStore->Dispose();
			this.queryCache->Dispose();
		
			MemoryUtility.Free(this.archetypeStore);
		}
	}
}