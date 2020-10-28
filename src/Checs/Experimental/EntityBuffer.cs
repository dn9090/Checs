using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checs
{

/*
struct EntityCommandBuffer : IDisposable
{

}

struct EntityCommandBufferHeader
{
	Entity entity
	EntityCommand cmd;
	int componentDataCount
	EntityCommandBufferData* bufferData
}

struct EntityCommandBufferData
{

}


*/


	/*
	
		REWORK THIS THING ENTIRELY!!!
	
	*/

	internal enum EntityCommand : int
	{
		None,
		Create,
		Modify,
		Destroy
	}

	internal unsafe struct ComponentDataInBuffer
	{
		public ComponentDataInBuffer* next;

		public int componentType;

		public byte* buffer;

		public static void WriteToBuffer<T>(ComponentDataInBuffer* componentData, T value) where T : unmanaged
		{

		}

		public static void Free(ComponentDataInBuffer* componentData)
		{
			// TODO: Iterate and free.
		}
	}

	internal unsafe struct EntityInBuffer : IComparable<EntityInBuffer>
	{
		public Entity entity;

		public EntityCommand command;

		public ComponentDataInBuffer* componentData;

		public EntityInBuffer(Entity entity)
		{
			this.entity = entity;
			this.command = EntityCommand.None;
			this.componentData = null;
		}

		public EntityInBuffer(Entity entity, EntityCommand command)
		{
			this.entity = entity;
			this.command = command;
			this.componentData = null;
		}

		public int CompareTo(EntityInBuffer other) => this.entity.CompareTo(other.entity);
	}

	public unsafe struct EntityBuffer : IDisposable
	{
		private const int DefaultCapacity = 16;

		internal int capacity => entitiesInBuffer.Length;

		internal int count;

		internal int[] bufferIndexByEntity;

		internal EntityInBuffer[] entitiesInBuffer;

		public static EntityBuffer Empty()
		{
			EntityBuffer buffer = new EntityBuffer();
			buffer.count = 0;
			buffer.bufferIndexByEntity = ArrayPool<int>.Shared.Rent(DefaultCapacity);
			buffer.entitiesInBuffer = ArrayPool<EntityInBuffer>.Shared.Rent(DefaultCapacity);

			return buffer;
		}

		public Entity CreateEntity()
		{
			Entity entity = new Entity(int.MaxValue - this.count, -1);
			Add(new EntityInBuffer(entity, EntityCommand.Create));

			return entity;
		}

		public void DestroyEntity(Entity entity)
		{
			int index = GetIndexOfEntityInBuffer(entity);

			if(index != -1)
			{
				this.entitiesInBuffer[index].command = EntityCommand.Destroy;
				ComponentDataInBuffer.Free(this.entitiesInBuffer[index].componentData);
				return;
			}

			Add(new EntityInBuffer(entity, EntityCommand.Destroy));
		}

		public bool AddOrSetComponentData<T>(Entity entity, T value) where T : unmanaged
		{
			int index = GetIndexOfEntityInBuffer(entity);

			if(index == -1)
				index = Add(new EntityInBuffer(entity, EntityCommand.Modify));

			var entityInBuffer = this.entitiesInBuffer[index];

			if(entityInBuffer.command == EntityCommand.Destroy)
				return false;
			
			ComponentDataInBuffer.WriteToBuffer(entityInBuffer.componentData, value);

			return true;
		}

		private int GetIndexOfEntityInBuffer(Entity entity)
		{
			var lookup = new EntityInBuffer(entity);
			return Array.BinarySearch<EntityInBuffer>(this.entitiesInBuffer, 0, this.count, lookup);
		}

		private int Add(EntityInBuffer entityInBuffer)
		{
			if(this.capacity <= this.count)
			{
				// TODO: ...
			}
				
				
			int index = this.count++;
			this.entitiesInBuffer[index] = entityInBuffer;
			Array.Sort<EntityInBuffer>(this.entitiesInBuffer, 0, this.count);

			return index;
		}

		public void Dispose()
		{
			this.count = 0;

			// TODO: Free memory.
			
			ArrayPool<int>.Shared.Return(this.bufferIndexByEntity);
			this.bufferIndexByEntity = null;

			ArrayPool<EntityInBuffer>.Shared.Return(this.entitiesInBuffer);
			this.entitiesInBuffer = null;
		}
	}
}