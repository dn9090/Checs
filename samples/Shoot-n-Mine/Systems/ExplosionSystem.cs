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
	public sealed class ExplosionSystem : ComponentSystem
	{
		private const int BufferSize = 16;

		private EntityQuery m_TileQuery;

		private EntityQuery m_ExplosionQuery;

		private Explosion[] m_ExplosionDataBuffer;

		private Vector2[] m_ExplosionPositionBuffer;

		private int m_ExplosionCount;

		public ExplosionSystem()
		{
			this.m_ExplosionDataBuffer = new Explosion[BufferSize];
			this.m_ExplosionPositionBuffer = new Vector2[BufferSize];
		}

		public override void OnEnabled(Scene scene)
		{
			this.m_TileQuery = scene.world.CreateQuery(typeof(TileChunk), typeof(Position));
			this.m_ExplosionQuery = scene.world.CreateQuery(typeof(Explosion), typeof(Position));
		}

		private void WriteExplosionDataToBuffer(World world, ReadOnlySpan<Entity> entities)
		{
			for(int i = 0; i < entities.Length; ++i)
				this.m_ExplosionPositionBuffer[i] = world.GetComponentData<Position>(entities[i]);

			for(int i = 0; i < entities.Length; ++i)
				this.m_ExplosionDataBuffer[i] = world.GetComponentData<Explosion>(entities[i]);
		}

		public override void OnUpdate(Scene scene)
		{
			var explosions = scene.world.GetEntities(this.m_ExplosionQuery);

			if(explosions.Length < 1)
				return;

			explosions = explosions.Slice(0, Math.Min(BufferSize, explosions.Length));
			this.m_ExplosionCount = explosions.Length;

			WriteExplosionDataToBuffer(scene.world, explosions);
			
			scene.world.ForEach(this.m_TileQuery, DetectExplosion);
			scene.world.DestroyEntity(explosions);
		}

		private void DetectExplosion(EntityBatch batch)
		{
			var chunks = batch.GetComponentDataReadWrite<TileChunk>();
			var positions = batch.GetComponentData<Position>();

			for(int i = 0; i < batch.length; ++i)
			{				
				var chunkPos = positions[i];

				if(chunks[i].flags.HasFlag(TileChunkFlags.NoneDestructible))
					continue;

				for(int e = 0; e < this.m_ExplosionCount; ++e)
				{
					var explPos = this.m_ExplosionPositionBuffer[e];
					var explData = this.m_ExplosionDataBuffer[e];

					if(TileChunk.Intersects(chunkPos, explPos, explData.radius))
						DestroyTilesInChunk(ref chunks[i], chunkPos, explPos, explData);
				}
			}
		}

		private void DestroyTilesInChunk(ref TileChunk chunk, Vector2 chunkPos, Vector2 explPos, Explosion explosion)
		{
			int ziklium = 0;
			int derylium = 0;

			for(int i = 0; i < TileChunk.Tiles; ++i)
			{
				var tile = chunk[i];

				if(tile.type == TileType.None || tile.type == TileType.Bedrock)
					continue;

				var targetPos = chunkPos + TileChunk.PositionOffsetOfIndex(i) + Tile.OriginOffset;
				var distSqr = (targetPos - explPos).LengthSquared();

				if(distSqr < explosion.radiusSqr)
				{
					var relDist = 1f - (distSqr / explosion.radiusSqr);
					var health = tile.health - (int)(explosion.impact * relDist) - 1;

					if(health <= 0)
					{
						if(tile.type == TileType.Derylium)
							derylium++;
						else if(tile.type == TileType.Ziklium)
							ziklium++;
						chunk[i] = new Tile(TileType.None);
					} else {
						chunk[i] = new Tile(chunk[i].type, health);
					}
				}
			}

			if(explosion.playerId == Player.playerId)
			{
				RocketResources.ziklium += ziklium * 20;
				RocketResources.derylium += derylium * 20;
			}
		}
	}
}
