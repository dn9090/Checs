using System;

namespace Checs.Tests
{
#pragma warning disable CS0169

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

		public override string ToString()
		{
			return "<" + this.x + "," + this.y + "," + this.z + ">";
		}
	}

	public struct Rotation
	{
		public float x;

		public float y;

		public float z;

		public float w;

		public Rotation(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public override string ToString()
		{
			return "<" + this.x + "," + this.y + "," + this.z + "," + this.w + ">";
		}
	}

	public struct Velocity
	{
		public float x;

		public float y;

		public float z;

		public Velocity(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public override string ToString()
		{
			return "<" + this.x + "," + this.y + "," + this.z + ">";
		}
	}

	public struct Scale
	{
		public float x;

		public float y;

		public float z;

		public Scale(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public override string ToString()
		{
			return "<" + this.x + "," + this.y + "," + this.z + ">";
		}
	}

	public struct Health
	{
		public int value;

		public Health(int value)
		{
			this.value = value;
		}
	}

	public struct Generic<T> where T : unmanaged
	{
		public T value;
	}

	public struct ComponentA { int value; }
	public struct ComponentB { int value; }
	public struct ComponentC { int value; }
	public struct ComponentD { int value; }
	public struct ComponentE { int value; }
	public struct ComponentF { int value; }
	public struct ComponentG { int value; }
	public struct ComponentH { int value; }
	public struct ComponentI { int value; }
	public struct ComponentJ { int value; }
	public struct ComponentK { int value; }
	public struct ComponentL { int value; }
	public struct ComponentM { int value; }
	public struct ComponentN { int value; }
	public struct ComponentO { int value; }
	public struct ComponentP { int value; }
	public struct ComponentQ { int value; }
	public struct ComponentR { int value; }
	public struct ComponentS { int value; }
	public struct ComponentT { int value; }
	public struct ComponentU { int value; }
	public struct ComponentV { int value; }
	public struct ComponentW { int value; }
	public struct ComponentX { int value; }
	public struct ComponentY { int value; }
	public struct ComponentZ { int value; }
}
