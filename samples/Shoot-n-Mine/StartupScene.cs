using System;
using System.Collections.Generic;
using System.Numerics;
using Shoot_n_Mine.Engine;

namespace Shoot_n_Mine
{
	public sealed class StartupScene : Scene
	{
		public const int Id = 0;

		public override void OnSceneLoad()
		{
			base.systems.Add(new WaitForInputSystem());
			base.systems.Add(new SkyRenderSystem());
			base.systems.Add(new TileRenderSystem());
			base.systems.Add(new SplashRenderSystem());
			//base.systems.Add(new DebugSystem());

			var archetype = world.CreateArchetype(typeof(TileChunk), typeof(Position));

			var grassLayer = world.CreateEntity(archetype, 4);
			Vector2 position = new Vector2(0f, -100f);

			for(int i = 0; i < grassLayer.Length; ++i)
			{
				world.SetComponentData<Position>(grassLayer[i], new Position(position));
				position.X += TileChunk.PixelSize;

				ref var chunk = ref world.RefComponentData<TileChunk>(grassLayer[i]);

				for(int t = 0; t < TileChunk.Size; ++t)
					chunk[t] = new Tile(TileType.Grass, 1);
				for(int t = TileChunk.Size; t < TileChunk.Tiles; ++t)
				{
					if(t % 17 == 0 || t % 43 == 0)
					{
						chunk[t] = new Tile(TileType.Stone, 1);
						continue;
					}

					chunk[t] = new Tile(TileType.Dirt, 1);
				}
			}
			
			var stoneLayer = world.CreateEntity(archetype, 4);
			position = new Vector2(0f, position.Y + TileChunk.PixelSize);
			
			for(int i = 0; i < stoneLayer.Length; ++i)
			{
				world.SetComponentData<Position>(stoneLayer[i], new Position(position));
				position.X += TileChunk.PixelSize;

				ref var chunk = ref world.RefComponentData<TileChunk>(stoneLayer[i]);

				for(int t = 0; t < TileChunk.Tiles; ++t)
					chunk[t] = new Tile(TileType.Stone, 1);
			}

			Renderer.view.Center = new SFML.System.Vector2f(TileChunk.PixelSize * 2f, 0f);
		}
	}
}
