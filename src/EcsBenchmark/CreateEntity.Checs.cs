using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Checs;

namespace EcsBenchmark
{
	public partial class CreateEntity
	{
		internal EntityManager Checs_manager;

		internal static ComponentType[] Checs_types = new[] {
			ComponentType.Of<Unmanaged.Component1>(),
			ComponentType.Of<Unmanaged.Component2>(),
			ComponentType.Of<Unmanaged.Component3>(),
			ComponentType.Of<Unmanaged.Component4>(),
			ComponentType.Of<Unmanaged.Component5>(),
			ComponentType.Of<Unmanaged.Component6>(),
			ComponentType.Of<Unmanaged.Component7>(),
			ComponentType.Of<Unmanaged.Component8>()
		};

		[IterationSetup(Target = nameof(Checs))]
		public void Checs_Setup()
		{
			Checs_manager = new EntityManager();
		}

		[IterationCleanup(Target = nameof(Checs))]
		public void Checs_Cleanup()
		{
			Checs_manager.Dispose();
		}

		[Arguments(  100_000, 1)]
		[Arguments(1_000_000, 1)]
		[Arguments(  100_000, 2)]
		[Arguments(1_000_000, 2)]
		[Arguments(  100_000, 4)]
		[Arguments(1_000_000, 4)]
		[Arguments(  100_000, 8)]
		[Arguments(1_000_000, 8)]
		[BenchmarkCategory(Categories.Checs)]
		[Benchmark]
		public void Checs(int entityCount, int componentCount)
		{
			var types     = Checs_types.AsSpan(0, componentCount);
			var archetype = Checs_manager.CreateArchetype(types);

			Checs_manager.CreateEntity(archetype, entityCount);
		}
	}
}
