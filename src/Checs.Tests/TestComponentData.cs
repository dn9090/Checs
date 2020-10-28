using System;

namespace Checs.Tests
{
	public struct Position
	{
		public float x;

		public float y;

		public float z;

		public Position(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
	}

	public struct EulerAngles
	{
		public float x;

		public float y;

		public float z;

		public EulerAngles(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
	}

	public struct Teleporter
	{
		public Position from;

		public Position to;

		public Teleporter(Position from, Position to)
		{
			this.from = from;
			this.to = to;
		}
	}
}
