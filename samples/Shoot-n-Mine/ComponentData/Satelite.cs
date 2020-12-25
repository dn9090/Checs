using System;
using System.Numerics;
using Checs;

namespace Shoot_n_Mine
{
	public enum SateliteState
	{
		Flight,
		SlowApproach,
		Hover
	}

	public struct Satelite : IComponentData
	{
		public Vector2 target;

		public float destructionTime;

		public SateliteState state;

		public Satelite(Vector2 target, float destructionTime)
		{
			this.target = target;
			this.destructionTime = destructionTime;
			this.state = SateliteState.Flight;
		}
	}
}
