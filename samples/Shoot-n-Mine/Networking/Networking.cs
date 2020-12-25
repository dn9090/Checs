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
	public static class Networking
	{
		public static volatile bool isConnected = false;

		public static volatile bool isReady = false;

		public static volatile int clientId = -1;

		public static volatile int clients = 0;

		public static LobbyServer server;

		public static LobbyClient client;

		public static void HostAndConnect()
		{
			server = new LobbyServer();
			server.Run();

			ConnectTo(NetworkInfo.localhost);
		}

		public static void ConnectTo(string address)
		{
			client = new LobbyClient();
			client.Connect(address);
		}
	}
}
