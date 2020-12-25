using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.Graphics;
using SFML.System;
using Checs;
using Shoot_n_Mine.Engine;

namespace Shoot_n_Mine
{
	public sealed class TileRenderSystem : ComponentSystem
	{
		private SpriteMap m_SpriteMap;

		private SpriteGrid m_SpriteGrid;

		private EntityQuery m_Query;

		private FloatRect m_ViewRect;

		public TileRenderSystem()
		{
			this.m_SpriteMap = SpriteMap.Load(Assets.NameToPath("tiles.png"), 24);
			this.m_SpriteGrid = new SpriteGrid(this.m_SpriteMap, TileChunk.Tiles);
		}

		public override void OnEnabled(Scene scene)
		{
			var world = scene.world;

			this.m_Query = scene.world.CreateQuery(typeof(TileChunk), typeof(Position));
		}

		public override void OnUpdate(Scene scene)
		{
			var world = scene.world;

			this.m_ViewRect = Renderer.MapViewRectToWorld();
			world.ForEach(this.m_Query, RenderTiles);
		}

		private void RenderTiles(EntityBatch batch)
		{
			var chunks = batch.GetComponentData<TileChunk>();
			var positions = batch.GetComponentData<Position>();

			for(int i = 0; i < batch.length; ++i)
			{				
				if(TileChunk.Intersects(positions[i], this.m_ViewRect))
					RenderTileChunk(in chunks[i], positions[i]);
			}
		}

		private void RenderTileChunk(in TileChunk chunk, Vector2 pos)
		{
			this.m_SpriteGrid.Clear();
			this.m_SpriteGrid.position = pos;

			if(chunk.flags.HasFlag(TileChunkFlags.StaticNotEmpty))
			{
				for(int i = 0; i < TileChunk.Tiles; ++i)
					this.m_SpriteGrid.Draw((int)chunk[i].type - 1, new Vector2(i % 24, i / 24));
			} else {
				for(int i = 0; i < TileChunk.Tiles; ++i)
				{
					Tile tile = chunk[i];

					if(tile.type == TileType.None || (int)tile.type >= 100)
						continue;

					this.m_SpriteGrid.Draw((int)tile.type - 1, new Vector2(i % 24, i / 24));
				}
			}
			
			this.m_SpriteGrid.RenderTo(Renderer.target);
		}
	}

	public sealed class SkyRenderSystem : ComponentSystem
	{
		private Texture m_SkyTexture;

		private Sprite m_SkySprite;

		private Vector2 m_SkySize;

		public SkyRenderSystem()
		{
			this.m_SkyTexture = new Texture(Assets.NameToPath("sky.png")); 
			this.m_SkySprite = new Sprite(this.m_SkyTexture);
			this.m_SkySize = new Vector2(
				this.m_SkySprite.GetLocalBounds().Width,
				this.m_SkySprite.GetLocalBounds().Height
			);
		}

		public override void OnUpdate(Scene scene)
		{
			var view = Renderer.view;
			float x = view.Size.X + this.m_SkySize.X;
			float y = view.Size.Y + this.m_SkySize.Y;

			Vector2 pos = new Vector2(view.Center.X - (view.Size.X * 0.5f), view.Center.Y - (view.Size.Y * 0.5f));
			Vector2 renderOffset = Vector2.Zero;

			while(renderOffset.Y < y)
			{
				renderOffset.X = 0f;

				while(renderOffset.X < x)
				{
					var renderPos = pos + renderOffset;
					this.m_SkySprite.Position = new SFML.System.Vector2f(renderPos.X, renderPos.Y);
					Renderer.target.Draw(this.m_SkySprite);
					renderOffset.X += this.m_SkySize.X;
				}
				
				renderOffset.Y += this.m_SkySize.Y;
			}
		}
	}

	public sealed class SpawnRenderSystem : ComponentSystem
	{
		private EntityQuery m_Query;

		private Texture m_BaseTexture;

		private Sprite m_BaseSprite;

		private FloatRect m_ViewRect;

