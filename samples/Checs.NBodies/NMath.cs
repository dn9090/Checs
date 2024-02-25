using System;
using System.Numerics;
using SFML.Graphics;

namespace Checs.NBodies
{
	public static class NMath
	{
		public static Vector2 Perpendicular(Vector2 value)
		{
			return new Vector2(-value.Y, value.X);
		}

		public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
		{
			return a + (b - a) * t;
		}
	}
}