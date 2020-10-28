using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	public unsafe partial class EntityManager : IDisposable
	{
		internal ArchetypeStore archetypeStore;

		internal EntityStore entityStore;

		internal EntityQueryCache queryCache;

		internal EntityManager()
		{
			this.archetypeStore = ArchetypeStore.Empty();
			this.entityStore = EntityStore.Empty();
			this.queryCache = EntityQueryCache.Empty();

			// Create the empty archetype to avoid that the index
			// of the default EntityArchetype is out of range.  
			CreateArchetype();
		}

		private void Construct()
		{
			void* ptr = MemoryUtility.Malloc(
				sizeof(ArchetypeStore) +
				sizeof(EntityStore) +
				sizeof(EntityQueryCache));

			ArchetypeStore.Construct(ptr);
			// TODO: ...
		}

		public void Dispose()
		{
			this.archetypeStore.Dispose();
			// TODO: Dispose ...
		}
	}
}