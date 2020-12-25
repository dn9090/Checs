using System;
using System.Numerics;
using Checs;

namespace Shoot_n_Mine
{
	public struct Position : IComponentData
	{
		public Vector2 value;

		public Position(Vector2 value)
		{
			this.value = value;
		}

		public Position(float x, float y)
		{
			this.value = new Vector2(x, y);
		}

		public static implicit operator Vector2(Position other) => other.value;

		public static explicit operator Position(Vector2 other) => new Position(other);
	}
}
