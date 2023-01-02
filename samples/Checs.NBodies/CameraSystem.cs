using System;
using System.Numerics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Checs.NBodies
{
	public class CameraSystem : System
	{
		public const float DragSpeed = 10f;

		public RenderWindow window;

		public View view;

		public Vector2f viewSize;

		public float zoom;

		public Vector2f dragOrigin;

		public bool isDragging;

		public CameraSystem(RenderWindow window, View view)
		{
			this.window = window;
			this.window.MouseWheelScrolled += OnMouseScroll;
			this.zoom = 1f;
			this.view = view;
			this.viewSize = this.view.Size;
			this.view.Zoom(1f);
		}

		public void OnMouseScroll(object sender, EventArgs e)
		{
			var mouseEvent = (MouseWheelScrollEventArgs)e;
			this.zoom -= 0.05f * mouseEvent.Delta;
			this.view.Size = this.viewSize * this.zoom;
			this.window.SetView(this.view);
		}
	
		public override void Run(float deltaTime)
		{
			if(this.isDragging && !Mouse.IsButtonPressed(Mouse.Button.Right))
				this.isDragging = false;

			if(this.window.HasFocus() && Mouse.IsButtonPressed(Mouse.Button.Right))
			{
				var mousePosition = Mouse.GetPosition(this.window);

				if(this.isDragging)
				{	
					var dragPosition = this.window.MapPixelToCoords(mousePosition);
					var delta = dragPosition - this.dragOrigin;
					var center = this.view.Center + (delta * DragSpeed * deltaTime * -1f);
					
					this.view.Center = new Vector2f(center.X, center.Y);
					this.window.SetView(this.view);
				} else {
					this.dragOrigin = this.window.MapPixelToCoords(mousePosition);
					this.isDragging = true;
				}
			}
		}
	}
}