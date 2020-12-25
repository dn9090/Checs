using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using SFML.Graphics;
using SFML.System;
using Checs;
using Shoot_n_Mine.Engine;

namespace Shoot_n_Mine
{
	public static class UIAssets
	{
		public static Font font = new Font(Assets.NameToPath("Orbitron-Regular.ttf"));
	}

	public interface IUIElement
	{
		void Draw(FloatRect view, Vector2f center);
	}

	public interface IUIInteractable : IUIElement
	{
		bool Touches(Vector2f position);

		void SetHover(bool state);

		void Interact(Scene scene);
	}

	public class UIText : IUIElement
	{
		public string displayText
		{
			get => text.DisplayedString;
			set => text.DisplayedString = value;
		}

		protected Text text;

		private Vector2 m_Position;

		private bool m_Centered;

		public UIText(string text, Color color, int size, Vector2 position, bool centered = false)
		{
			this.text = new Text(text, UIAssets.font, (uint)size);
			this.text.FillColor = color;
			this.text.Origin += new Vector2f(this.text.GetLocalBounds().Width * 0.5f, this.text.GetLocalBounds().Height * 0.5f);
			this.m_Position = position;
			this.m_Centered = centered;
		}

		public UIText(string text, Vector2 position) : this(text, Color.White, 16, position)
		{
		}

		public void Draw(FloatRect view, Vector2f center)
		{
			if(this.m_Centered)
				this.text.Position = new Vector2f(center.X + this.m_Position.X, center.Y + this.m_Position.Y);
			else
				this.text.Position = new Vector2f(view.Left + this.m_Position.X, view.Top + this.m_Position.Y);

			Renderer.target.Draw(this.text);
		}
	}

	public class UIButton : UIText, IUIInteractable
	{
		public event Action<UIButton, Scene> onClick;

		private Color m_Highlight;

		private Color m_Base;

		public UIButton(string text, Color color, Color highlight, int size, Vector2 position, bool centered = false)
			: base(text, color, size, position, centered)
		{
			this.m_Highlight = highlight;
			this.m_Base = color;
		}

		public UIButton(string text, Vector2 position) : base(text, position)
		{
		}

		public UIButton OnClick(Action<UIButton, Scene> action)
		{
			this.onClick = action;
			return this;
		}

		public bool Touches(Vector2f position) =>
			base.text.GetGlobalBounds().Contains(position.X, position.Y);

		public void SetHover(bool state)
		{
			if(state)
				base.text.FillColor = this.m_Highlight;
			else
				base.text.FillColor = this.m_Base;
		}

		public void Interact(Scene scene)
		{
			onClick?.Invoke(this, scene);
		}
	}

	public class UICanvas
	{
		public int elements => this.m_Elements.Count;

		private List<IUIElement> m_Elements;

		private List<IUIInteractable> m_Interactables;

		public UICanvas()
		{
			this.m_Elements = new List<IUIElement>();
			this.m_Interactables = new List<IUIInteractable>();
		}

		public void Add(IUIElement element)
		{
			this.m_Elements.Add(element);
		}

		public void Add(IUIInteractable interactable)
		{
			this.m_Elements.Add(interactable);
			this.m_Interactables.Add(interactable);
		}

		public void Render()
		{
			Vector2f center = Renderer.view.Center;
			FloatRect view = Renderer.MapViewRectToWorld();

			foreach(IUIElement element in this.m_Elements)
				element.Draw(view, center);
		}

		public void Hover(Vector2f position)
		{
			foreach(IUIInteractable interactable in this.m_Interactables)
				interactable.SetHover(interactable.Touches(position));
		}

		public void HoverAndInteract(Scene scene, Vector2f position)
		{
			foreach(IUIInteractable interactable in this.m_Interactables)
			{
				if(interactable.Touches(position))
				{
					interactable.SetHover(true);
					interactable.Interact(scene);
				} else {
					interactable.SetHover(false);
				}
			}
		}
	}
}
