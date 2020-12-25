using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Shoot_n_Mine.Engine;

namespace Shoot_n_Mine
{
	public sealed class GameTimeSystem : ComponentSystem
	{
		private Text m_TimeDisplay;

		private float m_FinalTime;

		public override void OnEnabled(Scene scene)
		{
			this.m_TimeDisplay = new Text("00:00", UIAssets.font, 24);
			this.m_TimeDisplay.FillColor = Color.White;
			this.m_FinalTime = Shoot_n_Mine.Engine.Time.time + 600f;
		}

		public override void OnUpdate(Scene scene)
		{
			var timeLeft = (this.m_FinalTime - Shoot_n_Mine.Engine.Time.time) / 60f;
			this.m_TimeDisplay.DisplayedString = timeLeft.ToString("00.00");
			this.m_TimeDisplay.Position = new Vector2f(
				Renderer.view.Center.X - 20f,
				Renderer.view.Center.Y - Renderer.view.Size.Y * 0.5f + 100f);

			Renderer.target.Draw(this.m_TimeDisplay);
		}
	}
}
