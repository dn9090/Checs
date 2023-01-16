using System;
using BenchmarkDotNet.Attributes;

namespace EcsBenchmark
{
	[MemoryDiagnoser]
	[BenchmarkCategory(Categories.CreateEntity)]
	public partial class CreateEntity
	{
	}
}
