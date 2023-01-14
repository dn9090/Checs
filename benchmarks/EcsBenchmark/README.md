# The super inofficial .NET ECS benchmark
This projects compares different ECS frameworks in various benchmarks. The goal is to get a quick overview of the performance of the ECS frameworks in selected situations.

### CreateEntity
<details>
	<summary>Environment and runtimes</summary>

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19044.2486/21H2/November2021Update)

AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores

.NET SDK=7.0.102

  [Host]     : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
  Job-ZLOYIZ : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
</details>
<details>
	<summary>Tested frameworks</summary>

* [Arch](https://github.com/genaray/Arch)
* [Checs](https://github.com/dn9090/Checs)
* [DefaultEcs](https://github.com/Doraku/DefaultEcs)
* [HypEcs](https://github.com/Byteron/HypEcs)
</details>

|      Method | entityCount | componentCount |         Mean |       Median |        Gen0 |      Gen1 |      Gen2 |    Allocated |
| ----------- |------------ |--------------- |------------- |------------- |------------ |---------- |---------- |------------- |
|        Arch |      100000 |              1 |     8.274 ms |     8.233 ms |   1000.0000 | 1000.0000 | 1000.0000 |    9959384 B |
|        Arch |      100000 |              2 |     6.415 ms |     6.412 ms |   1000.0000 | 1000.0000 | 1000.0000 |   10130240 B |
|        Arch |      100000 |              4 |     9.101 ms |     9.052 ms |   1000.0000 | 1000.0000 | 1000.0000 |   11063296 B |
|        Arch |      100000 |              8 |    13.144 ms |    13.042 ms |   1000.0000 | 1000.0000 | 1000.0000 |   12569032 B |
|        Arch |     1000000 |              1 |    47.665 ms |    47.551 ms |   3000.0000 | 2000.0000 | 1000.0000 |  136870696 B |
|        Arch |     1000000 |              2 |    60.236 ms |    60.247 ms |   3000.0000 | 2000.0000 | 1000.0000 |  139019232 B |
|        Arch |     1000000 |              4 |    84.763 ms |    84.672 ms |   3000.0000 | 2000.0000 | 1000.0000 |  148080136 B |
|        Arch |     1000000 |              8 |   131.314 ms |   130.919 ms |   5000.0000 | 4000.0000 | 2000.0000 |  163480688 B |
|       Checs |      100000 |              1 |     1.548 ms |     1.521 ms |           - |         - |         - |        480 B |
|       Checs |      100000 |              2 |     1.963 ms |     1.933 ms |           - |         - |         - |        480 B |
|       Checs |      100000 |              4 |     2.101 ms |     2.083 ms |           - |         - |         - |        480 B |
|       Checs |      100000 |              8 |     2.512 ms |     2.532 ms |           - |         - |         - |        480 B |
|       Checs |     1000000 |              1 |     5.570 ms |     5.540 ms |           - |         - |         - |        480 B |
|       Checs |     1000000 |              2 |     6.457 ms |     6.428 ms |           - |         - |         - |        480 B |
|       Checs |     1000000 |              4 |     8.542 ms |     8.447 ms |           - |         - |         - |        480 B |
|       Checs |     1000000 |              8 |    12.472 ms |    12.463 ms |           - |         - |         - |        480 B |
|  DefaultEcs |      100000 |              1 |     5.636 ms |     5.554 ms |   2000.0000 | 2000.0000 | 2000.0000 |   11591808 B |
|  DefaultEcs |      100000 |              2 |     7.627 ms |     7.663 ms |   2000.0000 | 2000.0000 | 2000.0000 |   15795680 B |
|  DefaultEcs |      100000 |              4 |    11.516 ms |    11.516 ms |   2000.0000 | 2000.0000 | 2000.0000 |   24187880 B |
|  DefaultEcs |      100000 |              8 |    20.886 ms |    20.836 ms |   2000.0000 | 2000.0000 | 2000.0000 |   40964064 B |
|  DefaultEcs |     1000000 |              1 |    56.622 ms |    55.495 ms |   3000.0000 | 2000.0000 | 2000.0000 |   99115224 B |
|  DefaultEcs |     1000000 |              2 |    74.096 ms |    73.016 ms |   3000.0000 | 2000.0000 | 2000.0000 |  132676664 B |
|  DefaultEcs |     1000000 |              4 |   106.322 ms |   106.872 ms |   5000.0000 | 4000.0000 | 3000.0000 |  199789816 B |
|  DefaultEcs |     1000000 |              8 |   223.007 ms |   223.809 ms |   5000.0000 | 4000.0000 | 3000.0000 |  334007232 B |
|      HypEcs |      100000 |              1 |    15.301 ms |    15.351 ms |   2000.0000 | 2000.0000 | 2000.0000 |   26445656 B |
|      HypEcs |      100000 |              2 |    28.329 ms |    28.328 ms |   2000.0000 | 2000.0000 | 2000.0000 |   46421208 B |
|      HypEcs |      100000 |              4 |    62.570 ms |    62.507 ms |   6000.0000 | 2000.0000 | 2000.0000 |   98373264 B |
|      HypEcs |      100000 |              8 |   176.663 ms |   176.351 ms |  15000.0000 | 3000.0000 | 2000.0000 |  250283184 B |
|      HypEcs |     1000000 |              1 |   143.084 ms |   138.604 ms |  12000.0000 | 4000.0000 | 4000.0000 |  240456880 B |
|      HypEcs |     1000000 |              2 |   276.894 ms |   277.305 ms |  22000.0000 | 4000.0000 | 4000.0000 |  437042520 B |
|      HypEcs |     1000000 |              4 |   598.498 ms |   598.675 ms |  52000.0000 | 4000.0000 | 4000.0000 |  950214712 B |
|      HypEcs |     1000000 |              8 | 1,776.389 ms | 1,771.772 ms | 140000.0000 | 6000.0000 | 5000.0000 | 2456565384 B |

### RunSystem
<details>
	<summary>Environment and runtimes</summary>

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19044.2486/21H2/November2021Update)

AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores

.NET SDK=7.0.102

  [Host]     : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
</details>
<details>
	<summary>Tested frameworks</summary>

* [Arch](https://github.com/genaray/Arch)
* [Checs](https://github.com/dn9090/Checs)
* [DefaultEcs](https://github.com/Doraku/DefaultEcs)
</details>

|      Method |       Mean |     Median |
| ----------- |----------- |----------- |
|        Arch |   796.6 μs |   794.5 μs |
|       Checs |   793.6 μs |   795.4 μs |
|  DefaultEcs | 2,268.7 μs | 2,233.4 μs |
