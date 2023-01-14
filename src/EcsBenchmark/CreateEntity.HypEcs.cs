using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using HypEcs;

namespace EcsBenchmark
{
	public partial class CreateEntity
	{
		internal World HypEcs_world;

		[IterationSetup(Target = nameof(HypEcs))]
		public void HypEcs_Setup()
		{
			HypEcs_world = new World();
		}

		[IterationCleanup(Target = nameof(HypEcs))]
		public void HypEcs_Cleanup()
		{
			HypEcs_world = null;
		}

		[Arguments(  100_000, 1)]
		[Arguments(1_000_000, 1)]
		[Arguments(  100_000, 2)]
		[Arguments(1_000_000, 2)]
		[Arguments(  100_000, 4)]
		[Arguments(1_000_000, 4)]
		[Arguments(  100_000, 8)]
		[Arguments(1_000_000, 8)]
		[BenchmarkCategory(Categories.HypEcs)]
		[Benchmark]
		public void HypEcs(int entityCount, int componentCount)
		{
			switch(componentCount)
			{
				case 1: HypEcs_1(entityCount); break;
				case 2: HypEcs_2(entityCount); break;
				case 4: HypEcs_4(entityCount); break;
				case 8: HypEcs_8(entityCount); break;
				default: throw new NotImplementedException();
			}
		}

		public void HypEcs_1(int entityCount)
		{
			for (int i = 0; i < entityCount; ++i)
			{
				var entity = HypEcs_world.Spawn();
				entity.Add<Unmanaged.Component1>();
			}
		}

		public void HypEcs_2(int entityCount)
		{
			for (int i = 0; i < entityCount; ++i)
			{
				var entity = HypEcs_world.Spawn();
				entity.Add<Unmanaged.Component1>();
				entity.Add<Unmanaged.Component2>();
			}
		}

		public void HypEcs_4(int entityCount)
		{
			for (int i = 0; i < entityCount; ++i)
			{
				var entity = HypEcs_world.Spawn();
				entity.Add<Unmanaged.Component1>();
				entity.Add<Unmanaged.Component2>();
				entity.Add<Unmanaged.Component3>();
				entity.Add<Unmanaged.Component4>();
			}
		}

		public void HypEcs_8(int entityCount)
		{
			for (int i = 0; i < entityCount; ++i)
			{
				var entity = HypEcs_world.Spawn();
				entity.Add<Unmanaged.Component1>();
				entity.Add<Unmanaged.Component2>();
				entity.Add<Unmanaged.Component3>();
				entity.Add<Unmanaged.Component4>();
				entity.Add<Unmanaged.Component5>();
				entity.Add<Unmanaged.Component6>();
				entity.Add<Unmanaged.Component7>();
				entity.Add<Unmanaged.Component8>();
			}
		}
	}
}
