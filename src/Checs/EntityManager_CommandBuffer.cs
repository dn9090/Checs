using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{
	public unsafe partial class EntityManager
	{
		public EntityCommandBuffer CreateCommandBuffer()
		{
			return new EntityCommandBuffer(this);
		}

		public void Playback(EntityCommandBuffer buffer)
		{
			if(buffer.manager != this)
				return; // throw
			
			buffer.CheckDisposed();

			PlaybackInternal(buffer.head);

			var next          = buffer.head->next;
			buffer.head->next = null;

			while(next != null)
			{
				var chunk = next;
				PlaybackInternal(chunk);

				next        = chunk->next;
				chunk->next = null;

				// Releasing the chunk directly so that if the playback of the next chunk takes
				// longer other threads can reuse the chunk earlier.
				// Also avoids repeated interlocking operations.
				this.chunkStore.Release((Chunk*)chunk);
			}

			buffer.Clear();
		}

		internal void PlaybackInternal(CommandChunk* chunk)
		{
			var count = chunk->commandCount;
			var offset = new CommandOffset(chunk);

			while(count-- > 0)
			{
				var header = offset.header;

				switch(header->type)
				{
					case CommandType.CreateEntity:
					{
						var command = (CreateEntityCommand*)header;
						// TODO: Peek withcomponentdata
						CreateEntity(command->archetype, command->count);
						break;
					}
					case CommandType.DestroyEntity:
					{
						var command = (DestroyEntityCommand*)header;
						var peekOffset = offset;

						while(TryPeekNextCommand(CommandType.Entity, ref peekOffset))
						{
							var entityCommand = (EntityCommand*)peekOffset.header;
							var entityBuffer = new Span<Entity>(entityCommand + 1, entityCommand->count);

							DestroyEntity(entityBuffer);
						}

						break;
					}
					case CommandType.DestroyArchetype:
					{
						var command = (DestroyArchetypeCommand*)header;
						DestroyEntity(command->archetype);
						break;
					}
					case CommandType.DestroyQuery:
					{
						var command = (DestroyQueryCommand*)header;
						DestroyEntity(command->query);
						break;
					}
					case CommandType.MoveEntity:
					{
						var command = (MoveEntityCommand*)header;
						var peekOffset = offset;

						while(TryPeekNextCommand(CommandType.Entity, ref peekOffset))
						{
							var entityCommand = (EntityCommand*)peekOffset.header;
							var entityBuffer = new Span<Entity>(entityCommand + 1, entityCommand->count);

							MoveEntity(entityBuffer, command->archetype);
						}
						break;
					}
					case CommandType.Instantiate:
					{
						var command = (InstantiateCommand*)header;
						Instantiate(command->entity, command->count);
						break;
					}
					case CommandType.SetComponentData:
					{
						var command = (SetComponentDataCommand*)header;
						var peekOffset = offset;

						while(TryPeekNextCommand(CommandType.Entity, ref peekOffset))
						{
							var entityCommand = (EntityCommand*)peekOffset.header;
							var entityBuffer = new Span<Entity>(entityCommand + 1, entityCommand->count);

							WriteComponentData(entityBuffer, (byte*)(command + 1), command->size, command->hashCode);
						}

						break;
					};
				}

				offset.Move();
			}
		}

		internal static bool TryPeekNextCommand(CommandType type, ref CommandOffset offset)
		{
			if(offset.TryNext() && offset.header->type == type)
				return true;

			return false;
		}

		internal static void WithComponentData(EntityTable table, CommandOffset offset)
		{
			while(TryPeekNextCommand(CommandType.WithComponentData, ref offset))
			{
				var command = (SetComponentDataCommand*)offset.header;
				var value = (byte*)(command + 1);
			}
		}
	}
}