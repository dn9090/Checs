using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace EcsBenchmark
{
	public static partial class Unmanaged
	{
		public partial struct Component1
		{
			public int value;
		}

		public partial struct Component2
		{
			public int value;
		}

		public partial struct Component3
		{
			public int value;
		}

		public partial struct Component4
		{
			public int value;
		}

		public partial struct Component5
		{
			public int value;
		}

		public partial struct Component6
		{
			public int value;
		}

		public partial struct Component7
		{
			public int value;
		}

		public partial struct Component8
		{
			public int value;
		}
	}

	public static partial class Managed
	{
		public partial class Component1
		{
			public int value;
		}

		public partial class Component2
		{
			public int value;
		}

		public partial class Component3
		{
			public int value;
		}

		public partial class Component4
		{
			public int value;
		}

		public partial class Component5
		{
			public int value;
		}

		public partial class Component6
		{
			public int value;
		}

		public partial class Component7
		{
			public int value;
		}

		public partial class Component8
		{
			public int value;
		}
	}
}
