using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.Graphics;
using SFMLSys = SFML.System;
using SFML.Window;
using Checs;
using Shoot_n_Mine.Engine;

namespace Shoot_n_Mine
{
	public sealed class SateliteControlSystem : ComponentSystem
	{
		private const float ThrusterAcc = 8.0f;

		private const float LockDistanceSqr = 30.0f * 30.0f;

		private EntityQuery m_Query;
		
		private float m_FixedTimeAcc;

		public SateliteControlSystem()
		{
		}

		public override void OnEnabled(Scene scene)
		{
			this.m_Query = scene.world.CreateQuery(typeof(Satelite), typeof(Position));
			this.m_FixedTimeAcc = 0f;
		}

		public override void OnUpdate(Scene scene)
		{
			this.m_FixedTimeAcc += Time.deltaTime;

			while(this.m_FixedTimeAcc >= Time.fixedDeltaTime)
			{
				scene.world.ForEach(this.m_Query, MoveSateliteToPosition);
				this.m_FixedTimeAcc -= Time.fixedDeltaTime;
			}
		}

		private void MoveSateliteToPosition(EntityBatch batch)
		{
			var satelites = batch.GetComponentDataReadWrite<Satelite>();
			var positions = batch.GetComponentDataReadWrite<Position>();

			for(int i = 0; i < batch.length; ++i)
			{
				var pos = positions[i].value;

				switch(satelites[i].state)
				{
					case SateliteState.Flight:
						pos += new Vector2(0f, ThrusterAcc);
						if((pos - satelites[i].target).LengthSquared() < LockDistanceSqr)
							satelites[i].state = SateliteState.SlowApproach;
						positions[i] = new Position(pos);
						break;
					case SateliteState.SlowApproach:
						pos += new Vector2(0f, ThrusterAcc * 0.3f);
						if((pos - satelites[i].target).LengthSquared() < 150f)
						{
							positions[i] = new Position(satelites[i].target);
							satelites[i].state = SateliteState.Hover;
						} else {
							positions[i] = new Position(pos);
						}
						break;
					case SateliteState.Hover:
						break;
				}
			}
		}
	}
}
