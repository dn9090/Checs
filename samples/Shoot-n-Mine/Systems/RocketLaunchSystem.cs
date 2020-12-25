using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.Window;
using SFML.Graphics;
using Checs;
using Shoot_n_Mine.Engine;
using Shoot_n_Mine.Net;

namespace Shoot_n_Mine
{
	public static class RocketResources
	{
		public static int derylium = 0;

		public static int ziklium = 0;

		public static bool HasEnoughForRocket(RocketType type)
		{
			switch(type)
			{
				case RocketType.Thor4:
					return derylium >= 20 && ziklium >= 10;
				case RocketType.Scout8:
					return derylium >= 30 && ziklium >= 25;
				case RocketType.Northstar10:
					return derylium >= 45 && ziklium >= 30;
			}

			return false;
		}

		public static void InvestInRocket(RocketType type)
		{
			switch(type)
			{
				case RocketType.Thor4:
					derylium -= 20;
					ziklium -= 10;
					break;
				case RocketType.Scout8:
					derylium -= 30;
					ziklium -= 25;
					break;
				case RocketType.Northstar10:
					derylium -= 45;
					ziklium -= 30;
					break;
			}
		}
	}

	public static class RocketMission
	{
		public static RocketType rocket;
	
		public static Vector2 target;

		public static Vector2 targetBounds;

		public static Vector2[] launchLocations;

		public static bool InTargetBounds(float value) => value > targetBounds.X && value < targetBounds.Y;

		public static Vector2 GetTargetCenter() => new Vector2((targetBounds.X + targetBounds.Y) * 0.5f, launchLocations[0].Y);
	}

	public sealed partial class RocketLaunchSystem : ComponentSystem
	{
		private EntityArchetype m_Arch;

		private float m_NextTimeFire;

		private float m_LastTimeChecked;

		public override void OnEnabled(Scene scene)
		{
			this.m_Arch = scene.world.CreateArchetype(typeof(Rocket), typeof(Position));
			this.m_NextTimeFire = Time.time + 2f;
			this.m_LastTimeChecked = Time.time;

			RocketResources.derylium = 40;
			RocketResources.ziklium = 25;
		}

		public override void OnUpdate(Scene scene)
		{
			if(Input.isFocused)
			{
				UpdateRocketSelection();

				if(Mouse.IsButtonPressed(Mouse.Button.Left) && this.m_NextTimeFire < Time.time)
					LaunchRocket(scene.world);
			}

			var timePassed = (int)Math.Round(Time.time - this.m_LastTimeChecked);

			if(timePassed > 1)
			{
				this.m_LastTimeChecked = Time.time;
				RocketResources.derylium += timePassed * 2;
				RocketResources.ziklium += timePassed;
			}
		}

		private void UpdateRocketSelection()
		{
			if(Keyboard.IsKeyPressed(Keyboard.Key.Num1))
				RocketMission.rocket = RocketType.Thor4;
			else if(Keyboard.IsKeyPressed(Keyboard.Key.Num2))
				RocketMission.rocket = RocketType.Scout8;
			else if(Keyboard.IsKeyPressed(Keyboard.Key.Num3))
				RocketMission.rocket = RocketType.Northstar10;
		}

		private void LaunchRocket(World world)
		{
			if(!RocketResources.HasEnoughForRocket(RocketMission.rocket))
				return;

			RocketResources.InvestInRocket(RocketMission.rocket);

			var target = Renderer.target.MapPixelToCoords(Input.mousePosition).ToVector2();

			if(!RocketMission.InTargetBounds(target.X))
				return;

			this.m_NextTimeFire = Time.time + 3f;
			
			var entity = world.CreateEntity(this.m_Arch);
			var spawn = RocketMission.launchLocations[Player.playerId] + new Vector2(10f, -10f);

			world.SetComponentData<Position>(entity, new Position(spawn));
			world.SetComponentData<Rocket>(entity, new Rocket(Player.playerId, RocketMission.rocket, target));

			if(Player.isMultiplayer)
				NetworkBuffer.Add(new RocketLaunchCommand(Player.playerId, RocketMission.rocket, target));
		}
	}
}
