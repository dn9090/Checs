using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Checs;
using Shoot_n_Mine.Engine;

namespace Shoot_n_Mine
{
	public sealed class DebugSystem : ComponentSystem
	{
		private Text m_MousePositionInfo;

		private Text m_WorldInfo;

		private EntityQuery m_TileChunkQuery;

		private RectangleShape m_TileChunkRect;

		public DebugSystem()
		{
			this.m_MousePositionInfo = new Text("...", UIAssets.font, 24);
			this.m_MousePositionInfo.FillColor = Color.Green;
		
			this.m_WorldInfo = new Text("...", UIAssets.font, 20);
			this.m_WorldInfo.FillColor = Color.Green;

			this.m_TileChunkRect = new RectangleShape();
			this.m_TileChunkRect.FillColor = Color.Transparent;
			this.m_TileChunkRect.OutlineColor = Color.Green;
			this.m_TileChunkRect.OutlineThickness = 1f;
			this.m_TileChunkRect.Size = new Vector2f(TileChunk.PixelSize, TileChunk.PixelSize);
		}

		public override void OnEnabled(Scene scene)
		{
			this.m_TileChunkQuery = scene.world.CreateQuery(typeof(TileChunk), typeof(Position));
		}

		public override void OnUpdate(Scene scene)
		{
			Vector2f position = Renderer.target.MapPixelToCoords(Input.mousePosition);
			this.m_MousePositionInfo.Position = position;
			this.m_MousePositionInfo.DisplayedString = "X: " + position.X + " Y: " + position.Y;
			Renderer.target.Draw(this.m_MousePositionInfo);

			FloatRect view = Renderer.MapViewRectToWorld();
			this.m_WorldInfo.Position = new Vector2f(view.Left, view.Top);
			this.m_WorldInfo.DisplayedString = new WorldStatsInfo(scene.world).MakeHumanReadable();
			Renderer.target.Draw(this.m_WorldInfo);

			if(Keyboard.IsKeyPressed(Keyboard.Key.T))
			{
				scene.world.ForEach(this.m_TileChunkQuery, (batch) => {
					var positions = batch.GetComponentData<Position>();
					
					for(int i = 0; i < batch.length; ++i)
					{
						this.m_TileChunkRect.Position = positions[i].value.ToVector2f();
						Renderer.target.Draw(this.m_TileChunkRect);
					}
				});
			}
		}
	}
}
