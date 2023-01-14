using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace EcsBenchmark
{
	[MemoryDiagnoser]
	[BenchmarkCategory(Categories.CreateEntity)]
	public partial class CreateEntity
	{
	}
}
