using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.Graphics;
using SFML.Window;
using SFML.System;
using Checs;
using Shoot_n_Mine.Engine;

namespace Shoot_n_Mine
{
	public sealed class UISystem : ComponentSystem
	{
		private List<UICanvas> m_Canvas;

		private bool m_IsMouseDown;

		public UISystem(params UICanvas[] canvas)
		{
			this.m_Canvas = new List<UICanvas>(canvas);
		}

		public override void OnUpdate(Scene scene)
		{
			bool pressed = Input.isFocused && Mouse.IsButtonPressed(Mouse.Button.Left);

			Vector2f position = Renderer.target.MapPixelToCoords(Input.mousePosition);

			if(this.m_IsMouseDown && !pressed)
			{
				foreach(UICanvas canvas in this.m_Canvas)
					canvas.HoverAndInteract(scene, position);
			} else {
				foreach(UICanvas canvas in this.m_Canvas)
					canvas.Hover(position);
			}

			this.m_IsMouseDown = pressed;


			foreach(UICanvas canvas in this.m_Canvas)
				canvas.Render();
		}
	}
}
