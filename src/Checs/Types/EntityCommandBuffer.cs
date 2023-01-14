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
		None,
		Entity,
		CreateEntity,
		DestroyEntity,
		DestroyArchetype,
		DestroyQuery,
		MoveEntity,
		Instantiate,
		WithComponentData,
		SetComponentData
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct CommandHeader
	{
		public CommandType type;

		public int byteCount;
	}

	[StructLayout(LayoutKind.Sequential, Size = 16)]
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
	internal struct InstantiateCommand
	{
		public CommandHeader header;

		public Entity entity;

		public int count;
	}

	[StructLayout(LayoutKind.Sequential, Size = 16)]
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

	internal unsafe struct CommandOffset
	{
		public CommandChunk* chunk;

		public int offset;
		
		public CommandHeader* header => (CommandHeader*)(chunk->buffer + offset);

		public CommandOffset(CommandChunk* chunk)
		{
			this.chunk = chunk;
			this.offset = 0;
		}

		public void Move()
		{
			this.offset += header->byteCount;
		}

		public bool TryNext()
		{
			Move();

			if(offset < Chunk.BufferSize)
				return true;

			if(chunk->next == null)
				return false;
			
			chunk = chunk->next;
			offset = 0;

			return true;
		}
	}


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
			this.head           = (CommandChunk*)this.manager.chunkStore.Aquire();
			this.head->current  = this.head;
			this.sequenceNumber = this.head->sequenceNumber;

			ConstructChunk(this.head);
		}

		public void CreateEntity(EntityArchetype archetype, int count = 1)
		{
			CheckDisposed();

			unsafe
			{
				var chunk = this.head->current;
				var prevCommand = (CreateEntityCommand*)chunk->prevCommand;

				if(prevCommand->header.type != CommandType.CreateEntity
					|| prevCommand->archetype != archetype)
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
				var chunk = this.head->current;

				var command = (DestroyEntityCommand*)Bump(CommandType.DestroyEntity, sizeof(DestroyEntityCommand));

				AppendEntities(entities);
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

		public void MoveEntity(ReadOnlySpan<Entity> entities, EntityArchetype archetype)
		{
			CheckDisposed();

			unsafe
			{
				var chunk = this.head->current;

				var command = (MoveEntityCommand*)Bump(CommandType.MoveEntity, sizeof(MoveEntityCommand));
				command->archetype = archetype;

				AppendEntities(entities);
			}
		}

		public void Instantiate(Entity entity, int count = 1)
		{
			CheckDisposed();

			unsafe
			{
				var chunk = this.head->current;
				var prevCommand = (InstantiateCommand*)chunk->prevCommand;

				if(prevCommand->header.type == CommandType.Instantiate
					&& prevCommand->entity == entity)
				{
					prevCommand->count += count;
					return;
				}

				var command = (InstantiateCommand*)Bump(CommandType.Instantiate, sizeof(InstantiateCommand));
				command->entity = entity;
				command->count  = count;
			}
		}
	
		public void SetComponentData<T>(in T value) where T : unmanaged
		{
			CheckDisposed();

			var typeInfo = TypeRegistry<T>.info;

			unsafe
			{
				var chunk = this.head->current;

				var command = (SetComponentDataCommand*)Bump(CommandType.WithComponentData,
					sizeof(SetComponentDataCommand) + sizeof(T));
				command->hashCode = typeInfo.hashCode;
				command->size     = sizeof(T);

				Unsafe.Write(command + 1, value);
			}
		}

		public void SetComponentData<T>(ReadOnlySpan<Entity> entities, in T value) where T : unmanaged
		{
			CheckDisposed();

			var typeInfo = TypeRegistry<T>.info;

			unsafe
			{
				var chunk = this.head->current;

				var command = (SetComponentDataCommand*)Bump(CommandType.SetComponentData,
					sizeof(SetComponentDataCommand) + sizeof(T));
				command->hashCode = typeInfo.hashCode;
				command->size     = sizeof(T);

				Unsafe.Write(command + 1, value);
				
				AppendEntities(entities);
			}
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

						this.manager.chunkStore.Release((Chunk*)chunk);
					}
				}
			}
		}

		internal unsafe void AppendEntities(ReadOnlySpan<Entity> entities)
		{
			fixed(Entity* ptr = entities)
			{
				var src = ptr;
				var absoluteByteCount = entities.Length * sizeof(Entity);
			
				while(absoluteByteCount > 0)
				{
					var command   = (EntityCommand*)Bump(CommandType.Entity, sizeof(EntityCommand),
						sizeof(Entity), absoluteByteCount, out var reservedByteCount);
					var count     = reservedByteCount / sizeof(Entity);
					var byteCount = count * sizeof(Entity);

					command->count = count;
					Unsafe.CopyBlockUnaligned(command + 1, src, (uint)byteCount);

					src += count;
					absoluteByteCount -= byteCount;
				}
			}
		}

		internal unsafe CommandHeader* Bump(CommandType type, int byteCount)
		{
			var alignedByteCount = Align16(byteCount);
			var capacity = this.head->current->capacity;

			if(capacity < alignedByteCount)
				AquireNextChunk();

			++this.head->absoluteCommandCount;

			var chunk = this.head->current;
			var header = (CommandHeader*)(chunk->buffer + chunk->used);

			header->type      = type;
			header->byteCount = alignedByteCount;

			chunk->used         += alignedByteCount;
			chunk->commandCount += 1;
			chunk->prevCommand   = header;

			return header;
		}

		internal unsafe CommandHeader* Bump(CommandType type, int byteCount,
			int minByteCount, int maxByteCount, out int reservedByteCount)
		{
			var alignedByteCount = Align16(byteCount);
			var capacity = this.head->current->capacity;

			if(capacity < (alignedByteCount + minByteCount))
				AquireNextChunk();

			++this.head->absoluteCommandCount;

			var chunk            = this.head->current;
			var header           = (CommandHeader*)(chunk->buffer + chunk->used);
			var reservedCapacity = chunk->capacity - alignedByteCount;

			reservedByteCount = reservedCapacity < maxByteCount ? reservedCapacity : maxByteCount;
			var alignedReservedByteCount = Align16(reservedByteCount);

			header->type      = type;
			header->byteCount = alignedByteCount + alignedReservedByteCount;

			chunk->used         += alignedByteCount + alignedReservedByteCount;
			chunk->commandCount += 1;
			chunk->prevCommand   = header;

			return header;
		}

		internal unsafe void AquireNextChunk()
		{
			var next = (CommandChunk*)this.manager.chunkStore.Aquire();

			this.head->current->next = next;
			this.head->current = next;

			ConstructChunk(next);
		}

		internal unsafe void CheckDisposed()
		{
			if(this.sequenceNumber != this.head->sequenceNumber)
				throw new ObjectDisposedException(nameof(EntityCommandBuffer));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static int Align16(int byteCount)
		{
			return ((byteCount - 1) | 15) + 1;
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

	internal static unsafe class CommandUtility
	{
		public static Span<Entity> AsSpan(EntityCommand* command)
		{
			return new Span<Entity>(command + 1, command->count);
		}
	}
}