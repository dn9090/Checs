using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.Graphics;

namespace Shoot_n_Mine.Engine
{
	public static class Renderer
	{
		public static RenderTarget target => s_Target;

		public static View view => s_View;

		public static float distance
		{
			get => s_Distance;
			set
			{
				s_Distance = value;
				ResizeView();
			}
		}

		private static RenderTarget s_Target;

		private static View s_View;

		private static float s_Distance;

		public static void RenderTo(RenderTarget target)
		{
			s_Target = target;
			s_View = target.GetView();
			s_Distance = 0.05f;
			ResizeView(target.Size.X, target.Size.Y);
		}

		public static void Update()
		{
			target.SetView(s_View);
		}

		public static void Clear()
		{
			target.Clear();
		}

		private static void ResizeView()
		{
			ResizeView(s_View.Size.X, s_View.Size.Y);
		}

		public static void ResizeView(float width, float height)
		{
			FloatRect visibleArea = new FloatRect(0f, 0f, width, height);
			s_View = new View(visibleArea);
			s_View.Center = target.GetView().Center;

			ApplyZoom(visibleArea);

			target.SetView(s_View);
			s_View = target.GetView();
		}

		public static Vector2 GetViewSize() => Renderer.view.Size.ToVector2();

		public static FloatRect MapViewRectToWorld()
		{
			return new FloatRect(
				s_View.Center.X - s_View.Size.X * 0.5f,
				s_View.Center.Y - s_View.Size.Y * 0.5f,
				s_View.Size.X,
				s_View.Size.Y);
		}

		private static void ApplyZoom(FloatRect visibleArea)
		{
			float zoom = 1f / Math.Max(visibleArea.Width / 1920, visibleArea.Height / 1080) - s_Distance;
			s_View.Zoom(zoom);
		}
	}
}
