using System;
using System.Linq;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Reports;
using EcsBenchmark;

var switcher = BenchmarkSwitcher.FromTypes(new[] {
	typeof(CreateEntity),
});

var summaries = Array.Empty<Summary>();

if(args.Length > 0)
	summaries = switcher.Run(args).ToArray();
else
	summaries = switcher.RunAll().ToArray();

new Readme(summaries)
	.WithUrl(Categories.Checs,      "https://github.com/dn9090/Checs")
	.WithUrl(Categories.DefaultEcs, "https://github.com/Doraku/DefaultEcs")
	.WithUrl(Categories.HypEcs,     "https://github.com/Byteron/HypEcs")
	.WriteTo("README.md");
