using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	public unsafe partial class EntityManager
	{
		public bool HasChanged(EntityChangeVersion changeVersion)
		{
			return this.entityStore.changeVersion->HasChanged(changeVersion.version);
		}

		public bool HasChanged(Entity entity, EntityChangeVersion changeVersion)
		{
			if(TryGetEntityInChunk(entity, out var entityInChunk))
				return ChunkUtility.HasChanged(entityInChunk.chunk, changeVersion.version);

			return false;
		}

		public bool HasChanged<T>(Entity entity, EntityChangeVersion changeVersion) where T : unmanaged
		{
			if(TryGetEntityInChunk(entity, out var entityInChunk))
			{
				if(ChunkUtility.HasChanged(entityInChunk.chunk, changeVersion.version))
				{
					var hashCode = TypeRegistry<T>.info.hashCode;
					return ArchetypeUtility.HasChanged(entityInChunk.chunk->archetype, hashCode, changeVersion.version);
				}
			}

			return false;
		}

		public bool HasChanged(EntityArchetype archetype, EntityChangeVersion changeVersion)
		{
			var arch = GetArchetypeInternal(archetype);
			
			return ArchetypeUtility.HasChanged(arch, changeVersion.version);
		}

		public bool HasChanged<T>(EntityArchetype archetype, EntityChangeVersion changeVersion) where T : unmanaged
		{
			var arch = GetArchetypeInternal(archetype);
			var hashCode = TypeRegistry<T>.info.hashCode;

			return ArchetypeUtility.HasChanged(arch, hashCode, changeVersion.version);
		}

		public bool HasChanged(EntityQuery query, EntityChangeVersion changeVersion)
		{
			var qry = GetQueryInternal(query);
			UpdateQueryCache(qry);

			for(int i = 0; i < qry->archetypeList.count; ++i)
			{
				if(ArchetypeUtility.HasChanged(qry->archetypeList.archetypes[i], changeVersion.version))
					return true;
			}

			return false;
		}

		public EntityChangeVersion GetChangeVersion()
		{
			var version = this.entityStore.changeVersion->GetCurrentChangeVersion();
			return new EntityChangeVersion(version);
		}

		public bool UpdateChangeVersion(ref EntityChangeVersion changeVersion)
		{
			var version = this.entityStore.changeVersion->GetCurrentChangeVersion();
			var hasChanged = this.entityStore.changeVersion->HasChanged(changeVersion.version);

			changeVersion = new EntityChangeVersion(version);

			return hasChanged;
		}
	}
}