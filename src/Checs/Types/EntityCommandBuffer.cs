using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Checs
{
	[Flags]
	internal enum CommandType : int
	{
		None              = 0,
		Entity            = 1 << 0,
		CreateEntity      = 1 << 1,
		DestroyEntity     = 1 << 2,
		DestroyArchetype  = 1 << 3,
		DestroyQuery      = 1 << 4,
		MoveEntity        = 1 << 5,
		InstantiateEntity = 1 << 6,
		InstantiatePrefab = 1 << 7,
		SetComponentData  = 1 << 8,
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct CommandHeader
	{
		public CommandType type;

		public int byteCount;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct EntityCommand
	{
		public CommandHeader header;

		public int count;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct CreateEntityCommand
	{
		public CommandHeader header;

		public EntityArchetype archetype;

		public int count;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct DestroyEntityCommand
	{
		public CommandHeader header;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct DestroyArchetypeCommand
	{
		public CommandHeader header;

		public EntityArchetype archetype;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct DestroyQueryCommand
	{
		public CommandHeader header;

		public EntityQuery query;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct MoveEntityCommand
	{
		public CommandHeader header;

		public EntityArchetype archetype;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct InstantiateEntityCommand
	{
		public CommandHeader header;

		public Entity entity;

		public int count;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct InstantiatePrefabCommand
	{
		public CommandHeader header;

		public GCHandle handle;

		public int count;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct SetComponentDataCommand
	{
		public CommandHeader header;

		public uint hashCode;

		public int size;
	}

	[StructLayout(LayoutKind.Explicit)]
	internal unsafe struct CommandChunk
	{
		[FieldOffset(0)]
		public int used;

		[FieldOffset(4)]
		public int commandCount;

		[FieldOffset(8)]
		public int handleCount;

		[FieldOffset(12)]
		public int absoluteCommandCount;

		[FieldOffset(16)]
		public CommandHeader* prevCommand;

		[FieldOffset(24)]
		public CommandChunk* next;

		[FieldOffset(32)]
		public CommandChunk* current;

		[FieldOffset(44)]
		public uint sequenceNumber;

		[FieldOffset(64)]
		public fixed byte buffer[4];

		public int capacity => Chunk.BufferSize - used;
	}

	internal unsafe struct CommandPlayback
	{
		public CommandChunk* chunk;

		public CommandHeader* prevCommand;

		public CommandPlayback(CommandChunk* chunk)
		{
			this.chunk = chunk;
			this.prevCommand = null;
		}
	}
	
	// +--------------+
	// | CHUNK HEADER |
	// |--------------|
	// |   COMMAND    |  |
	// |--------------|  |
	// |   COMMAND    |  v
	// |--------------|
	// |     ...      |
	// |--------------|
	// |   GCHANDLE   |  ^
	// |--------------|  |
	// |   GCHANDLE   |  |
	// +--------------+

	public struct EntityCommandBuffer : IDisposable
	{
		public int count
		{
			get
			{
				unsafe
				{
					CheckDisposed();
					return this.head->absoluteCommandCount;
				}
			}
		}

		internal EntityManager manager;

		internal unsafe CommandChunk* head;

		internal uint sequenceNumber;

		internal unsafe EntityCommandBuffer(EntityManager manager)
		{
			this.manager        = manager;
			this.head           = (CommandChunk*)manager.chunkStore.Aquire();
			this.head->current  = head;
			this.sequenceNumber = this.head->sequenceNumber;

			ConstructChunk(this.head);
		}

		public void Clear()
		{
			CheckDisposed();

			unsafe
			{
				var next = this.head->next;

				while(next != null)
				{
					var chunk = next;
					next = chunk->next;

					this.manager.chunkStore.Release((Chunk*)chunk);
				}

				ConstructChunk(this.head);
				this.head->current = this.head;
			}
		}

		public void Dispose()
		{
			unsafe
			{
				if(this.head != null && this.sequenceNumber == this.head->sequenceNumber)
				{
					var next = this.head;

					while(next != null)
					{
						var chunk = next;
						next = chunk->next;

						// Releasing increments the sequence number of the head chunk.
						this.manager.chunkStore.Release((Chunk*)chunk);
					}
				}
			}
		}

		public void CreateEntity(int count = 1)
		{
			CreateEntity(EntityArchetype.empty, count);
		}

		public void CreateEntity(EntityArchetype archetype, int count = 1)
		{
			CheckDisposed();

			unsafe
			{
				var chunk       = this.head->current;
				var prevCommand = (CreateEntityCommand*)chunk->prevCommand;

				if(prevCommand->header.type == CommandType.CreateEntity
					&& prevCommand->archetype == archetype)
				{
					prevCommand->count += count;
					return;
				}

				var command = (CreateEntityCommand*)Bump(CommandType.CreateEntity, sizeof(CreateEntityCommand));
				command->archetype = archetype;
				command->count     = count;
			}
		}

		public void DestroyEntity(ReadOnlySpan<Entity> entities)
		{
			CheckDisposed();

			unsafe
			{
				Bump(CommandType.DestroyEntity, sizeof(DestroyEntityCommand));
				AppendEntities(CommandType.DestroyEntity, entities);
			}
		}

		public void DestroyEntity(EntityArchetype archetype)
		{
			CheckDisposed();

			unsafe
			{
				var chunk = this.head->current;
				var prevCommand = (DestroyArchetypeCommand*)chunk->prevCommand;

				if(prevCommand->header.type == CommandType.DestroyArchetype
					&& prevCommand->archetype == archetype)
					return;

				var command = (DestroyArchetypeCommand*)Bump(CommandType.DestroyArchetype, sizeof(DestroyArchetypeCommand));
				command->archetype = archetype;
			}
		}

		public void DestroyEntity(EntityQuery query)
		{
			CheckDisposed();

			unsafe
			{
				var chunk = this.head->current;
				var prevCommand = (DestroyQueryCommand*)chunk->prevCommand;

				if(prevCommand->header.type == CommandType.DestroyQuery
					&& prevCommand->query == query)
					return;

				var command = (DestroyQueryCommand*)Bump(CommandType.DestroyQuery, sizeof(DestroyQueryCommand));
				command->query = query;
			}
		}

		public void MoveEntity(Entity entity, EntityArchetype archetype)
		{
			unsafe
			{
				MoveEntity(new ReadOnlySpan<Entity>(&entity, 1), archetype);
			}
		}

		public void MoveEntity(ReadOnlySpan<Entity> entities, EntityArchetype archetype)
		{
			CheckDisposed();

			unsafe
			{
				var command = (MoveEntityCommand*)Bump(CommandType.MoveEntity, sizeof(MoveEntityCommand));
				command->archetype = archetype;

				AppendEntities(CommandType.MoveEntity, entities);
			}
		}

		public void Instantiate(Entity entity, int count = 1)
		{
			CheckDisposed();

			unsafe
			{
				var chunk = this.head->current;
				var prevCommand = (InstantiateEntityCommand*)chunk->prevCommand;

				if(prevCommand->header.type == CommandType.InstantiateEntity
					&& prevCommand->entity == entity)
				{
					prevCommand->count += count;
					return;
				}

				var command = (InstantiateEntityCommand*)Bump(CommandType.InstantiateEntity, sizeof(InstantiateEntityCommand));
				command->entity = entity;
				command->count  = count;
			}
		}

		public void Instantiate(EntityPrefab prefab, int count = 1)
		{
			CheckDisposed();

			unsafe
			{
				var chunk = this.head->current;
				var prevCommand = (InstantiatePrefabCommand*)chunk->prevCommand;

				if(prevCommand->header.type == CommandType.InstantiatePrefab
					&& prevCommand->handle.Target == prefab)
				{
					prevCommand->count += count;
					return;
				}

				var command = (InstantiatePrefabCommand*)Bump(CommandType.InstantiatePrefab, sizeof(InstantiatePrefabCommand));
				command->handle = Bump(prefab);
				command->count  = count;
			}
		}

		public void SetComponentData<T>(Entity entity, in T value) where T : unmanaged
		{
			unsafe
			{
				SetComponentData(new ReadOnlySpan<Entity>(&entity, 1), in value);
			}
		}

		public void SetComponentData<T>(ReadOnlySpan<Entity> entities, in T value) where T : unmanaged
		{
			CheckDisposed();

			unsafe
			{
				var typeInfo = TypeRegistry<T>.info;
				var command = (SetComponentDataCommand*)Bump(CommandType.SetComponentData,
					sizeof(SetComponentDataCommand), typeInfo.size, 1, out _);
				command->hashCode = typeInfo.hashCode;
				command->size     = typeInfo.size;

				var buffer = GetBuffer(command, sizeof(SetComponentDataCommand));
				Unsafe.WriteUnaligned(buffer, value);

				AppendEntities(CommandType.SetComponentData, entities);
			}
		}

		internal unsafe void AppendEntities(CommandType type, ReadOnlySpan<Entity> entities)
		{
			var count = 0;

			while(count < entities.Length)
			{
				var remaining = entities.Length - count;
				var command   = (EntityCommand*)Bump(type | CommandType.Entity, sizeof(EntityCommand),
						sizeof(Entity), remaining, out var reservedCount);
				var slice     = entities.Slice(count, reservedCount);

				command->count = reservedCount;
				count         += reservedCount;

				slice.CopyTo(GetEntities(command));
			}
		}

		internal static unsafe Span<Entity> GetEntities(EntityCommand* command)
		{
			var buffer = (byte*)command + ChunkUtility.Align(sizeof(EntityCommand));
			return new Span<Entity>(buffer, command->count);
		}

		internal static unsafe byte* GetBuffer(void* command, int commandSize)
		{
			return (byte*)command + ChunkUtility.Align(commandSize);
		}

		internal unsafe CommandHeader* Bump(CommandType type, int commandSize)
		{
			var byteCount = ChunkUtility.Align(commandSize);

			EnsureCapacity(byteCount);

			var chunk  = this.head->current;

			++this.head->absoluteCommandCount;

			return Bump(chunk, type, byteCount);
		}

		internal unsafe CommandHeader* Bump(CommandType type, int commandSize, int elementSize, int elementCount, out int reservedCount)
		{
			var byteCount    = ChunkUtility.Align(commandSize);
			var minByteCount = byteCount + ChunkUtility.Align(elementSize); // At least one element is guaranteed to be reserved.

			EnsureCapacity(minByteCount);

			var chunk  = this.head->current;

			++this.head->absoluteCommandCount;

			var remainingCapacity = chunk->capacity - byteCount;
			var maxElementCount   = remainingCapacity / elementSize;
			
			reservedCount = maxElementCount > elementCount ? elementCount : maxElementCount;
			
			return Bump(chunk, type, byteCount + ChunkUtility.AlignComponentArraySize(elementSize, reservedCount));
		}

		internal static unsafe CommandHeader* Bump(CommandChunk* chunk, CommandType type, int byteCount)
		{
			var header = (CommandHeader*)(chunk->buffer + chunk->used);
			
			header->type      = type;
			header->byteCount = byteCount;

			chunk->used         += byteCount;
			chunk->commandCount += 1;
			chunk->prevCommand   = header;

			return header;
		}

		internal unsafe GCHandle Bump(object target)
		{
			throw new InvalidOperationException(); // TODO
		}

		internal unsafe void EnsureCapacity(int byteCount)
		{
			if(byteCount > Chunk.BufferSize)
				throw new InvalidOperationException();

			if(this.head->current->capacity < byteCount)
			{
				var chunk = (CommandChunk*)this.manager.chunkStore.Aquire();

				this.head->current->next = chunk;
				this.head->current       = chunk;

				ConstructChunk(chunk);
			}
		}

		internal unsafe void CheckDisposed()
		{
			if(this.sequenceNumber != this.head->sequenceNumber)
				throw new ObjectDisposedException(nameof(EntityCommandBuffer));
		}

		internal unsafe static void ConstructChunk(CommandChunk* chunk)
		{
			chunk->next                 = null;
			chunk->used                 = 0;
			chunk->commandCount         = 0;
			chunk->absoluteCommandCount = 0;
			chunk->prevCommand          = (CommandHeader*)chunk->buffer;
			chunk->prevCommand->type    = CommandType.None;
		}
	}
}