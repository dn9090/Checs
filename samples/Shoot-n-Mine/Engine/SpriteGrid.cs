using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.Graphics;
using SFML.System;

namespace Shoot_n_Mine.Engine
{
	public struct SpriteGrid
	{
		public Vector2 position;

		public Vertex[] vertices;

		public Sprite[] sprites;

		private int m_Index;

		private int m_Size;

		private RenderStates m_RenderStates;

		public SpriteGrid(SpriteMap map, int capacity)
		{
			this.position = Vector2.Zero;
			this.sprites = map.sprites;
			this.vertices = new Vertex[capacity * 4];
			this.m_Index = 0;
			this.m_Size = map.tileSize;
			this.m_RenderStates = new RenderStates(map.texture);
		}

		public void Draw(int index, Vector2 offset)
		{
			var sprite = this.sprites[index];
			sprite.Position = new Vector2f(
				this.position.X + offset.X * this.m_Size,
				this.position.Y + offset.Y * this.m_Size);
			Draw(sprite);
		}

		public void RenderTo(RenderTarget renderTarget)
		{
			renderTarget.Draw(vertices, PrimitiveType.Quads, this.m_RenderStates);
		}
		
		public void Clear()
		{
			this.m_Index = 0;
		}

		private unsafe void Draw(Sprite sprite)
		{
			var pX = -sprite.Origin.X * sprite.Scale.X;
			var pY = -sprite.Origin.Y * sprite.Scale.Y;

			Vector2 scale = new Vector2(
				this.m_Size  * sprite.Scale.X,
				this.m_Size * sprite.Scale.Y);

			Vector2 position = new Vector2(sprite.Position.X, sprite.Position.Y);
			
			float right = sprite.TextureRect.Left + this.m_Size;
			float bottom = sprite.TextureRect.Top + this.m_Size;

			fixed(Vertex* vPtr = this.vertices)
			{
				var ptr = vPtr + this.m_Index;

				ptr->Position.X = pX + position.X;
				ptr->Position.Y = pY + position.Y;
				ptr->TexCoords.X = sprite.TextureRect.Left;
				ptr->TexCoords.Y = sprite.TextureRect.Top;
				ptr->Color = sprite.Color;
				ptr++;

				pX += scale.X;
				ptr->Position.X = pX + position.X;
				ptr->Position.Y = pY + position.Y;
				ptr->TexCoords.X = right;
				ptr->TexCoords.Y = sprite.TextureRect.Top;
				ptr->Color = sprite.Color;
				ptr++;

				pY += scale.Y;
				ptr->Position.X = pX + position.X;
				ptr->Position.Y = pY + position.Y;
				ptr->TexCoords.X = right;
				ptr->TexCoords.Y = bottom;
				ptr->Color = sprite.Color;
				ptr++;

				pX -= scale.X;
				ptr->Position.X = pX + position.X;
				ptr->Position.Y = pY + position.Y;
				ptr->TexCoords.X = sprite.TextureRect.Left;
				ptr->TexCoords.Y = bottom;
				ptr->Color = sprite.Color;
			}

			this.m_Index += 4;
		}
	}

}
