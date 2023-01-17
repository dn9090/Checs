using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Checs;

namespace EcsBenchmark
{
	public partial class RemoveComponent
	{
		internal EntityManager Checs_manager;

		internal Entity[] Checs_entities = new Entity[1_000_000];

		[IterationSetup(Target = nameof(Checs))]
		public void Checs_Setup()
		{
			Checs_manager = new EntityManager();

			var archetype = Checs_manager.CreateArchetype(ComponentType.Of<Unmanaged.Component1>());
			Checs_manager.CreateEntity(archetype, Checs_entities);
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
			for(int i = 0; i < entityCount; ++i)
				Checs_manager.RemoveComponentData<Unmanaged.Component1>(Checs_entities[i]);
		}
	}
}
