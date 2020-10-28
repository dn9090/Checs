using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
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
		}

		private void Construct()
		{
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