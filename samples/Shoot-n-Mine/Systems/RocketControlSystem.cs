using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.Window;
using SFML.Graphics;
using Checs;
using Shoot_n_Mine.Engine;

namespace Shoot_n_Mine
{
	public sealed partial class RocketControlSystem : ComponentSystem
	{
		private const float Gravity = 9.81f;

		private const float ThrusterAcc = 70.0f;

		private const float LockDistance = 250f;

		private const float LockDistanceSqr = LockDistance * LockDistance;

		private const float TurnSpeed = 375.0f * (float)Math.PI / 180.0f;
		
		private const float RocketCollisionRadius = 24f;

		private const float RocketCollisionRadiusSqr = RocketCollisionRadius * RocketCollisionRadius;

		private const float CollisionBorderOverlap = 5f;

		private EntityArchetype m_Explosion;

		private EntityQuery m_RocketQuery;

		private EntityQuery m_TileQuery;

		private float m_FixedTimeAcc;

		public override void OnEnabled(Scene scene)
		{
			this.m_Explosion = scene.world.CreateArchetype(typeof(Explosion), typeof(Position));
			this.m_RocketQuery = scene.world.CreateQuery(typeof(Rocket), typeof(Position));
			this.m_TileQuery = scene.world.CreateQuery(typeof(TileChunk), typeof(Position));
			this.m_FixedTimeAcc = 0f;
		}

		public override void OnUpdate(Scene scene)
		{
			FixedUpdate(scene.world);
		}

		private void FixedUpdate(World world)
		{
			var rockets = world.GetEntities(this.m_RocketQuery);
			var chunks = world.GetEntities(this.m_TileQuery);

			this.m_FixedTimeAcc += Time.deltaTime;

			while(this.m_FixedTimeAcc >= Time.fixedDeltaTime)
			{
				for(int i = 0; i < rockets.Length; ++i)
				{
					ref var rocket = ref world.RefComponentData<Rocket>(rockets[i]);

					if(rocket.impact)
						continue;

					ref var position = ref world.RefComponentData<Position>(rockets[i]);

					UpdateFlightRoute(ref rocket, ref position);

					if((int)rocket.state > (int)RocketState.Launch)
						CheckRocketToRocketCollision(world, rockets, rockets[i], ref rocket, position.value);

					if((int)rocket.state > (int)RocketState.Cruise)
						CheckRocketToTileCollision(world, chunks, ref rocket, position.value);
				}

				this.m_FixedTimeAcc -= Time.fixedDeltaTime;
			}

			for(int i = 0; i < rockets.Length; ++i)
			{
				ref var rocket = ref world.RefComponentData<Rocket>(rockets[i]);

				if(!rocket.impact)
					continue;

				var position = world.GetComponentData<Position>(rockets[i]);

				var expl = world.CreateEntity(this.m_Explosion);
				world.SetComponentData<Position>(expl, position);

				switch(rocket.type)
				{
					case RocketType.Thor4:
						world.SetComponentData<Explosion>(expl, Explosion.Create(rocket.playerId, ExplosionType.T10));
						break;
					case RocketType.Scout8:
						world.SetComponentData<Explosion>(expl, Explosion.Create(rocket.playerId, ExplosionType.KL50));
						break;
					case RocketType.Northstar10:
						world.SetComponentData<Explosion>(expl, Explosion.Create(rocket.playerId, ExplosionType.IU7));
						break;
				}				
				
				world.DestroyEntity(rockets[i]);
			}
		}

		private void CheckRocketToRocketCollision(World world, ReadOnlySpan<Entity> rockets, Entity entity, ref Rocket rocket, Vector2 position)
		{
			for(int i = 0; i < rockets.Length; ++i)
			{
				if(rockets[i] == entity)
					continue;
				
				var testPos = world.GetComponentData<Position>(rockets[i]).value - position;

				if(testPos.LengthSquared() < RocketCollisionRadiusSqr)
				{
					ref var other = ref world.RefComponentData<Rocket>(rockets[i]);
					other.impact = true;
					rocket.impact = true;
				}
					
			}
		}

