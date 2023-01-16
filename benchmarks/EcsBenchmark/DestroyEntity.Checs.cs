using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Checs;

namespace EcsBenchmark
{
	public partial class DestroyEntity
	{
		internal EntityManager Checs_manager;
		
		internal Entity[] Checs_entities = new Entity[1_000_000];

		[IterationSetup(Target = nameof(Checs))]
		public void Checs_Setup()
		{
			Checs_manager = new EntityManager();
			Checs_manager.CreateEntity(EntityArchetype.empty, Checs_entities);
		}

		[IterationCleanup(Target = nameof(Checs))]
		public void Checs_Cleanup()
		{
			Checs_manager.Dispose();
		}

		[Arguments(  100_000)]
		[Arguments(1_000_000)]
		[BenchmarkCategory(Categories.Checs)]
		[Benchmark]
		public void Checs(int entityCount)
		{
			Checs_manager.DestroyEntity(Checs_entities.AsSpan(0, entityCount));
		}
	}
}
