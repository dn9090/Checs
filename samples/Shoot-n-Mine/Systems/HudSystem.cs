using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Shoot_n_Mine.Engine;

namespace Shoot_n_Mine
{
	public sealed class Crosshair
	{
		private const float Length = 100f;

		private const float Thickness = 4f;

		private RectangleShape m_Vertical;

		private RectangleShape m_Horizontal;

		public Crosshair()
		{
			this.m_Vertical = new RectangleShape();
			this.m_Horizontal = new RectangleShape();
			this.m_Vertical.Size = new Vector2f(Thickness, Length);
			this.m_Horizontal.Size = new Vector2f(Length, Thickness);
			this.m_Vertical.FillColor = Color.Yellow;
			this.m_Horizontal.FillColor = Color.Yellow;
		}

		public void SetColor(Color color)
		{
			this.m_Vertical.FillColor = color;
			this.m_Horizontal.FillColor = color;
		}

		public void Render(Vector2f position)
		{
			this.m_Vertical.Position = position + new Vector2f(0f, 50f);
			Renderer.target.Draw(this.m_Vertical);

			this.m_Vertical.Position = position - new Vector2f(0f, 50f + Length);
			Renderer.target.Draw(this.m_Vertical);
		
			this.m_Horizontal.Position = position + new Vector2f(50f, 0f);
			Renderer.target.Draw(this.m_Horizontal);

			this.m_Horizontal.Position = position - new Vector2f(50f + Length, 0f);
			Renderer.target.Draw(this.m_Horizontal);
		}
	}


	public sealed class HudSystem : ComponentSystem
	{
		private const float IconPixelSize = 64f;

		private Sprite[] m_Rockets;

		private Sprite m_SelectionHighlight;

		private Crosshair m_Crosshair;

		private Text m_ResourcesDisplay;

		public override void OnEnabled(Scene scene)
		{
			this.m_Rockets = new Sprite[] {
				new Sprite(new Texture(Assets.NameToPath("rocket1_icon.png"))),
				new Sprite(new Texture(Assets.NameToPath("rocket2_icon.png"))),
				new Sprite(new Texture(Assets.NameToPath("rocket3_icon.png")))
			};

			this.m_SelectionHighlight = new Sprite(new Texture(Assets.NameToPath("rocket_sel.png")));
			this.m_Crosshair = new Crosshair();
			this.m_ResourcesDisplay = new Text("", UIAssets.font, 24);
			this.m_ResourcesDisplay.FillColor = Color.Yellow;
		}

		public override void OnUpdate(Scene scene)
		{
			RenderCrosshair();
			RenderHud();
			RenderInfo();
		}

		private void RenderInfo()
		{
			this.m_ResourcesDisplay.DisplayedString =
				"Derylium 95: " + RocketResources.derylium +
				"\nZiklium 140: " + RocketResources.ziklium;

			this.m_ResourcesDisplay.Position = new Vector2f(
				Renderer.view.Center.X - Renderer.view.Size.X * 0.5f + 100f,
				Renderer.view.Center.Y - Renderer.view.Size.Y * 0.5f + 100);

			Renderer.target.Draw(this.m_ResourcesDisplay);
		}

		private void RenderCrosshair()
		{
			Vector2f position = Renderer.target.MapPixelToCoords(Input.mousePosition);

			if(RocketMission.InTargetBounds(position.X) && RocketResources.HasEnoughForRocket(RocketMission.rocket))
				this.m_Crosshair.SetColor(Color.Yellow);
			else
				this.m_Crosshair.SetColor(Color.Red);

			this.m_Crosshair.Render(position);
		}		

		private void RenderHud()
		{
			var anchorPos = Renderer.view.Center + new Vector2f(-Renderer.view.Size.X * 0.5f + 64f, Renderer.view.Size.Y * 0.5f - 128f - IconPixelSize);
			var currentPos = anchorPos;

			for(int i = 0; i < this.m_Rockets.Length; ++i)
			{
				if((int)RocketMission.rocket == i)
				{
					this.m_SelectionHighlight.Position = currentPos;
					Renderer.target.Draw(this.m_SelectionHighlight);
				}

				this.m_Rockets[i].Position = currentPos;
				Renderer.target.Draw(this.m_Rockets[i]);
				currentPos += new Vector2f(64f + IconPixelSize, 0f);
			}
		}
	}
}
