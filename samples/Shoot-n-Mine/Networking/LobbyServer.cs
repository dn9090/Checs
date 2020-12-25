using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Shoot_n_Mine.Engine;

namespace Shoot_n_Mine.Net
{
	public struct LobbyInfo
	{
		public int clientCount;

		public int clientId;
		
		public bool isReady;
	}

	public class LobbyServer
	{
		private TcpListener m_Listener;

		private int m_Connections;

		private ConcurrentDictionary<int, TcpClient> m_Clients;

		private ConcurrentDictionary<int, NetworkStream> m_Streams;

		private volatile bool m_Run;

		public LobbyServer()
		{
			this.m_Listener = new TcpListener(IPAddress.Any, NetworkInfo.port);
			this.m_Clients = new ConcurrentDictionary<int, TcpClient>();
			this.m_Streams = new ConcurrentDictionary<int, NetworkStream>();
		}

		public void Run()
		{
			this.m_Run = true;
			ThreadPool.QueueUserWorkItem(WaitForClients);
		}

		public void Ready()
		{
			BroadcastLobbyInfo(isReady: true);
		}

		public void Stop()
		{
			this.m_Run = false;
		}

		private void WaitForClients(object stateInfo)
		{
			this.m_Listener.Start();

			while(this.m_Run)
			{
				while(!this.m_Listener.Pending() && this.m_Run)
					Thread.Sleep(10);

				if(!this.m_Run)
					break;
				
				var client = this.m_Listener.AcceptTcpClient();
				client.NoDelay = true;

				int connId = Interlocked.Increment(ref this.m_Connections) - 1;
				this.m_Clients.TryAdd(connId, client);
				this.m_Streams.TryAdd(connId, client.GetStream());

				BroadcastLobbyInfo(isReady: false);

				ThreadPool.QueueUserWorkItem(DistributeClientMessages, connId);

				Console.WriteLine("Connection to " + client.Client.RemoteEndPoint + " (Client) was established.");
			}

			this.m_Listener.Stop();
		}

		private void DistributeClientMessages(object stateInfo)
		{
			int connId = (int)stateInfo;
			this.m_Streams.TryGetValue(connId, out NetworkStream stream);
			var buffer = new byte[1024];

			while(this.m_Run)
			{
				int bytes = stream.Read(buffer);

				for(int i = 0; i < this.m_Clients.Count; ++i)
				{
					if(i == connId)
						continue;

					this.m_Streams.TryGetValue(i, out NetworkStream targetStream);
					targetStream.Write(buffer, 0, bytes);
				}
			}
		}

		private unsafe void BroadcastLobbyInfo(bool isReady)
		{
			var size = sizeof(LobbyInfo);
			var data = stackalloc byte[size];

			for(int i = 0; i < this.m_Clients.Count; ++i)
			{
				var info = new LobbyInfo()
				{
					clientCount = this.m_Clients.Count,
					clientId = i,
					isReady = isReady
				};

				((LobbyInfo*)data)[0] = info;

				this.m_Streams.TryGetValue(i, out NetworkStream targetStream);
				targetStream.Write(new Span<byte>(data, size));
			}
		}
	}
}
