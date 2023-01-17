using System;
using System.Linq;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Reports;
using EcsBenchmark;

var switcher = BenchmarkSwitcher.FromTypes(new[] {
	typeof(CreateEntity),
	typeof(DestroyEntity),
	typeof(AddComponent),
	typeof(RemoveComponent),
	typeof(RunSystem),
});

if(args.Length > 0)
{
	switcher.Run(args);
	return;
}

var summaries = switcher.RunAll();
new Readme(summaries.ToArray())
	.WithUrl(Categories.Arch,       "https://github.com/genaray/Arch")
	.WithUrl(Categories.Checs,      "https://github.com/dn9090/Checs")
	.WithUrl(Categories.DefaultEcs, "https://github.com/Doraku/DefaultEcs")
	.WithUrl(Categories.HypEcs,     "https://github.com/Byteron/HypEcs")
	.WriteTo("README.md");

