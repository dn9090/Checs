using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using DefaultEcs;

namespace EcsBenchmark
{
	public partial class CreateEntity
	{
		internal World DefaultEcs_world;

		[IterationSetup(Target = nameof(DefaultEcs))]
		public void DefaultEcs_Setup()
		{
			DefaultEcs_world = new World();
		}

		[IterationCleanup(Target = nameof(DefaultEcs))]
		public void DefaultEcs_Cleanup()
		{
			DefaultEcs_world.Dispose();
		}

		[Arguments(  100_000, 1)]
		[Arguments(1_000_000, 1)]
		[Arguments(  100_000, 2)]
		[Arguments(1_000_000, 2)]
		[Arguments(  100_000, 4)]
		[Arguments(1_000_000, 4)]
		[Arguments(  100_000, 8)]
		[Arguments(1_000_000, 8)]
		[BenchmarkCategory(Categories.DefaultEcs)]
		[Benchmark]
		public void DefaultEcs(int entityCount, int componentCount)
		{
			switch(componentCount)
			{
				case 1: DefaultEcs_1(entityCount); break;
				case 2: DefaultEcs_2(entityCount); break;
				case 4: DefaultEcs_4(entityCount); break;
				case 8: DefaultEcs_8(entityCount); break;
				default: throw new NotImplementedException();
			}
		}

		public void DefaultEcs_1(int entityCount)
		{
			for (int i = 0; i < entityCount; ++i)
			{
				var entity = DefaultEcs_world.CreateEntity();
				entity.Set<Unmanaged.Component1>();
			}
		}

		public void DefaultEcs_2(int entityCount)
		{
			for (int i = 0; i < entityCount; ++i)
			{
				var entity = DefaultEcs_world.CreateEntity();
				entity.Set<Unmanaged.Component1>();
				entity.Set<Unmanaged.Component2>();
			}
		}

		public void DefaultEcs_4(int entityCount)
		{
			for (int i = 0; i < entityCount; ++i)
			{
				var entity = DefaultEcs_world.CreateEntity();
				entity.Set<Unmanaged.Component1>();
				entity.Set<Unmanaged.Component2>();
				entity.Set<Unmanaged.Component3>();
				entity.Set<Unmanaged.Component4>();
			}
		}

		public void DefaultEcs_8(int entityCount)
		{
			for (int i = 0; i < entityCount; ++i)
			{
				var entity = DefaultEcs_world.CreateEntity();
				entity.Set<Unmanaged.Component1>();
				entity.Set<Unmanaged.Component2>();
				entity.Set<Unmanaged.Component3>();
				entity.Set<Unmanaged.Component4>();
				entity.Set<Unmanaged.Component5>();
				entity.Set<Unmanaged.Component6>();
				entity.Set<Unmanaged.Component7>();
				entity.Set<Unmanaged.Component8>();
			}
		}
	}
}
