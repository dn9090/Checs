using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.Graphics;

namespace Shoot_n_Mine.Engine
{
	public struct SpriteMap
	{
		public int tileSize;

		public Texture texture;

		public Sprite[] sprites;

		public static SpriteMap Load(string filePath, int tileSize)
		{
			SpriteMap map = new SpriteMap();
			map.tileSize = tileSize;
			map.texture = new Texture(filePath);
			
			int x = (int)map.texture.Size.X / tileSize;
			int y = (int)map.texture.Size.Y / tileSize;

			map.sprites = new Sprite[x * y];

			for(int row = 0; row < y; ++row)
			{
				for(int col = 0; col < x; ++col)
				{
					IntRect rect = new IntRect( 
						tileSize * col,
						tileSize * row,
						tileSize,
						tileSize);
					map.sprites[row * x + col] = new Sprite(map.texture, rect);
				}
			}

			return map;
		}
	}

}
