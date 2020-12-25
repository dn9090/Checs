using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.Graphics;
using SFMLSys = SFML.System;
using SFML.Window;
using Checs;
using Shoot_n_Mine.Engine;
using Shoot_n_Mine.Net;

namespace Shoot_n_Mine
{
	public sealed class SateliteLaunchSystem : ComponentSystem
	{
		private EntityArchetype m_Arch;

		private float m_NextTimeFire;

		public SateliteLaunchSystem()
		{
		}

		public override void OnEnabled(Scene scene)
		{
			this.m_Arch = scene.world.CreateArchetype(typeof(Satelite), typeof(Position));
			this.m_NextTimeFire = Time.time + 2f;
		}

		public override void OnUpdate(Scene scene)
		{
			if(Mouse.IsButtonPressed(Mouse.Button.Left) && this.m_NextTimeFire < Time.time)
				LaunchSatelite(scene.world);
		}

		private void LaunchSatelite(World world)
		{
			this.m_NextTimeFire = Time.time + 10f;

			var target = Renderer.target.MapPixelToCoords(Input.mousePosition).ToVector2();
			target = new Vector2(target.X, Math.Clamp(target.Y, -250f, -160f));

			var entity = world.CreateEntity(this.m_Arch);

			world.SetComponentData<Satelite>(entity, new Satelite(target, Time.time + 20f));
			world.SetComponentData<Position>(entity, new Position(new Vector2(target.X, -800f)));

			if(Player.isMultiplayer)
				NetworkBuffer.Add(new SateliteLaunchCommand(Player.playerId, target));
		}
	}
}
