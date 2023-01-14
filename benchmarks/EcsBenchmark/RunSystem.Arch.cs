using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Arch.Core;

namespace EcsBenchmark
{
	public partial class RunSystem
	{
		internal struct ForEach : IForEach<Unmanaged.Component1, Unmanaged.Component2,
			Unmanaged.Component3, Unmanaged.Component4>
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public void Update(ref Unmanaged.Component1 c1, ref Unmanaged.Component2 c2,
				ref Unmanaged.Component3 c3, ref Unmanaged.Component4 c4)
			{
				c1.value = c3.value + 1;
				c2.value = c4.value + 1;
			}
		}

		internal World Arch_world;

		internal ForEach Arch_forEach;

		internal static Type[] Arch_types = new Type[] {
			typeof(Unmanaged.Component1),
			typeof(Unmanaged.Component2),
			typeof(Unmanaged.Component3),
			typeof(Unmanaged.Component4),
		};

		[GlobalSetup(Target = nameof(Arch))]
		public void Arch_Setup()
		{
			Arch_world = World.Create();
			Arch_world.Reserve(Arch_types, entityCount);

			for(int i = 0; i < entityCount;++i)
				Arch_world.Create(Arch_types);
		}

		[GlobalCleanup(Target = nameof(Arch))]
		public void Arch_Cleanup()
		{
			World.Destroy(Arch_world);
		}

		[BenchmarkCategory(Categories.Arch)]
		[Benchmark]
		public void Arch()
		{
			var query = new QueryDescription() { All = Arch_types };
			Arch_world.HPQuery<ForEach, Unmanaged.Component1, Unmanaged.Component2,
				Unmanaged.Component3, Unmanaged.Component4>(query, ref Arch_forEach);
		}
	}
}
