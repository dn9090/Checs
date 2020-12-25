using System;
using System.Collections.Generic;
using System.Numerics;
using Checs;
using Shoot_n_Mine.Engine;

namespace Shoot_n_Mine
{
	public sealed class GameScene : Scene
	{
		public const int Id = 20;

		public override void OnSceneLoad()
		{
			Player.UpdateMultiplayerInfo();

			if(Player.isHost)
				GenerateLevelData();
			else
				ReceiveLevelData();
		
			LocatePlayerSpawn(base.world);

			if(Player.isMultiplayer)
				base.systems.Add(new NetworkingSystem());
			else
				base.systems.Add(new AISystem());

			base.systems.Add(new DragSystem());

			if(Player.playerId < 2)
				base.systems.Add(new RocketLaunchSystem());
			else
				base.systems.Add(new SateliteLaunchSystem());

			base.systems.Add(new RocketControlSystem());
			base.systems.Add(new SateliteControlSystem());
			base.systems.Add(new ExplosionSystem());
			base.systems.Add(new SkyRenderSystem());
			base.systems.Add(new FlightObjectRenderSystem());
			base.systems.Add(new TileRenderSystem());
			base.systems.Add(new SpawnRenderSystem());

			if(Player.playerId < 2)
				base.systems.Add(new HudSystem());

			base.systems.Add(new GameTimeSystem());
			//base.systems.Add(new DebugSystem());
		}

		private void GenerateLevelData()
		{
			var generator = new LevelGenerator();

			if(Player.isMultiplayer)
				generator.SendParametersOverNetwork();
				
			generator.Generate(base.world);
		}

		private void ReceiveLevelData()
		{
			var generator = LevelGenerator.ReadFromNetwork();
			generator.Generate(base.world);
		}

		private void LocatePlayerSpawn(World world)
		{
			var spawnQuery = world.CreateQuery(typeof(Spawn), typeof(Position));
			RocketMission.targetBounds = new Vector2(float.MaxValue, float.MinValue);

			world.ForEach(spawnQuery, (batch) => {
				var spawns = batch.GetComponentData<Spawn>();
				var positions = batch.GetComponentData<Position>();

				if(RocketMission.launchLocations == null)
					RocketMission.launchLocations = new Vector2[spawns.Length];
				else
					Array.Resize(ref RocketMission.launchLocations, RocketMission.launchLocations.Length + spawns.Length);

				RocketMission.launchLocations = new Vector2[spawns.Length];

				for(int i = 0; i < batch.length; ++i)
				{
					if(positions[i].value.X < RocketMission.targetBounds.X)
						RocketMission.targetBounds.X = positions[i].value.X;
					
					if(positions[i].value.X > RocketMission.targetBounds.Y)
						RocketMission.targetBounds.Y = positions[i].value.X;

					RocketMission.launchLocations[spawns[i].playerId]  = positions[i].value;
				}
			});

			RocketMission.targetBounds += new Vector2(TileChunk.PixelSize * 0.5f, TileChunk.PixelSize * -0.5f);
		}
	}
}
