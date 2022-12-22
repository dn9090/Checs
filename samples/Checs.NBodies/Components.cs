using System;
using System.Numerics;
using SFML.Graphics;

namespace Checs.NBodies
{
	public struct Position
	{
		public Vector2 value;
	}

	public struct Velocity
	{
		public Vector2 value;
	}

	public struct Acceleration
	{
		public Vector2 value;
	}

	public struct Mass
	{
		public float value;
	}

	public struct GravCenter
	{
	}

	public struct RenderShape
	{
		public float radius;

		public Color color;

		public Color baseColor;
	}
}