using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.Window;
using SFML.System;

namespace Shoot_n_Mine.Engine
{
	public static class Input
	{
		public static Vector2i mousePosition;

		public static bool isFocused;

		private static Window s_Window;

		public static void ReadFrom(Window window)
		{
			s_Window = window;
		}

		public static void Update()
		{
			s_Window.DispatchEvents();
			isFocused = s_Window.HasFocus();
			mousePosition = Mouse.GetPosition(s_Window);
		}
	}
}
