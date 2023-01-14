using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using DefaultEcs;
using DefaultEcs.System;

namespace EcsBenchmark
{
	public partial class RunSystem
	{
		sealed partial class EntitySetSystem : AEntitySetSystem<int>
		{
			[Update]
			private static void Update(ref Unmanaged.Component1 c1, ref Unmanaged.Component2 c2,
				in Unmanaged.Component3 c3, in Unmanaged.Component4 c4)
			{
				c1.value = c3.value + 1;
				c2.value = c4.value + 1;
			}
		}

		internal World DefaultEcs_world;

		internal ISystem<int> DefaultEcs_system;

		[GlobalSetup(Target = nameof(DefaultEcs))]
		public void DefaultEcs_Setup()
		{
			DefaultEcs_world = new World();
			DefaultEcs_system = new EntitySetSystem(DefaultEcs_world);

			for (int i = 0; i < entityCount; ++i)
			{
				var entity = DefaultEcs_world.CreateEntity();
				entity.Set<Unmanaged.Component1>();
				entity.Set<Unmanaged.Component2>();
				entity.Set<Unmanaged.Component3>();
				entity.Set<Unmanaged.Component4>();
			}
		}

		[GlobalCleanup(Target = nameof(DefaultEcs))]
		public void DefaultEcs_Cleanup()
		{
			DefaultEcs_world.Dispose();
		}

		[BenchmarkCategory(Categories.DefaultEcs)]
		[Benchmark]
		public void DefaultEcs()
		{
			DefaultEcs_system.Update(0);
		}
	}
}
