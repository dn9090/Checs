using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	[StructLayout(LayoutKind.Explicit)]
	public struct EntityIterator : IDisposable
	{
		[FieldOffset(0)]
		internal unsafe Query* query;

		[FieldOffset(0)]
		internal unsafe Archetype* archetype;

		[FieldOffset(8)]
		internal unsafe ChunkList* chunkList;

		[FieldOffset(16)]
		internal int archetypeIndex;

		[FieldOffset(20)]
		internal int chunkIndex;

		[FieldOffset(24)]
		internal int chunkCount;

		[FieldOffset(28)]
		internal uint structuralVersion;

		internal unsafe EntityIterator(Query* query)
		{
			this.archetype = null;
			this.query = query;
			this.chunkList = null;
			this.archetypeIndex = 0;
			this.chunkIndex = 0;
			this.chunkCount = 0;
			this.structuralVersion = query->changeVersion->structuralVersion;
		}

		internal unsafe EntityIterator(Archetype* archetype)
		{
			this.query = null;
			this.archetype = archetype;
			this.chunkList = &archetype->chunkList;
			this.archetypeIndex = -1;
			this.chunkIndex = 0;
			this.chunkCount = archetype->chunkList.count;
			this.structuralVersion = archetype->changeVersion->structuralVersion;
		}

		public bool TryNext(out EntityTable table)
		{
			unsafe
			{
		Retry:
				if((uint)this.chunkIndex < (uint)this.chunkCount)
				{
					var chunk = chunkList->chunks[this.chunkIndex++];
					table = new EntityTable(chunk);
					return true;
				}

				if(this.archetypeIndex >= 0)
				{
					var archetypeList = &query->archetypeList;

					if((uint)this.archetypeIndex < (uint)archetypeList->count)
					{
						chunkList = &archetypeList->archetypes[this.archetypeIndex++]->chunkList;
						this.chunkIndex = 0;
						this.chunkCount = chunkList->count;
						goto Retry;
					}
				}
			}

			table = default;

			return false;	
		}
		
		public void Dispose()
		{
			if(this.structuralVersion != 0)
			{
				unsafe
				{
					if(this.archetypeIndex >= 0)
						query->changeVersion->CheckStructuralChange(this.structuralVersion); // TODO
					else
						archetype->changeVersion->CheckStructuralChange(this.structuralVersion);
				}
			}
			
			this.chunkIndex = -1;
			this.archetypeIndex = -1;
			this.structuralVersion = 0;
		}
	}
}