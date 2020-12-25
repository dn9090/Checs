using System;
using System.Collections.Generic;
using SFML.Window;
using SFML.Graphics;

namespace Shoot_n_Mine.Engine
{
	public static class Application
	{
		public static RenderWindow window;

		public static event Action onQuit;

		public static event Action<int, int> onResize;

		public static void CreateWindow(int width, int height, string name)
		{
			if(window != null)
				window.Close();

			window = new RenderWindow(new VideoMode((uint)width, (uint)height), name);
			window.Closed += OnClose;
			window.Resized += OnResize;
		}

		public static void Close()
		{
			onQuit?.Invoke();
			window.Close();
		}

		private static void OnResize(object target, SizeEventArgs args)
		{
			onResize?.Invoke((int)args.Width, (int)args.Height);
		}

		private static void OnClose(object target, EventArgs args)
		{
			Close();
		}
	}
}
