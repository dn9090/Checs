using System;
using System.Numerics;
using Checs;

namespace Shoot_n_Mine
{
	public enum ExplosionType
	{
		T10,
		KL50,
		IU7
	}

	public struct Explosion : IComponentData
	{
		public int playerId;

		public float radius;

		public float radiusSqr;

		public float impact;

		public static Explosion Create(int playerId, ExplosionType type)
		{
			switch(type)
			{
				case ExplosionType.T10:
					return new Explosion(playerId, 100f, 70f);
				case ExplosionType.KL50:
					return new Explosion(playerId, 170f, 50f);
				case ExplosionType.IU7:
					return new Explosion(playerId, 160f, 100f);
			}

			return default;
		}

		public Explosion(int playerId, float radius, float impact)
		{
			this.playerId = playerId;
			this.radius = radius;
			this.radiusSqr = radius * radius;
			this.impact = impact;
		}
	}

	public struct ExplosionParticle : IComponentData
	{
		public float startTime;

		public float scale;
	}
}
