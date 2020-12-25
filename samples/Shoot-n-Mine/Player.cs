using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.Window;
using SFML.Graphics;
using Shoot_n_Mine.Net;

namespace Shoot_n_Mine
{
	public static class Player
	{
		public static int playerId;

		public static bool isHost;

		public static bool isMultiplayer;

		public static void UpdateMultiplayerInfo()
		{
			if(!isMultiplayer)
				return;

			if(isHost)
				playerId = 0;
			else
				playerId = Networking.clientId;
		}
	}
}
