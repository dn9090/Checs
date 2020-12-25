using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;
using Checs;
using Shoot_n_Mine.Engine;
using Shoot_n_Mine.Net;

namespace Shoot_n_Mine
{
	
	public sealed class NetworkingSystem : ComponentSystem
	{
		private volatile bool m_Run;

		private byte[] m_ReceiveBuffer;

		private NetworkStream m_Stream;

		public NetworkingSystem()
		{
			this.m_ReceiveBuffer = new byte[NetworkBuffer.Capacity];
			this.m_Run = false;
		}

		public override void OnEnabled(Scene scene)
		{
			this.m_Run = true;
			this.m_Stream = Networking.client.stream;
			ThreadPool.QueueUserWorkItem(TransmitNetworkBuffer);
		}

		public override void OnDisabled(Scene scene)
		{
			this.m_Run = false;
		}

		public override void OnUpdate(Scene scene)
		{
			if(this.m_Stream.DataAvailable)
				ExecuteCommands(scene.world);
		}

		private unsafe void ExecuteCommands(World world)
		{
			var bytes = this.m_Stream.Read(this.m_ReceiveBuffer);

			fixed(byte* buffer = this.m_ReceiveBuffer)
			{
				int offset = 0;

				while(offset < bytes)
				{
					var ptr = buffer + offset;
					var command = ((Command*)ptr)[0];

					switch(command.cmdType)
					{
						case CommandType.RocketLaunch:
						{
							var cmd = ((RocketLaunchCommand*)ptr)[0];
							var entity = world.CreateEntity(world.CreateArchetype(typeof(Rocket), typeof(Position)));
							var spawn = RocketMission.launchLocations[cmd.command.playerId] + new Vector2(10f, -10f);

							world.SetComponentData<Position>(entity, new Position(spawn));
							world.SetComponentData<Rocket>(entity, new Rocket(cmd.command.playerId, cmd.rocketType, cmd.target));
							offset += sizeof(RocketLaunchCommand);
							break;
						}
						case CommandType.SateliteLaunch:
						{
							var cmd = ((SateliteLaunchCommand*)ptr)[0];
							var entity = world.CreateEntity(world.CreateArchetype(typeof(Satelite), typeof(Position)));
							
							world.SetComponentData<Position>(entity, new Position(new Vector2(cmd.target.X, -800f)));
							world.SetComponentData<Satelite>(entity, new Satelite(cmd.target, Time.time + 20f));
							offset += sizeof(SateliteLaunchCommand);
							break;
						}
					}
				}
			}
		}

		private void TransmitNetworkBuffer(object stateInfo)
		{
			var stream = Networking.client.stream;
			var buffer = new byte[NetworkBuffer.Capacity];

			while(this.m_Run)
			{
				while(NetworkBuffer.count == 0)
				{
					if(!this.m_Run)
						return;

					Thread.Sleep(10);
				}

				var bytes = NetworkBuffer.CopyAndClear(buffer);
				stream.Write(buffer, 0, bytes);
			}
		}
	}
	
}
