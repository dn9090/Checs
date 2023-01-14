using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Checs;

namespace EcsBenchmark
{
	public partial class RunSystem
	{
		internal EntityManager Checs_manager;

		internal static ComponentType[] Checs_types = new[] {
			ComponentType.Of<Unmanaged.Component1>(),
			ComponentType.Of<Unmanaged.Component2>(),
			ComponentType.Of<Unmanaged.Component3>(),
			ComponentType.Of<Unmanaged.Component4>()
		};

		[GlobalSetup(Target = nameof(Checs))]
		public void Checs_Setup()
		{
			Checs_manager = new EntityManager();

			var archetype = Checs_manager.CreateArchetype(Checs_types);
			Checs_manager.CreateEntity(archetype, entityCount);
		}

		[GlobalCleanup(Target = nameof(Checs))]
		public void Checs_Cleanup()
		{
			Checs_manager.Dispose();
		}

		[BenchmarkCategory(Categories.Checs)]
		[Benchmark]
		public void Checs()
		{
			var query = Checs_manager.CreateQuery(includeTypes: Checs_types);
			var it    = Checs_manager.GetIterator(query);

			while(it.TryNext(out var table))
			{
				var component1s = table.GetComponentData<Unmanaged.Component1>();
				var component2s = table.GetComponentData<Unmanaged.Component2>();
				var component3s = table.GetComponentDataReadOnly<Unmanaged.Component3>();
				var component4s = table.GetComponentDataReadOnly<Unmanaged.Component4>();

				for(int i = 0; i < table.length; ++i)
				{
					component1s[i].value = component3s[i].value + 1;
					component2s[i].value = component4s[i].value + 1;
				}
			}
		}
	}
}
