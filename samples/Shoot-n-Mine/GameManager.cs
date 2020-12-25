using System;
using System.Collections.Generic;
using SFML.Window;
using SFML.Graphics;
using Shoot_n_Mine.Engine;

namespace Shoot_n_Mine
{
	public sealed class GameManager
	{
		private SceneManager m_SceneManager;

		public GameManager()
		{
			this.m_SceneManager = new SceneManager();
			this.m_SceneManager.Add(StartupScene.Id, new StartupScene());
			this.m_SceneManager.Add(MenuScene.Id, new MenuScene());
			this.m_SceneManager.Add(PreparingScene.Id, new PreparingScene());
			this.m_SceneManager.Add(GameScene.Id, new GameScene());
		}

		public void Run()
		{
			Application.CreateWindow(800, 600, "Shoot n Mine v1");
			Application.onResize += UpdateView;
			
			Input.ReadFrom(Application.window);
			Renderer.RenderTo(Application.window);
			Time.Initialize();

			this.m_SceneManager.Load(StartupScene.Id);
			
			while(Application.window.IsOpen)
			{
				Input.Update();
				Renderer.Clear();
				Time.UpdateDeltaTime();
				
				Application.window.SetTitle(Time.deltaTime.ToString());
				
				this.m_SceneManager.Update();
				Renderer.Update();

				Application.window.Display();
			}
		}

		private void UpdateView(int width, int height)
		{
			Renderer.ResizeView((float)width, (float)height);
		}
	}
}
