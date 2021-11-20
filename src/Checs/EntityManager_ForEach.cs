using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	public unsafe partial class EntityManager
	{
		public void ForEach(EntityArchetype archetype, Action<EntityBatch> action)
		{
			Archetype* ptr = GetArchetypeInternal(archetype);
			ForEachInternal(ptr, action);
		}

		public void ForEach(EntityQuery query, Action<EntityBatch> action)
		{
			var queryData = GetUpdatedQueryData(query);
			
			for(int i = 0; i < queryData->archetypeCount; ++i)
				ForEachInternal(queryData->archetypes[i], action);
		}

		internal void ForEachInternal(Archetype* archetype, Action<EntityBatch> action)
		{
			var count = archetype->chunkArray->count;
			var chunks = archetype->chunkArray->chunks;

			for(int i = 0; i < count; ++i)
				action(new EntityBatch(chunks[i]));
		}

		// void ForEach<T>(EntityQuery query, T job) where T : struct, IJob ???

		// void ForEachParallel(EntityQuery query, T job)
	}
}