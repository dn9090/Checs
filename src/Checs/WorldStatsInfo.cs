using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	public readonly struct WorldStatsInfo
	{
		public readonly int globalAllocatedChunks;

		public readonly int globalPooledChunks;

		public readonly int archetypes;

		public readonly int archetypeCapacity;

		public readonly int entities;

		public readonly int entityCapacity;

		public readonly int freeEntitySlots;

		public WorldStatsInfo(World world)
		{
			this.globalAllocatedChunks = ChunkPool.rentedCount + ChunkPool.count;
			this.globalPooledChunks = ChunkPool.count;
			this.archetypes = world.entityManager.archetypeStore.count;
			this.archetypeCapacity = world.entityManager.archetypeStore.capacity;
			this.entities = world.entityManager.entityStore.count;
			this.entityCapacity = world.entityManager.entityStore.capacity;
			this.freeEntitySlots = world.entityManager.entityStore.freeSlots.count;		}

		public string MakeHumanReadable() =>
			"-- Global"
			+ "\nAllocated Chunks:   " + this.globalAllocatedChunks
			+ "\nPooled Chunks:      " + this.globalPooledChunks
			+ "\n-- Store"
			+ "\nArchetype Capacity: " + this.archetypeCapacity
			+ "\nEntity Capacity:    " + this.entityCapacity
			+ "\nFree Entity Slots:  " + this.freeEntitySlots
			+ "\n-- World"
			+ "\nArchetypes:         " + this.archetypes
			+ "\nEntities:           " + this.entities;
	}
}