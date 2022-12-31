using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	public unsafe partial class EntityManager
	{
		/// <summary>
		/// Creates the empty archetype.
		/// </summary>
		/// <remarks>
		/// Is equivalent to <see cref="EntityArchetype.empty"/> or
		/// the <c>default</c> archetype.
		/// </remarks>
		/// <returns>The empty archetype.</returns>
		public EntityArchetype CreateArchetype()
		{
			return EntityArchetype.empty;
		}

		/// <summary>
		/// Creates an archetype with the specified component type,
		/// unless the archetype already exists.
		/// </summary>
		/// <param name="type">The component type.</param>
		/// <returns>A new or existing matching archetype.</returns>
		public EntityArchetype CreateArchetype(ComponentType type)
		{
			if(type.isEntity)
				return EntityArchetype.empty;

			var hashCodes = stackalloc uint[2] { 0, type.hashCode };
			var sizes     = stackalloc int[2] { sizeof(Entity), type.size };

			return CreateArchetypeInternal(hashCodes, sizes, 2);
		}

		/// <summary>
		/// Creates an archetype with the specified set of component types,
		/// unless the archetype already exists.
		/// </summary>
		/// <param name="types">The component types.</param>
		/// <returns>A new or existing matching archetype.</returns>
		public EntityArchetype CreateArchetype(ReadOnlySpan<ComponentType> types)
		{
			if(types.Length == 0)
				return EntityArchetype.empty;
			
			var typeCount = types.Length + 1;
			var hashCodes = stackalloc uint[typeCount];
			var sizes     = stackalloc int[typeCount];
			
			hashCodes[0] = 0; sizes[0] = sizeof(Entity);

			var count = TypeUtility.Sort(types, hashCodes, sizes, 1);

			return CreateArchetypeInternal(hashCodes, sizes , count);
		}

		/// <summary>
		/// Creates an archetype with the specified set of component types,
		/// unless the archetype already exists.
		/// </summary>
		/// <param name="types">The component types.</param>
		/// <returns>A new or existing matching archetype.</returns>
		public EntityArchetype CreateArchetype(params ComponentType[] types)
		{
			return CreateArchetype(types.AsSpan());
		}

		/// <summary>
		/// Creates an archetype which extends the given archetype with
		/// the specified set of component types, unless the archetype already exists.
		/// </summary>
		/// <param name="archetype">The archetype to extend.</param>
		/// <param name="types">The component types.</param>
		/// <returns>A new or existing matching archetype.</returns>
		public EntityArchetype CreateArchetype(EntityArchetype archetype, ReadOnlySpan<ComponentType> types)
		{
			if(types.Length == 0)
				return archetype;

			var arch = GetArchetypeInternal(archetype);

			var typeCount = arch->componentCount + types.Length;
			var hashCodes = stackalloc uint[typeCount];
			var sizes     = stackalloc int[typeCount];

			ArchetypeUtility.Copy(arch, hashCodes, sizes);

			var count = TypeUtility.Sort(types, hashCodes, sizes, arch->componentCount);

			return CreateArchetypeInternal(hashCodes, sizes, count);
		}

		public EntityArchetype CreateArchetypeWithout(EntityArchetype archetype, ReadOnlySpan<ComponentType> types)
		{
			if(types.Length == 0)
				return archetype;

			var hashCodes = stackalloc uint[types.Length];

			var count = TypeUtility.Sort(types, hashCodes);
			var arch = GetArchetypeInternal(archetype);

			return CreateArchetypeWithoutInternal(arch, hashCodes, count);
		}

		/// <summary>
		/// Creates a new archetypes by combining component types
		/// of two archetypes, unless the archetype already exists.
		/// </summary>
		/// <param name="lhs">The first archetype.</param>
		/// <param name="rhs">The second archetype.</param>
		/// <returns>A new or existing matching archetype.</returns>
		public EntityArchetype CombineArchetypes(EntityArchetype lhs, EntityArchetype rhs)
		{
			if(lhs == rhs)
				return lhs;
			
			if(lhs.isEmpty)
				return rhs;
			
			if(rhs.isEmpty)
				return lhs;

			var archLhs = GetArchetypeInternal(lhs);
			var archRhs = GetArchetypeInternal(rhs);

			return CreateArchetypeInternal(archLhs, archRhs);
		}

		/// <summary>
		/// Gets the number of existing entities of the specified archetype.
		/// </summary>
		/// <param name="archetype">The archetype.</param>
		/// <returns>The number of entities.</returns>
		public int GetEntityCount(EntityArchetype archetype)
		{
			return GetArchetypeInternal(archetype)->entityCount;
		}

		/// <summary>
		/// Copies entities of the specified archetype to the buffer.
		/// </summary>
		/// <param name="archetype">The archetype.</param>
		/// <param name="entities">The destination buffer.</param>
		/// <returns>The number of copied entities.</returns>
		public int GetEntities(EntityArchetype archetype, Span<Entity> entities)
		{
			Archetype* arch = GetArchetypeInternal(archetype);
			return GetEntitiesInternal(arch, entities);
		}

		/// <summary>
		/// Returns a single entity of the specified archetype.
		/// </summary>
		/// <param name="archetype">The archetype.</param>
		/// <returns>An entity of the specified archetype or the default entity.</returns>
		public Entity GetEntity(EntityArchetype archetype)
		{
			Span<Entity> buffer = stackalloc Entity[1];
			var arch  = GetArchetypeInternal(archetype);
			var found = GetEntitiesInternal(arch, buffer);
			
			return found > 0 ? buffer[0] : default;
		}

		internal int GetEntitiesInternal(Archetype* archetype, Span<Entity> entities)
		{
			var chunkCount = archetype->chunkList.count;
			var chunks     = archetype->chunkList.chunks;
			var count = 0;
			
			fixed(Entity* ptr = entities)
			{
				for(int i = 0; i < chunkCount && count < entities.Length; ++i)
					count += ChunkUtility.CopyEntities(chunks[i], ptr + count, entities.Length - count);
			}

			return count;
		}

		/// <summary>
		/// Gets the number of component types in the archetype including
		/// the <see cref="Entity"/> type.
		/// </summary>
		/// <param name="archetype">The archetype.</param>
		/// <returns>The number of component types.</returns>
		public int GetComponentCount(EntityArchetype archetype)
		{
			var arch = GetArchetypeInternal(archetype);
			return arch->componentCount;
		}

		/// <summary>
		/// Gets the <see cref="System.Type"/> of the component types in
		/// the archetype including the <see cref="Entity"/> type.
		/// </summary>
		/// <param name="archetype">The archetype.</param>
		/// <param name="types">The destination buffer.</param>
		/// <returns>The number of types in the buffer.</returns>
		public int GetTypes(EntityArchetype archetype, Span<Type> types)
		{
			var arch      = GetArchetypeInternal(archetype);
			var hashCodes = Archetype.GetComponentHashCodes(arch);

			return TypeRegistry.GetTypes(hashCodes, arch->componentCount, types);
		}

		/// <summary>
		/// Gets the component types in the archetype including the
		/// <see cref="Entity"/> component type.
		/// </summary>
		/// <param name="archetype">The archetype.</param>
		/// <param name="types">The destination buffer.</param>
		/// <returns>The number of types in the buffer.</returns>
		public int GetComponentTypes(EntityArchetype archetype, Span<ComponentType> types)
		{
			var arch      = GetArchetypeInternal(archetype);
			var hashCodes = Archetype.GetComponentHashCodes(arch);
			var sizes     = Archetype.GetComponentSizes(arch);

			var count = arch->componentCount > types.Length ? types.Length : arch->componentCount;

			for(int i = 0; i < count; ++i)
				types[i] = new ComponentType(hashCodes[i], sizes[i]);

			return count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Archetype* GetArchetypeInternal(EntityArchetype archetype)
		{
			return this.archetypeStore.archetypes[archetype.index];
		}

		internal void CreateEmptyArchetype()
		{
			var hashCode = 0u;
			var size     = sizeof(Entity);

			CreateArchetypeInternal(&hashCode, &size, 1);
		}

		/// <summary>
		/// Creates an archetype based on the provided type information.
		/// </summary>
		/// <remarks>
		/// Note that the entity information must be in the first index
		/// and the type information must be sorted based on the hash code.
		/// </remarks>
		internal EntityArchetype CreateArchetypeInternal(uint* typeHashCodes, int* typeSizes, int typeCount)
		{
			var hashCode = xxHash.GetHashCode(typeHashCodes, typeCount) & 0x7FFFFFFF;

			if(this.lookupTable.TryGet(hashCode, out var index))
				return new EntityArchetype(index);

			var chunkCapacity = ChunkUtility.CalculateBufferCapacity(typeSizes, typeCount);
			var bufferSize    = Archetype.SizeOfBuffer(typeCount);

			if(chunkCapacity == 0)
				throw new ArgumentOutOfRangeException("The archetype is too large.");

			var archetype = this.archetypeStore.Aquire(bufferSize);

			this.lookupTable.Add(hashCode, archetype->index);
			
			Archetype.Construct(archetype, typeHashCodes, typeSizes, typeCount, chunkCapacity);

			return new EntityArchetype(archetype->index);
		}

		/// <summary>
		/// Creates an archetype with all components of both archetypes. 
		/// </summary>
		internal EntityArchetype CreateArchetypeInternal(Archetype* lhs, Archetype* rhs)
		{
			// One less because we dont need to copy the entity information twice.
			var bufferCount = lhs->componentCount + rhs->componentCount - 1;

			var combinedHashCodes = stackalloc uint[bufferCount];
			var combinedSizes     = stackalloc int[bufferCount];
			var combinedCount     = ArchetypeUtility.Union(lhs, rhs, combinedHashCodes, combinedSizes);
		
			return CreateArchetypeInternal(combinedHashCodes, combinedSizes, combinedCount);
		}

		internal EntityArchetype CreateArchetypeWithoutInternal(Archetype* archetype, uint* typeHashCodes, int typeCount)
		{
			var excludedHashCodes = stackalloc uint[archetype->componentCount];
			var excludedSizes     = stackalloc int[archetype->componentCount];
			var excludedCount     = ArchetypeUtility.Difference(archetype, typeHashCodes, typeCount, excludedHashCodes, excludedSizes);

			return CreateArchetypeInternal(excludedHashCodes, excludedSizes, excludedCount);
		}

		/// <summary>
		/// Extends a given archetype with the provided type information.
		/// </summary>
		/// <remarks>
		/// The type information does not have to be sorted.
		/// </remarks>
		internal EntityArchetype CreateArchetypeInternal(Archetype* archetype, uint* typeHashCodes, int* typeSizes, int typeCount)
		{
			var combinedCount     = archetype->componentCount + typeCount;
			var combinedHashCodes = stackalloc uint[combinedCount];
			var combinedSizes     = stackalloc int[combinedCount];

			ArchetypeUtility.Copy(archetype, combinedHashCodes, combinedSizes);

			var size = sizeof(int) * typeCount;
			Unsafe.CopyBlockUnaligned(combinedHashCodes + archetype->componentCount, typeHashCodes, (uint)size);
			Unsafe.CopyBlockUnaligned(combinedSizes     + archetype->componentCount, typeSizes, (uint)size);

			var uniqueCount = TypeUtility.Sort(combinedHashCodes, combinedSizes, combinedCount); // TODO
			var startIndex = combinedCount - uniqueCount;

			return CreateArchetypeInternal(combinedHashCodes + startIndex, combinedSizes + startIndex, uniqueCount);
		}
	}
}