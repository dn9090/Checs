using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	/// <summary>
	/// Header of a snapshot.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Size = 32)]
	public struct EntitySnapshot
	{
		/// <summary>
		/// The number of entities in the snapshot.
		/// </summary>
		public int entityCount;

		/// <summary>
		/// The entity version of the manager.
		/// </summary>
		public uint entityVersion;

		/// <summary>
		/// The overall capacity of entities in the manager.
		/// </summary>
		public int entityCapacity;

		/// <summary>
		/// The hash-code of the archetype.
		/// </summary>
		public uint hashCode;

		/// <summary>
		/// The number of components in the snapshot.
		/// </summary>
		public int componentCount;

		/// <summary>
		/// The number of chunks in the snapshot.
		/// </summary>
		public int chunkCount;

		internal unsafe EntitySnapshot(Archetype* archetype, uint entityVersion, int entityCapacity)
		{
			this.entityCount    = archetype->entityCount;
			this.entityVersion  = entityVersion;
			this.entityCapacity = entityCapacity;
			this.hashCode       = archetype->hashCode;
			this.componentCount = archetype->componentCount;
			this.chunkCount     = archetype->chunkList.count;
		}
	}
}
