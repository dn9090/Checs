# The super inoffical .NET ECS benchmark
This projects compares different ECS frameworks in various benchmarks. The goal is to get a quick overview of the performance of the ECS frameworks in selected situations.

### CreateEntity
<details>
	<summary>Environment and runtimes</summary>

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19044.2486/21H2/November2021Update)

AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores

.NET SDK=7.0.102

  [Host]     : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
  Job-FERFAT : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
</details>
<details>
	<summary>Tested frameworks</summary>

* [Checs](https://github.com/dn9090/Checs)
* [DefaultEcs](https://github.com/Doraku/DefaultEcs)
* [HypEcs](https://github.com/Byteron/HypEcs)
</details>

|      Method | entityCount | componentCount |         Mean |     Error |    StdDev |       Median |        Gen0 |      Gen1 |      Gen2 |    Allocated |
| ----------- |------------ |--------------- |------------- |---------- |---------- |------------- |------------ |---------- |---------- |------------- |
|       Checs |      100000 |              1 |     1.511 ms | 0.0301 ms | 0.0629 ms |     1.496 ms |           - |         - |         - |        480 B |
|       Checs |      100000 |              2 |     1.984 ms | 0.0391 ms | 0.0684 ms |     1.982 ms |           - |         - |         - |        480 B |
|       Checs |      100000 |              4 |     2.197 ms | 0.0436 ms | 0.0928 ms |     2.211 ms |           - |         - |         - |        480 B |
|       Checs |      100000 |              8 |     2.664 ms | 0.0523 ms | 0.0957 ms |     2.642 ms |           - |         - |         - |        480 B |
|       Checs |     1000000 |              1 |     6.113 ms | 0.0765 ms | 0.1361 ms |     6.148 ms |           - |         - |         - |        480 B |
|       Checs |     1000000 |              2 |     7.512 ms | 0.2342 ms | 0.6567 ms |     7.158 ms |           - |         - |         - |        480 B |
|       Checs |     1000000 |              4 |     9.472 ms | 0.0942 ms | 0.1351 ms |     9.469 ms |           - |         - |         - |        480 B |
|       Checs |     1000000 |              8 |    13.634 ms | 0.2716 ms | 0.3335 ms |    13.673 ms |           - |         - |         - |        480 B |
|  DefaultEcs |      100000 |              1 |    10.330 ms | 0.2014 ms | 0.1978 ms |    10.284 ms |   2000.0000 | 2000.0000 | 2000.0000 |   11591512 B |
|  DefaultEcs |      100000 |              2 |     7.842 ms | 0.1561 ms | 0.2733 ms |     7.780 ms |   2000.0000 | 2000.0000 | 2000.0000 |   15795704 B |
|  DefaultEcs |      100000 |              4 |    11.374 ms | 0.2241 ms | 0.2914 ms |    11.370 ms |   2000.0000 | 2000.0000 | 2000.0000 |   24187856 B |
|  DefaultEcs |      100000 |              8 |    20.875 ms | 0.3509 ms | 0.3604 ms |    20.901 ms |   2000.0000 | 2000.0000 | 2000.0000 |   40964064 B |
|  DefaultEcs |     1000000 |              1 |    54.922 ms | 0.5092 ms | 0.4252 ms |    54.949 ms |   3000.0000 | 2000.0000 | 2000.0000 |   99115176 B |
|  DefaultEcs |     1000000 |              2 |    71.767 ms | 0.3123 ms | 0.2768 ms |    71.615 ms |   3000.0000 | 2000.0000 | 2000.0000 |  132676640 B |
|  DefaultEcs |     1000000 |              4 |   102.860 ms | 1.8566 ms | 1.5504 ms |   103.200 ms |   5000.0000 | 4000.0000 | 3000.0000 |  199789744 B |
|  DefaultEcs |     1000000 |              8 |   226.734 ms | 3.8876 ms | 3.4463 ms |   225.937 ms |   5000.0000 | 4000.0000 | 3000.0000 |  334007232 B |
|      HypEcs |      100000 |              1 |    15.163 ms | 0.2533 ms | 0.2245 ms |    15.230 ms |   2000.0000 | 2000.0000 | 2000.0000 |   26445656 B |
|      HypEcs |      100000 |              2 |    27.860 ms | 0.3516 ms | 0.3117 ms |    27.910 ms |   2000.0000 | 2000.0000 | 2000.0000 |   46421208 B |
|      HypEcs |      100000 |              4 |    64.649 ms | 0.2844 ms | 0.2375 ms |    64.632 ms |   6000.0000 | 2000.0000 | 2000.0000 |   98373264 B |
|      HypEcs |      100000 |              8 |   172.408 ms | 1.7169 ms | 1.6060 ms |   173.418 ms |  15000.0000 | 3000.0000 | 2000.0000 |  250283160 B |
|      HypEcs |     1000000 |              1 |   139.663 ms | 1.3518 ms | 1.2645 ms |   139.830 ms |  12000.0000 | 4000.0000 | 4000.0000 |  240456880 B |
|      HypEcs |     1000000 |              2 |   282.080 ms | 5.6076 ms | 8.7304 ms |   280.583 ms |  22000.0000 | 4000.0000 | 4000.0000 |  437042520 B |
|      HypEcs |     1000000 |              4 |   594.006 ms | 1.3902 ms | 1.1609 ms |   594.374 ms |  52000.0000 | 4000.0000 | 4000.0000 |  950214736 B |
|      HypEcs |     1000000 |              8 | 1,810.258 ms | 4.1283 ms | 3.8616 ms | 1,810.110 ms | 140000.0000 | 6000.0000 | 5000.0000 | 2456565424 B |
