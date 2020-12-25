using System;
using System.Numerics;
using SFML.System;

namespace Shoot_n_Mine
{
	public static class NumericExtensions
	{
		public const float PI = (float)Math.PI;

		public const float Deg2Rad = PI * 2F / 360F;

		public const float Rad2Deg = 1F / Deg2Rad;

		public static Vector3 ToEulerAngles(this Quaternion value)
		{
			Vector3 euler;

			float unit =
				(value.X * value.X) + 
				(value.Y * value.Y) +
				(value.Z * value.Z) +
				(value.W * value.W);

			float test = value.X * value.W - value.Y * value.Z;

			if (test > 0.4995f * unit)
			{
				euler.X = PI / 2;
				euler.Y = 2f * (float)Math.Atan2(value.Y, value.X);
				euler.Z = 0;
			} else if (test < -0.4995f * unit) {
				euler.X = -PI / 2;
				euler.Y = -2f * (float)Math.Atan2(value.Y, value.X);
				euler.Z = 0;
			} else {
				euler.X = (float)Math.Asin(2f * (value.W * value.X - value.Y * value.Z));
				euler.Y = (float)Math.Atan2(2f * value.W * value.Y + 2f * value.Z * value.X, 1 - 2f * (value.X * value.X + value.Y * value.Y));
				euler.Z = (float)Math.Atan2(2f * value.W * value.Z + 2f * value.X * value.Z, 1 - 2f * (value.Z * value.Z + value.X * value.X));
			}

			euler *= Rad2Deg;
			euler.X %= 360;
			euler.Y %= 360;
			euler.Z %= 360;

			return euler;
		}

		public static Vector2f ToVector2f(this Vector2 value) => new Vector2f(value.X, value.Y);

		public static Vector2 ToVector2(this Vector2f value) => new Vector2(value.X, value.Y);

		public static float AngleBetween(this Vector2 from, Vector2 to)
		{
			float denominator = (float)Math.Sqrt(from.LengthSquared() * to.LengthSquared());

			if(denominator < 1e-15f)
				return 0f;

			float dot = Math.Clamp(Vector2.Dot(from, to) / denominator, -1f, 1f); 
			return (float)Math.Acos(dot) * Rad2Deg;
		}
	}
}