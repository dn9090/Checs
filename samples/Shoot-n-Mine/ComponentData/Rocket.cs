using System;
using System.Numerics;
using Checs;

namespace Shoot_n_Mine
{
	public enum RocketType
	{
		Thor4,
		Scout8,
		Northstar10
	}

	public enum RocketState
	{
		Idle,
		Launch,
		Cruise,
		Target,
		Impact
	}

	public partial struct Rocket : IComponentData
	{
		public int playerId;

		public RocketType type;

		public RocketState state;

		public Vector2 target;

		public Vector2 velocity;

		public Vector2 acc;

		public float angle;

		public float airFrictionCoeff;

		public float cruiseAltitude;

		public bool impact;

		public Rocket(int playerId, RocketType type, Vector2 target)
		{
			this.playerId = playerId;
			this.type = type;
			this.state = RocketState.Idle;
			this.target = target;
			this.velocity = Vector2.Zero;
			this.acc = Vector2.Zero;
			this.angle = 0f;
			this.airFrictionCoeff = 0f;
			this.cruiseAltitude = 0f;
			this.impact = false;

			switch(type)
			{
				case RocketType.Thor4:
					this.airFrictionCoeff = 0.002f;
					this.cruiseAltitude = -60f;
					break;
				case RocketType.Scout8:
					this.airFrictionCoeff = 0.002f;
					this.cruiseAltitude = -40f;
					break;
				case RocketType.Northstar10:
					this.airFrictionCoeff = 0.002f;
					this.cruiseAltitude = -80f;
					break;
			}
		}
	}
}
