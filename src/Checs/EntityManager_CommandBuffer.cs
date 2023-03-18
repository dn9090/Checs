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

		public void Playback(EntityCommandBuffer buffer, bool clear = true)
		{
			if(buffer.manager != this)
				throw new ArgumentException("The command buffer does not belong to the manager."); // TODO: Better name.
			
			buffer.CheckDisposed();

			var playback = new CommandPlayback(buffer.head);

			while(playback.chunk != null)
			{
				PlaybackInternal(&playback);
				playback.chunk = playback.chunk->next;
			}

			if(clear)
				buffer.Clear();
		}

		internal void PlaybackInternal(CommandPlayback* playback)
		{
			var at    = playback->chunk->buffer;
			var count = playback->chunk->commandCount;

			while(count-- > 0)
			{
				var header = (CommandHeader*)at;
				at        += header->byteCount;
				
				switch(header->type)
				{
					case CommandType.CreateEntity:
					{
						var command = (CreateEntityCommand*)header;
						CreateEntity(command->archetype, command->count);
					} break;
					case CommandType.DestroyEntity | CommandType.Entity:
					{
						var command  = (EntityCommand*)header;
						var entities = EntityCommandBuffer.GetEntities(command);
						DestroyEntity(entities);
					} break;
					case CommandType.DestroyArchetype:
					{
						var command = (DestroyArchetypeCommand*)header;
						DestroyEntity(command->archetype);
					} break;
					case CommandType.DestroyQuery:
					{
						var command = (DestroyQueryCommand*)header;
						DestroyEntity(command->query);
					} break;
					case CommandType.MoveEntity | CommandType.Entity:
					{
						var command  = (EntityCommand*)header;
						var entities = EntityCommandBuffer.GetEntities(command);
						var prev     = (MoveEntityCommand*)playback->prevCommand;
						MoveEntity(entities, prev->archetype);
					} break;
					case CommandType.InstantiateEntity:
					{
						var command = (InstantiateEntityCommand*)header;
						Instantiate(command->entity, command->count);
					} break;
					case CommandType.InstantiatePrefab:
					{
						var command = (InstantiatePrefabCommand*)header;
						Instantiate(command->handle.Target as EntityPrefab); // TODO: Count
					} break;
				}

				if((header->type & CommandType.Entity) == 0)
					playback->prevCommand = header;
			}
		}
	}
}