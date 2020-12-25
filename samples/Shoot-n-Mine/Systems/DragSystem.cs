using System;
using System.Collections.Generic;
using System.Numerics;
using SFMLSys = SFML.System;
using SFML.Window;
using Checs;
using Shoot_n_Mine.Engine;

namespace Shoot_n_Mine
{
	public sealed class DragSystem : ComponentSystem
	{
		private const float Speed = 10f;

		private Vector2 m_MinPos;

		private Vector2 m_MaxPos;

		private Vector2 m_DragOrigin;

		private bool m_Dragging;

		public override void OnEnabled(Scene scene)
		{
			this.m_MinPos = new Vector2(float.MaxValue, float.MaxValue);
			this.m_MaxPos = new Vector2(float.MinValue, float.MinValue);

			var tileQuery = scene.world.CreateQuery(typeof(TileChunk), typeof(Position));

			scene.world.ForEach(tileQuery, (batch) => {
				var positions = batch.GetComponentData<Position>();

				for(int i = 0; i < batch.length; ++i)
				{
					if(positions[i].value.X < this.m_MinPos.X)
						this.m_MinPos.X = positions[i].value.X;

					if(positions[i].value.X > this.m_MaxPos.X)
						this.m_MaxPos.X = positions[i].value.X;

					if(positions[i].value.Y < this.m_MinPos.Y)
						this.m_MinPos.Y = positions[i].value.Y;

					if(positions[i].value.Y > this.m_MaxPos.Y)
						this.m_MaxPos.Y = positions[i].value.Y;
				}
			});

			this.m_MaxPos += new Vector2(TileChunk.PixelSize, TileChunk.PixelSize);
			this.m_MinPos.Y -= TileChunk.PixelSize;
		
			SetViewCenterToSpawn();
		}

		public override void OnUpdate(Scene scene)
		{
			if(this.m_Dragging && !Mouse.IsButtonPressed(Mouse.Button.Right))
				this.m_Dragging = false;

			if(Input.isFocused && Mouse.IsButtonPressed(Mouse.Button.Right))
			{
				if(this.m_Dragging)
				{
					var pos = Renderer.target.MapPixelToCoords(Input.mousePosition).ToVector2();
					var delta = pos - this.m_DragOrigin;
					var center = Renderer.view.Center.ToVector2() + (delta * Speed * Time.deltaTime * -1);
					var extends = Renderer.GetViewSize() * 0.5f;
					
					center.X = Math.Clamp(center.X, this.m_MinPos.X + extends.X, this.m_MaxPos.X - extends.X);
					center.Y = Math.Clamp(center.Y, this.m_MinPos.Y + extends.Y, this.m_MaxPos.Y - extends.Y);

					Renderer.view.Center = center.ToVector2f();
					Renderer.Update();
				} else {
					this.m_DragOrigin = Renderer.target.MapPixelToCoords(Input.mousePosition).ToVector2();
					this.m_Dragging = true;
				}
			}
		}
		
		private void SetViewCenterToSpawn()
		{
			var position = RocketMission.GetTargetCenter();

			if(Player.playerId < RocketMission.launchLocations.Length)
				position = RocketMission.launchLocations[Player.playerId];

			var extends = Renderer.GetViewSize() * 0.5f;
			var viewPos =  new Vector2(Math.Clamp(position.X, this.m_MinPos.X + extends.X, this.m_MaxPos.X - extends.X), position.Y + 30f);
			Renderer.view.Center = viewPos.ToVector2f();
		}
	}
}
