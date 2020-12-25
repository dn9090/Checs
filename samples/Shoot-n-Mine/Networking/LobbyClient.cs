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
	public class LobbyClient
	{
		public NetworkStream stream;

		private TcpClient m_Client;

		private IPEndPoint m_EndPoint;

		public LobbyClient()
		{
			this.m_Client = new TcpClient();
			this.m_Client.NoDelay = true;
		}

		public void Connect(string address)
		{
			this.m_EndPoint = new IPEndPoint(IPAddress.Parse(address), NetworkInfo.port);
			ThreadPool.QueueUserWorkItem(ConnectAndUpdateInfo);
		}

		private void ConnectAndUpdateInfo(object stateInfo)
		{
			this.m_Client.Connect(this.m_EndPoint);
			this.stream = this.m_Client.GetStream();

			Networking.isConnected = true;

			Console.WriteLine("Connection to " + this.m_Client.Client.RemoteEndPoint + " (Server) was established.");

			while(!Networking.isReady)
				ReceiveLobbyInfo();
		}

		private unsafe void ReceiveLobbyInfo()
		{
			var size = sizeof(LobbyInfo);
			var data = stackalloc byte[size];
			
			this.stream.Read(new Span<byte>(data, size));
			var info = ((LobbyInfo*)data)[0];
			
			Networking.isReady = info.isReady;
			Networking.clientId = info.clientId;
			Networking.clients = info.clientCount;
		}
	}
}