		public SpawnRenderSystem()
		{
			this.m_BaseTexture = new Texture(Assets.NameToPath("base.png")); 
			this.m_BaseSprite = new Sprite(this.m_BaseTexture);
			this.m_BaseSprite.Origin = new Vector2f(
				this.m_BaseSprite.GetLocalBounds().Width * 0.5f - Tile.PixelSize * 0.5f,
				this.m_BaseSprite.GetLocalBounds().Height * 0.5f + Tile.PixelSize
			);
		}

		public override void OnEnabled(Scene scene)
		{
			this.m_Query = scene.world.CreateQuery(typeof(Position), typeof(Spawn));
		}

		public override void OnUpdate(Scene scene)
		{
			this.m_ViewRect = Renderer.MapViewRectToWorld();

			var world = scene.world;

			world.ForEach(this.m_Query, RenderSpawns);
		}

		private void RenderSpawns(EntityBatch batch)
		{
			var spawns = batch.GetComponentData<Spawn>();
			var positions = batch.GetComponentData<Position>();

			for(int i = 0; i < batch.length; ++i)
			{
				FloatRect spawnBounds = this.m_BaseSprite.GetLocalBounds();
				FloatRect worldBounds = new FloatRect(positions[i].value.ToVector2f(),
					new Vector2f(spawnBounds.Width, spawnBounds.Height));

				if(this.m_ViewRect.Intersects(worldBounds))
				{
					this.m_BaseSprite.Position = positions[i].value.ToVector2f();
					Renderer.target.Draw(this.m_BaseSprite);
				}
			}
		}
	}

	public sealed class FlightObjectRenderSystem : ComponentSystem
	{
		private EntityQuery m_Query;

		private EntityQuery m_SatQuery;

		private Sprite[] m_Sprites;

		private Sprite m_SatSprite;

		private FloatRect m_ViewRect;

		public FlightObjectRenderSystem()
		{
			var rocket1 = new Texture(Assets.NameToPath("rocket1.png"));
			var rocket2 = new Texture(Assets.NameToPath("rocket2.png"));
			var rocket3 = new Texture(Assets.NameToPath("rocket3.png"));

			this.m_Sprites = new Sprite[] {
				new Sprite(rocket1),
				new Sprite(rocket2),
				new Sprite(rocket3)
			};

			for(int i = 0; i < this.m_Sprites.Length; ++i)
			{
				var sprite = this.m_Sprites[i];
				sprite.Origin = new Vector2f(
					sprite.Origin.X + sprite.GetLocalBounds().Width * 0.5f,
					sprite.Origin.Y + sprite.GetLocalBounds().Height * 0.5f
				);
			}

			this.m_SatSprite = new Sprite(new Texture(Assets.NameToPath("sat.png")));
			this.m_SatSprite.Origin = new Vector2f(
				this.m_SatSprite.GetLocalBounds().Width * 0.5f - Tile.PixelSize * 0.5f,
				this.m_SatSprite.GetLocalBounds().Height * 0.5f + Tile.PixelSize
			);
		}

		public override void OnEnabled(Scene scene)
		{
			this.m_Query = scene.world.CreateQuery(typeof(Rocket), typeof(Position));
			this.m_SatQuery = scene.world.CreateQuery(typeof(Satelite), typeof(Position));
		}

		public override void OnUpdate(Scene scene)
		{
			this.m_ViewRect = Renderer.MapViewRectToWorld();

			var world = scene.world;

			world.ForEach(this.m_Query, RenderRockets);
			world.ForEach(this.m_SatQuery, RenderSats);
		}

		private void RenderRockets(EntityBatch batch)
		{
			var rockets = batch.GetComponentData<Rocket>();
			var positions = batch.GetComponentData<Position>();

			for(int i = 0; i < batch.length; ++i)
			{
				var sprite = this.m_Sprites[(int)rockets[i].type];
				sprite.Position = positions[i].value.ToVector2f();
				sprite.Rotation = rockets[i].angle;
				Renderer.target.Draw(sprite);
			}
		}

		private void RenderSats(EntityBatch batch)
		{
			var sats = batch.GetComponentData<Satelite>();
			var positions = batch.GetComponentData<Position>();

			for(int i = 0; i < batch.length; ++i)
			{
				this.m_SatSprite.Position = positions[i].value.ToVector2f();
				Renderer.target.Draw(this.m_SatSprite);
			}
		}
	}
}
