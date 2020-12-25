using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.Graphics;
using Checs;
using Shoot_n_Mine.Engine;

namespace Shoot_n_Mine
{
	public sealed class MenuScene : Scene
	{
		public const int Id = 10;

		public override void OnSceneLoad()
		{
			base.systems.Add(new TileRenderSystem());
			base.systems.Add(new UISystem(BuildMainMenu()));
			//base.systems.Add(new DebugSystem());

			var menuTiles = TileMap.Load(Assets.NameToPath("tm_menu.tiles"));

			var archetype = world.CreateArchetype(typeof(TileChunk), typeof(Position));
			var background = world.CreateEntity(archetype, 6);

			var centerPos = new Vector2(TileChunk.PixelSize, 50f);

			SetChunk(background[0], TileType.Dirt, new Vector2(0f, -TileChunk.PixelSize));
			SetChunk(background[1], TileType.Dirt, new Vector2(TileChunk.PixelSize, -TileChunk.PixelSize));
			SetChunk(background[2], TileType.Dirt, new Vector2(TileChunk.PixelSize * 2, -TileChunk.PixelSize));

			menuTiles.Instantiate(world, new Position(new Vector2(0f, 0f)));
			
			SetChunk(background[3], TileType.Stone, new Vector2(0f, TileChunk.PixelSize));
			SetChunk(background[4], TileType.Stone, new Vector2(TileChunk.PixelSize, TileChunk.PixelSize));
			SetChunk(background[5], TileType.Stone, new Vector2(TileChunk.PixelSize * 2, TileChunk.PixelSize));

			Renderer.view.Center = new SFML.System.Vector2f(centerPos.X + TileChunk.PixelSize * 0.5f, centerPos.Y + TileChunk.PixelSize * 0.5f);
		}

		private UICanvas BuildMainMenu()
		{
			UICanvas canvas = new UICanvas();

			canvas.Add(new UIButton("Versus KI", Color.White, Color.Yellow, 32, new Vector2(0f, -120f), true)
				.OnClick((button, scene) => {
					Player.isHost = true;
					Player.isMultiplayer = false;
					scene.manager.Load(PreparingScene.Id);
				}));


			canvas.Add(new UIButton("Host", Color.White, Color.Yellow, 32, new Vector2(0f, -20f), true)
				.OnClick((button, scene) => {
					Player.isHost = true;
					Player.isMultiplayer = true;
					scene.manager.Load(PreparingScene.Id);
				}));

			canvas.Add(new UIButton("Connect", Color.White, Color.Yellow, 32, new Vector2(0f, 80f), true)
				.OnClick((button, scene) => {
					Player.isHost = false;
					Player.isMultiplayer = true;
					scene.manager.Load(PreparingScene.Id);
				}));

			canvas.Add(new UIButton("Exit", Color.White, Color.Yellow, 32, new Vector2(0f, 180), true)
				.OnClick((button, scene) => Application.Close()));

			return canvas;
		}

		private UICanvas BuildNetworkMenu()
		{
			return null;
		}

		private void SetChunk(Entity entity, TileType tileType, Vector2 position)
		{
			world.SetComponentData<Position>(entity, new Position(position));
			ref var chunk = ref world.RefComponentData<TileChunk>(entity);
			chunk.SetAll(new Tile(tileType));
		}
	}
}
