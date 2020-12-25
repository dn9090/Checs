using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.Graphics;
using SFMLSys = SFML.System;
using Shoot_n_Mine.Engine;
using Shoot_n_Mine.Net;

namespace Shoot_n_Mine
{
	public sealed class PreparingScene : Scene
	{
		public const int Id = 19;

		public override void OnSceneLoad()
		{
			if(Player.isHost)
				base.systems.Add(new WaitForClientsInLobbySystem());

			base.systems.Add(new PrepareLobbySystem());
			base.systems.Add(new PreparingScreenSystem());
		}
	}
}
