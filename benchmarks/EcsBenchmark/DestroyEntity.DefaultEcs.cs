using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using DefaultEcs;

namespace EcsBenchmark
{
	public partial class DestroyEntity
	{
		internal World DefaultEcs_world;

		internal Entity[] DefaultEcs_entities = new Entity[1_000_000];

		[IterationSetup(Target = nameof(DefaultEcs))]
		public void DefaultEcs_Setup()
		{
			DefaultEcs_world = new World();

			for(int i = 0; i < DefaultEcs_entities.Length; ++i)
				DefaultEcs_entities[i] = DefaultEcs_world.CreateEntity();
		}

		[IterationCleanup(Target = nameof(DefaultEcs))]
		public void DefaultEcs_Cleanup()
		{
			DefaultEcs_world.Dispose();
		}

		[Arguments(  100_000)]
		[Arguments(1_000_000)]
		[BenchmarkCategory(Categories.DefaultEcs)]
		[Benchmark]
		public void DefaultEcs(int entityCount)
		{
			for(int i = 0; i < entityCount; ++i)
				DefaultEcs_entities[i].Dispose();
		}
	}
}