		private void CheckRocketToTileCollision(World world, ReadOnlySpan<Entity> chunks, ref Rocket rocket, Vector2 position)
		{
			var chunkRect = new FloatRect(0f, 0f, TileChunk.PixelSize + CollisionBorderOverlap, TileChunk.PixelSize + CollisionBorderOverlap);
			var tileRect = new FloatRect(0f, 0f, Tile.PixelSize + CollisionBorderOverlap, Tile.PixelSize + CollisionBorderOverlap);

			for(int i = 0; i < chunks.Length; ++i)
			{
				var chunkPos = world.GetComponentData<Position>(chunks[i]).value;

				chunkRect.Left = chunkPos.X;
				chunkRect.Top = chunkPos.Y;

				if(!chunkRect.Contains(position.X, position.Y))
					continue;
				
				var chunk = world.RefComponentData<TileChunk>(chunks[i]);

				for(int t = 0; t < TileChunk.Tiles; ++t)
				{
					if(chunk[t].type == TileType.None)
						continue;

					var targetPos = chunkPos + TileChunk.PositionOffsetOfIndex(t);
					tileRect.Left = targetPos.X;
					tileRect.Top = targetPos.Y;

					if(tileRect.Contains(position.X, position.Y))
					{
						rocket.impact = true;
						return;
					}
				}
			}
		}

		private void UpdateFlightRoute(ref Rocket rocket, ref Position position)
		{
			if(rocket.state == RocketState.Idle)
			{
				if(rocket.target.Y < rocket.cruiseAltitude)
					rocket.target.Y = rocket.cruiseAltitude;
				else if(rocket.target.Y < 0f)
					rocket.cruiseAltitude = rocket.target.Y;

				rocket.velocity = Vector2.Zero;
				rocket.acc = Vector2.Zero;
				rocket.state = RocketState.Launch;
				return;
			}

			// Newton / D'Alembert Markov Chain.

			Vector2 heading = Vector2.Zero;
			
			if(rocket.state == RocketState.Launch)
			{
				heading = new Vector2(0f, 1f);
				if(position.value.Y < rocket.cruiseAltitude)
					rocket.state = RocketState.Cruise;
			}

			var v = rocket.velocity.Length();

			if(rocket.state != RocketState.Launch && rocket.state != RocketState.Impact)
			{
				heading = Vector2.Normalize(rocket.velocity);
				var dir = rocket.target - position.value;

				if(rocket.state == RocketState.Cruise)
				{
					dir.Y = 0;

					if((dir.X * dir.X) < LockDistanceSqr)
						rocket.state = RocketState.Target;
				} else {
					if((dir.X * dir.X) < 25f)
						rocket.state = RocketState.Impact;
				}

				dir = Vector2.Normalize(dir);
				var dot = Vector2.Dot(heading, dir);
				var turn = (float)Math.Abs(Math.Acos(Math.Clamp(dot, -0.99999f, 0.99999f)));

				if(turn > float.MinValue)
					turn = Time.fixedDeltaTime * TurnSpeed * 0.5f / turn;
				else
					turn = 0f;
				
				heading += turn * (dir - heading);
				heading = Vector2.Normalize(heading);

				rocket.velocity = v * heading;
			}
			
			rocket.acc = -rocket.airFrictionCoeff * v * rocket.velocity;
			rocket.acc += heading * ThrusterAcc;
			rocket.acc += new Vector2(0f, Gravity);

			// Simulation step.
			rocket.velocity += rocket.acc * Time.fixedDeltaTime * new Vector2(0f, -1f);
			position.value += rocket.velocity * Time.fixedDeltaTime;

			if((int)rocket.state < (int)RocketState.Target)
				rocket.angle = -rocket.velocity.AngleBetween(new Vector2(1f, 0f)) + 90;
			else
				rocket.angle = new Vector2(1f, 0f).AngleBetween(rocket.velocity) + 90;
		}
	}
}
