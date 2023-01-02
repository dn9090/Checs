using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	public unsafe partial class EntityManager
	{
		public EntityChangeVersion GetChangeVersion()
		{
			return new EntityChangeVersion(this.changeVersion.Read());
		}

		public bool DidChange(EntityChangeVersion changeVersion)
		{
			return ChangeVersion.DidChange(this.changeVersion.value, changeVersion.version);
		}

		public bool DidChange(EntityChangeVersion changeVersion, Entity entity)
		{
			if(TryGetEntityInChunk(entity, out var entityInChunk))
				return ChangeVersion.DidChange(entityInChunk.chunk->changeVersion, changeVersion.version);

			return false;
		}

		public bool DidChange(EntityChangeVersion changeVersion, Entity entity, ComponentType type)
		{
			if(TryGetEntityInChunk(entity, out var entityInChunk))
			{
				var chunkChanged     = ChangeVersion.DidChange(entityInChunk.chunk->changeVersion, changeVersion.version);
				var componentChanged = ArchetypeUtility.DidChange(entityInChunk.chunk->archetype, type.hashCode, changeVersion.version);
				return chunkChanged && componentChanged;
			}

			return false;
		}

		public bool DidChange(EntityChangeVersion changeVersion, EntityArchetype archetype)
		{
			var arch = GetArchetypeInternal(archetype);
			return ArchetypeUtility.DidChange(arch, changeVersion.version);
		}

		public bool DidChange(EntityChangeVersion changeVersion, EntityArchetype archetype, ComponentType type)
		{
			var arch = GetArchetypeInternal(archetype);
			return ArchetypeUtility.DidChange(arch, type.hashCode, changeVersion.version);
		}

		public bool DidChange(EntityChangeVersion changeVersion, EntityQuery query)
		{
			var qry = GetQueryInternal(query);
			UpdateQueryCache(qry);

			var count = qry->archetypeList.count;
			var archetypes = qry->archetypeList.archetypes;

			for(int i = 0; i < count; ++i)
			{
				if(ArchetypeUtility.DidChange(archetypes[i], changeVersion.version))
					return true;
			}

			return false;
		}

		public bool DidChange(EntityChangeVersion changeVersion, EntityQuery query, ComponentType type)
		{
			var qry = GetQueryInternal(query);
			UpdateQueryCache(qry);

			var count = qry->archetypeList.count;
			var archetypes = qry->archetypeList.archetypes;

			for(int i = 0; i < count; ++i)
			{
				if(ArchetypeUtility.DidChange(archetypes[i], type.hashCode, changeVersion.version))
					return true;
			}

			return false;
		}
	}
}