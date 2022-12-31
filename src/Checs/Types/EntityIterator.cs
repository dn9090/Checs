using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	[StructLayout(LayoutKind.Sequential)]
	public struct EntityIterator : IDisposable
	{
		internal unsafe ArchetypeList* archetypeList;

		internal unsafe Archetype* archetype;

		internal int archetypeIndex;

		internal int chunkIndex;

		internal int chunkCount;

		internal uint chunkVersion;

		internal unsafe EntityIterator(Query* query)
		{
			this.archetypeList  = &query->archetypeList;
			this.archetype      = null;
			this.archetypeIndex = 0;
			this.chunkIndex     = 0;
			this.chunkCount     = 0;
			this.chunkVersion   = 0;
		}

		internal unsafe EntityIterator(Archetype* archetype)
		{
			this.archetypeList  = null;
			this.archetype      = archetype;
			this.archetypeIndex = 0;
			this.chunkIndex     = 0;
			this.chunkCount     = archetype->chunkList.count;
			this.chunkVersion   = archetype->chunkVersion;
		}

		public bool TryNext(out EntityTable table)
		{
			unsafe
			{
		Retry:
				if((uint)this.chunkIndex < (uint)this.chunkCount)
				{
					var chunk = archetype->chunkList.chunks[this.chunkIndex++];
					table = new EntityTable(chunk);
					return true;
				}

				if(this.archetypeList != null && (uint)this.archetypeIndex < (uint)this.archetypeList->count)
				{
					this.archetype  = archetypeList->archetypes[this.archetypeIndex++];
					this.chunkIndex = 0;
					this.chunkCount = this.archetype->chunkList.count;
					goto Retry;
				}
			}

			table = default;

			return false;	
		}
		
		public void Dispose()
		{
			this.chunkIndex = -1;
			this.archetypeIndex = -1;
		}
	}
}