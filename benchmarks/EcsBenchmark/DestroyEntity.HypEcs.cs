using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using HypEcs;

namespace EcsBenchmark
{
	public partial class DestroyEntity
	{
		internal World HypEcs_world;

		internal Entity[] HypEcs_entities = new Entity[1_000_000];

		[IterationSetup(Target = nameof(HypEcs))]
		public void HypEcs_Setup()
		{
			HypEcs_world = new World();

			for (int i = 0; i < HypEcs_entities.Length; ++i)
				HypEcs_entities[i] = HypEcs_world.Spawn().Id();
		}

		[IterationCleanup(Target = nameof(HypEcs))]
		public void HypEcs_Cleanup()
		{
			HypEcs_world = null;
		}

		[Arguments(  100_000)]
		[Arguments(1_000_000)]
		[BenchmarkCategory(Categories.HypEcs)]
		[Benchmark]
		public void HypEcs(int entityCount)
		{
			for(int i = 0; i < entityCount; ++i)
				HypEcs_world.Despawn(HypEcs_entities[i]);
		}
	}
}
