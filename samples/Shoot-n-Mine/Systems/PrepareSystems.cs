using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.Graphics;
using SFMLSys = SFML.System;
using Shoot_n_Mine.Engine;
using Shoot_n_Mine.Net;

namespace Shoot_n_Mine
{
	public sealed class WaitForClientsInLobbySystem : ComponentSystem
	{
		private const float LobbyWaitTime = 7f;

		private float m_NextLobbyCloseTime;

		private int m_LastClientCount;

		public override void OnEnabled(Scene scene)
		{
			this.m_NextLobbyCloseTime = Time.time + LobbyWaitTime;
			this.m_LastClientCount = 0;
		}

		public override void OnUpdate(Scene scene)
		{
			var clientCount = Networking.clients;

			if(this.m_LastClientCount < clientCount || clientCount < 2)
			{
				this.m_LastClientCount = clientCount;
				this.m_NextLobbyCloseTime = Time.time + LobbyWaitTime;
			}

			if(Time.time > this.m_NextLobbyCloseTime)
				Networking.server.Ready();
		}
	}

	public sealed class PrepareLobbySystem : ComponentSystem
	{
		public override void OnEnabled(Scene scene)
		{
			if(Player.isMultiplayer)
			{
				if(Player.isHost)
					Networking.HostAndConnect();
				else
					Networking.ConnectTo(NetworkInfo.localhost);
			}
		}

		public override void OnUpdate(Scene scene)
		{
			if(!Player.isMultiplayer)
				scene.manager.Load(GameScene.Id);

			if(Networking.isReady)
				scene.manager.Load(GameScene.Id);
		}
	}

	public sealed class PreparingScreenSystem : ComponentSystem
	{
		private RectangleShape m_Background;

		private Text m_Text;

		private bool m_UpdateStatus;

		public override void OnEnabled(Scene scene)
		{
			this.m_Background = new RectangleShape(Renderer.view.Size * 2f);
			this.m_Background.FillColor = Color.Black;

			string text = "Waiting for other players...";

			this.m_Text = new Text(text, UIAssets.font, 30);
			this.m_Text.FillColor = Color.White;

			this.m_UpdateStatus = !Player.isHost;
		}

		public override void OnUpdate(Scene scene)
		{
			if(Networking.clientId != -1 && this.m_UpdateStatus)
			{
				this.m_Text.DisplayedString =
					"Connected as Player #" +
					Networking.clientId +
					".\nWaiting for other players...";

				this.m_UpdateStatus = false;
			}

			this.m_Background.Position = Renderer.view.Center - new Vector2(
				this.m_Background.Size.X * 0.5f,
				this.m_Background.Size.Y * 0.5f).ToVector2f();
			this.m_Text.Position = Renderer.view.Center - new Vector2(
				this.m_Text.GetLocalBounds().Width * 0.5f,
				this.m_Text.GetLocalBounds().Height * 0.5f).ToVector2f();

			Renderer.target.Draw(this.m_Background);
			Renderer.target.Draw(this.m_Text);
		}
	}
}
