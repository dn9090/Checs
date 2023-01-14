using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace EcsBenchmark
{
	// [HardwareCounters(HardwareCounter.CacheMisses)]
	[BenchmarkCategory(Categories.RunSystem)]
	public partial class RunSystem
	{
		public static int entityCount = 1_000_000;
	}
}
