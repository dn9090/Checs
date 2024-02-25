using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	public unsafe partial class EntityManager
	{
		/// <summary>
		/// Takes a binary snapshot of all entities in the query.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <param name="stream">The destination stream.</param>
		public void TakeSnapshot(EntityQuery query, Stream stream)
		{
			var writer = new BinaryWriter(stream);
			var qry    = GetQueryInternal(query);
			UpdateQueryCache(qry);

			for(int i = 0; i < qry->archetypeList.count; ++i)
			{
				var archetype = qry->archetypeList.archetypes[i];

				if(archetype->entityCount > 0)
					TakeSnapshotInternal(archetype, writer);
			}
		}

		/// <summary>
		/// Takes a binary snapshot of all entities in the archetype.
		/// </summary>
		/// <param name="archetype">The archetype.</param>
		/// <param name="stream">The destination stream.</param>
		public void TakeSnapshot(EntityArchetype archetype, Stream stream)
		{
			var writer = new BinaryWriter(stream);
			var arch   = GetArchetypeInternal(archetype);

			if(arch->entityCount > 0)
				TakeSnapshotInternal(arch, writer);
		}

		internal void TakeSnapshotInternal(Archetype* archetype, BinaryWriter writer)
		{
			// TODO: Ignore entity type name.

			var snapshot = new EntitySnapshot(archetype, this.entityStore.version, this.entityStore.capacity);

			writer.Write(new Span<byte>(&snapshot, sizeof(EntitySnapshot)));

			var componentHashCodes = Archetype.GetComponentHashCodes(archetype);

			for(int j = 0; j < archetype->componentCount; ++j)
			{
				var name = TypeRegistry.GetTypeInfo(componentHashCodes[j]).type.AssemblyQualifiedName;
				writer.Write(name);
			}

			for(int j = 0; j < archetype->chunkList.count; ++j)
			{
				writer.Write(archetype->chunkList.chunks[j]->count);
				writer.Write(ChunkUtility.GetBuffer(archetype->chunkList.chunks[j]));
			}
		}

		/// <summary>
		/// Restores a snapshot.
		/// </summary>
		/// <remarks>
		/// Restoring a snapshot replaces all entities in manager with the same index as in the snapshot.
		/// Requires reflection to restore the type information.
		/// </remarks>
		/// <param name="stream"></param>
		public void RestoreSnapshot(Stream stream)
		{
			var reader   = new BinaryReader(stream);
			var snapshot = new EntitySnapshot();

			var componentTypes = new ComponentType[32];

			while(true)
			{
				var byteCount = 0;

				while(byteCount < sizeof(EntitySnapshot))
				{
					var temp = reader.Read(new Span<byte>(&snapshot, sizeof(EntitySnapshot) - byteCount));
					
					if(temp == 0)
						return;
					
					byteCount += temp;
				}

				if(componentTypes.Length < snapshot.componentCount)
					Array.Resize(ref componentTypes, snapshot.componentCount);
				
				for(int i = 0; i < snapshot.componentCount; ++i)
				{
					var typeName = reader.ReadString();
					var type     = Type.GetType(typeName);

					componentTypes[i] = ComponentType.Of(type);
				}

				Archetype* archetype = null;

				if(lookupTable.TryGet(snapshot.hashCode, out var index))
				{
					archetype = archetypeStore.archetypes[index];
				} else {
					var types = componentTypes.AsSpan(0, snapshot.componentCount);
					archetype = GetArchetypeInternal(CreateArchetype(types));
				}
				
				for(int i = 0; i < snapshot.chunkCount; ++i)
				{
					var count = reader.ReadInt32();
					var chunk = ConstructAndAppendChunk(archetype);

					var buffer = ChunkUtility.GetBuffer(chunk);
					reader.Read(buffer);

					entityStore.Remap(chunk, 0, count, snapshot.entityVersion);
				}
			}
		}

		internal Chunk* GetOrConstructChunk(Archetype* archetype, int requestedCapacity, ref int startIndex)
		{
			if(requestedCapacity < archetype->chunkCapacity)
			{
				var chunkList = &archetype->chunkList;

				for(int i = startIndex; i < chunkList->count; ++i)
				{
					if(chunkList->chunks[i]->count < chunkList->chunks[i]->capacity)
					{
						startIndex = i + 1;
						return chunkList->chunks[i];
					}
				}
			}

			return ConstructAndAppendChunk(archetype);
		}

		internal Chunk* GetOrConstructChunkWithEmptySlots(Archetype* archetype, ref int indexInList)
		{
			var chunkList = &archetype->chunkList;

			for(int i = indexInList; i < chunkList->count; ++i)
			{
				if(chunkList->chunks[i]->count < chunkList->chunks[i]->capacity)
				{
					indexInList = i;
					return chunkList->chunks[i];
				}
			}
			
			return ConstructAndAppendChunk(archetype);
		}

		internal Chunk* ConstructAndAppendChunk(Archetype* archetype)
		{
			Chunk* chunk = this.chunkStore.Aquire();

			Debug.Assert(chunk != null);

			ChunkUtility.ConstructChunk(chunk, archetype);
			archetype->chunkList.Add(chunk);

			return chunk;
		}

		internal void ReleaseChunkIfEmpty(Chunk* chunk)
		{
			if(chunk->count == 0)
			{
				chunk->archetype->chunkList.Remove(chunk);
				this.chunkStore.Release(chunk);
			}

			// TODO: Dispose Buffers

			// TODO: Merge chunks
			//       var c1 = chunk->count < (archetype->chunkCapacity / 2);
			//       var c2 = chunk2->count < (archetype->chunkCapacity / 2);
			//       if(c1 && c2) ChunkUtility.MergeChunks(c1, c2);
		}
	}
}