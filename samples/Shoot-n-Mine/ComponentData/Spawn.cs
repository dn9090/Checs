using System;
using System.Numerics;
using Checs;

namespace Shoot_n_Mine
{
	public struct Spawn : IComponentData
	{
		public int playerId;

		public float damage;
	}
}
