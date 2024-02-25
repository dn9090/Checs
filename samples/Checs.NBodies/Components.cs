using System;
using System.Numerics;
using SFML.Graphics;
using SFML.System;

namespace Checs.NBodies
{
	public struct Position
	{
		public Vector2 value;

		public Position(Vector2 value)
		{
			this.value = value;	
		}
	}

	public struct Velocity
	{
		public Vector2 value;

		public Velocity(Vector2 value)
		{
			this.value = value;	
		}
	}

	public struct Acceleration
	{
		public Vector2 value;

		public Acceleration(Vector2 value)
		{
			this.value = value;	
		}
	}

	public struct Mass
	{
		public float value;

		public Mass(float value)
		{
			this.value = value;
		}
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