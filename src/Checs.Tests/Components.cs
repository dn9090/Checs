using System;

namespace Checs.Tests
{
#pragma warning disable CS0169

	public struct Layer : IComponentData
	{
		public int value;

		public Layer(int value)
		{
			this.value = value;
		}
	}

	public struct Position : IComponentData
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

	public struct Rotation : IComponentData
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
	}

	public struct Velocity : IComponentData
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
	}

	public struct Line : IComponentData
	{
		public Position start;

		public Position end;

		public Line(Position start, Position end)
		{
			this.start = start;
			this.end = end;
		}
	}

	public struct Hitpoints : IComponentData
	{
		public int value;

		public Hitpoints(int value)
		{
			this.value = value;
		}
	}

	public struct ComponentA : IComponentData { int value; }
	public struct ComponentB : IComponentData { int value; }
	public struct ComponentC : IComponentData { int value; }
	public struct ComponentD : IComponentData { int value; }
	public struct ComponentE : IComponentData { int value; }
	public struct ComponentF : IComponentData { int value; }
	public struct ComponentG : IComponentData { int value; }
	public struct ComponentH : IComponentData { int value; }
	public struct ComponentI : IComponentData { int value; }
	public struct ComponentJ : IComponentData { int value; }
	public struct ComponentK : IComponentData { int value; }
	public struct ComponentL : IComponentData { int value; }
	public struct ComponentM : IComponentData { int value; }
	public struct ComponentN : IComponentData { int value; }
	public struct ComponentO : IComponentData { int value; }
	public struct ComponentP : IComponentData { int value; }
	public struct ComponentQ : IComponentData { int value; }
	public struct ComponentR : IComponentData { int value; }
	public struct ComponentS : IComponentData { int value; }
	public struct ComponentT : IComponentData { int value; }
	public struct ComponentU : IComponentData { int value; }
	public struct ComponentV : IComponentData { int value; }
	public struct ComponentW : IComponentData { int value; }
	public struct ComponentX : IComponentData { int value; }
	public struct ComponentY : IComponentData { int value; }
	public struct ComponentZ : IComponentData { int value; }
}
