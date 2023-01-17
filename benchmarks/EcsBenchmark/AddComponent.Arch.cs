using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Arch.Core;

namespace EcsBenchmark
{
	public partial class AddComponent
	{
		internal World Arch_world;

		internal Entity[] Arch_entities = new Entity[1_000_000];

		[IterationSetup(Target = nameof(Arch))]
		public void Arch_Setup()
		{
			Arch_world = World.Create();
			Arch_world.Reserve(Array.Empty<Type>(), Arch_entities.Length);
			
			for(int i = 0; i < Arch_entities.Length; ++i)
				Arch_entities[i] = Arch_world.Create();
		}

		[IterationCleanup(Target = nameof(Arch))]
		public void Arch_Cleanup()
		{
			World.Destroy(Arch_world);
		}

		[Arguments(  100_000)]
		[Arguments(1_000_000)]
		[BenchmarkCategory(Categories.Arch)]
		[Benchmark]
		public void Arch(int entityCount)
		{
			for(int i = 0; i < entityCount; ++i)
				Arch_world.Add<Unmanaged.Component1>(in Arch_entities[i]);
		}
	}
}
