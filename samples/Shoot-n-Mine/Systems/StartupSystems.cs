using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.Graphics;
using SFML.Window;
using Checs;
using Shoot_n_Mine.Engine;

namespace Shoot_n_Mine
{
	public sealed class WaitForInputSystem : ComponentSystem
	{
		public override void OnUpdate(Scene scene)
		{
			if(Input.isFocused && Keyboard.IsKeyPressed(Keyboard.Key.Space))
				scene.manager.Load(MenuScene.Id);
		}
	}

	public sealed class SplashRenderSystem : ComponentSystem
	{
		private Texture m_SplashTexture;

		private Sprite m_SplashSprite;

		private Texture m_SpaceTexture;

		private Sprite m_SpaceSprite;

		public SplashRenderSystem()
		{
			this.m_SplashTexture = new Texture(Assets.NameToPath("splash.png")); 
			this.m_SplashSprite = new Sprite(this.m_SplashTexture);
			this.m_SplashSprite.Origin = new SFML.System.Vector2f(
				this.m_SplashSprite.GetLocalBounds().Width * 0.5f,
				this.m_SplashSprite.GetLocalBounds().Height * 0.5f);

			this.m_SpaceTexture = new Texture(Assets.NameToPath("space.png")); 
			this.m_SpaceSprite = new Sprite(this.m_SpaceTexture);
			this.m_SpaceSprite.Origin = new SFML.System.Vector2f(
				this.m_SpaceSprite.GetLocalBounds().Width * 0.5f,
				this.m_SpaceSprite.GetLocalBounds().Height * 0.5f);
		}

		public override void OnUpdate(Scene scene)
		{
			this.m_SplashSprite.Position = Renderer.view.Center;
			Renderer.target.Draw(this.m_SplashSprite);

			if(((int)Time.time & 2) > 0)
			{
				this.m_SpaceSprite.Position = Renderer.view.Center + new SFML.System.Vector2f(0f, 128f);
				Renderer.target.Draw(this.m_SpaceSprite);
			}
		}
	}
}
