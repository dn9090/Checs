using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Checs;
using Shoot_n_Mine.Engine;

namespace Shoot_n_Mine
{
	public static class PlayerAI
	{
		public static int playerId => Player.playerId + 1;
	}

	public sealed class AISystem : ComponentSystem
	{
		private EntityQuery m_Query;

		private EntityArchetype m_Arch;

		private Random m_Random;

		private float m_NextLaunch;

		private bool m_FoundTarget;

		private Vector2 m_TargetPosition;

		public AISystem()
		{
			this.m_Random = new Random((int)Time.time);
		}

		public override void OnEnabled(Scene scene)
		{
			this.m_Query = scene.world.CreateQuery(typeof(TileChunk), typeof(Position));
			this.m_Arch = scene.world.CreateArchetype(typeof(Rocket), typeof(Position));
			this.m_NextLaunch = Time.time + 5f;
		}

		public override void OnUpdate(Scene scene)
		{
			if(Time.time > this.m_NextLaunch)
			{
				this.m_NextLaunch = Time.time + this.m_Random.Next(5, 15) * 0.5f;
				this.m_FoundTarget = false;

				scene.world.ForEach(this.m_Query, FindTargetTile);

				double rand = this.m_Random.NextDouble();
				RocketType type = RocketType.Thor4;

				if(rand < 0.5f)
					type = RocketType.Thor4;
				else if(rand < 0.85f)
					type = RocketType.Scout8;
				else
					type = RocketType.Northstar10;

				var entity = scene.world.CreateEntity(this.m_Arch);
				var spawn = RocketMission.launchLocations[PlayerAI.playerId] + new Vector2(10f, -10f);

				scene.world.SetComponentData<Position>(entity, new Position(spawn));
				scene.world.SetComponentData<Rocket>(entity, new Rocket(PlayerAI.playerId, type, this.m_TargetPosition));
			}
		}

		private void FindTargetTile(EntityBatch batch)
		{
			var chunks = batch.GetComponentData<TileChunk>();
			var positions = batch.GetComponentData<Position>();

			for(int i = 0; i < batch.length; ++i)
			{				
				if(chunks[i].flags.HasFlag(TileChunkFlags.NoneDestructible))
					continue;
				
				FindTargetInChunk(in chunks[i], positions[i]);
			}
		}

		private void FindTargetInChunk(in TileChunk chunk, Vector2 pos)
		{
			for(int i = 0; i < TileChunk.Tiles; ++i)
			{
				Tile tile = chunk[i];

				if(tile.type == TileType.Derylium || tile.type == TileType.Ziklium)
				{
					if(!this.m_FoundTarget || this.m_Random.NextDouble() > 0.8f)
					{
						this.m_TargetPosition = pos + TileChunk.PositionOffsetOfIndex(i);
						this.m_FoundTarget = true;
					}
					break;
				}
			}
		}
	}
}
