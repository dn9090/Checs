using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Arch.Core;

namespace EcsBenchmark
{
	public partial class CreateEntity
	{
		internal World Arch_world;

		internal static Type[] Arch_types = new[] {
			typeof(Unmanaged.Component1),
			typeof(Unmanaged.Component2),
			typeof(Unmanaged.Component3),
			typeof(Unmanaged.Component4),
			typeof(Unmanaged.Component5),
			typeof(Unmanaged.Component6),
			typeof(Unmanaged.Component7),
			typeof(Unmanaged.Component8)
		};

		[IterationSetup(Target = nameof(Arch))]
		public void Arch_Setup()
		{
			Arch_world = World.Create();
		}

		[IterationCleanup(Target = nameof(Arch))]
		public void Arch_Cleanup()
		{
			World.Destroy(Arch_world);
		}

		[Arguments(  100_000, 1)]
		[Arguments(1_000_000, 1)]
		[Arguments(  100_000, 2)]
		[Arguments(1_000_000, 2)]
		[Arguments(  100_000, 4)]
		[Arguments(1_000_000, 4)]
		[Arguments(  100_000, 8)]
		[Arguments(1_000_000, 8)]
		[BenchmarkCategory(Categories.Arch)]
		[Benchmark]
		public void Arch(int entityCount, int componentCount)
		{
			var types = Arch_types.AsSpan(0, componentCount).ToArray();
			Arch_world.Reserve(types, entityCount);
			
			for(int i = 0; i < entityCount; ++i)
				Arch_world.Create(types);
		}
	}
}
