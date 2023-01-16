# The super inofficial .NET ECS benchmark
This projects compares different ECS frameworks in various benchmarks. The goal is to get a quick overview of the performance of the ECS frameworks in selected situations.

> Note that the benchmarks try to achieve the best possible execution time for the frameworks. However, APIs may change or other APIs may be better suited for the benchmark. In such cases, please contact the author.

## Results

### CreateEntity
<details>
	<summary>Environment and runtimes</summary>

```
BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19044.2486/21H2/November2021Update)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.102
  [Host]     : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
  Job-GLKZDZ : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
```
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
|        Arch |      100000 |              1 |     8.255 ms |     8.230 ms |   1000.0000 | 1000.0000 | 1000.0000 |    9959384 B |
|        Arch |      100000 |              2 |     9.479 ms |     9.391 ms |   1000.0000 | 1000.0000 | 1000.0000 |   10130240 B |
|        Arch |      100000 |              4 |     8.961 ms |     8.926 ms |   1000.0000 | 1000.0000 | 1000.0000 |   11063272 B |
|        Arch |      100000 |              8 |    14.688 ms |    14.679 ms |   1000.0000 | 1000.0000 | 1000.0000 |   12569032 B |
|        Arch |     1000000 |              1 |    48.796 ms |    48.784 ms |   3000.0000 | 2000.0000 | 1000.0000 |  136876056 B |
|        Arch |     1000000 |              2 |    60.821 ms |    60.669 ms |   3000.0000 | 2000.0000 | 1000.0000 |  139019232 B |
|        Arch |     1000000 |              4 |    86.791 ms |    86.400 ms |   3000.0000 | 2000.0000 | 1000.0000 |  148080136 B |
|        Arch |     1000000 |              8 |   135.023 ms |   134.667 ms |   5000.0000 | 4000.0000 | 2000.0000 |  163476536 B |
|       Checs |      100000 |              1 |     1.711 ms |     1.673 ms |           - |         - |         - |        480 B |
|       Checs |      100000 |              2 |     2.159 ms |     2.143 ms |           - |         - |         - |        480 B |
|       Checs |      100000 |              4 |     1.985 ms |     2.227 ms |           - |         - |         - |        480 B |
|       Checs |      100000 |              8 |     2.836 ms |     2.823 ms |           - |         - |         - |        480 B |
|       Checs |     1000000 |              1 |     7.519 ms |     7.485 ms |           - |         - |         - |        480 B |
|       Checs |     1000000 |              2 |     8.557 ms |     8.492 ms |           - |         - |         - |        480 B |
|       Checs |     1000000 |              4 |    10.404 ms |    10.380 ms |           - |         - |         - |        480 B |
|       Checs |     1000000 |              8 |    17.263 ms |    17.953 ms |           - |         - |         - |        480 B |
|  DefaultEcs |      100000 |              1 |    10.772 ms |    10.751 ms |   2000.0000 | 2000.0000 | 2000.0000 |   11591528 B |
|  DefaultEcs |      100000 |              2 |     7.840 ms |     7.779 ms |   2000.0000 | 2000.0000 | 2000.0000 |   15795704 B |
|  DefaultEcs |      100000 |              4 |    11.811 ms |    11.812 ms |   2000.0000 | 2000.0000 | 2000.0000 |   24187880 B |
|  DefaultEcs |      100000 |              8 |    22.416 ms |    22.327 ms |   2000.0000 | 2000.0000 | 2000.0000 |   40964088 B |
|  DefaultEcs |     1000000 |              1 |    56.724 ms |    55.844 ms |   3000.0000 | 2000.0000 | 2000.0000 |   99123304 B |
|  DefaultEcs |     1000000 |              2 |    79.151 ms |    79.452 ms |   3000.0000 | 2000.0000 | 2000.0000 |  132676560 B |
|  DefaultEcs |     1000000 |              4 |   109.684 ms |   108.083 ms |   5000.0000 | 4000.0000 | 3000.0000 |  199797712 B |
|  DefaultEcs |     1000000 |              8 |   230.084 ms |   228.013 ms |   5000.0000 | 4000.0000 | 3000.0000 |  334007232 B |
|      HypEcs |      100000 |              1 |    15.235 ms |    15.321 ms |   2000.0000 | 2000.0000 | 2000.0000 |   26445656 B |
|      HypEcs |      100000 |              2 |    28.928 ms |    28.931 ms |   2000.0000 | 2000.0000 | 2000.0000 |   46421208 B |
|      HypEcs |      100000 |              4 |    63.047 ms |    62.904 ms |   6000.0000 | 2000.0000 | 2000.0000 |   98373240 B |
|      HypEcs |      100000 |              8 |   180.107 ms |   180.050 ms |  15000.0000 | 3000.0000 | 2000.0000 |  250283160 B |
|      HypEcs |     1000000 |              1 |   149.631 ms |   153.300 ms |  12000.0000 | 4000.0000 | 4000.0000 |  240456880 B |
|      HypEcs |     1000000 |              2 |   276.285 ms |   276.916 ms |  22000.0000 | 4000.0000 | 4000.0000 |  437042512 B |
|      HypEcs |     1000000 |              4 |   627.494 ms |   624.338 ms |  53000.0000 | 5000.0000 | 5000.0000 |  950218048 B |
|      HypEcs |     1000000 |              8 | 1,786.057 ms | 1,787.175 ms | 140000.0000 | 6000.0000 | 5000.0000 | 2456565376 B |


### DestroyEntity
<details>
	<summary>Environment and runtimes</summary>

```
BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19044.2486/21H2/November2021Update)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.102
  [Host]     : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
  Job-GLKZDZ : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
```
</details>
<details>
	<summary>Tested frameworks</summary>

* [Arch](https://github.com/genaray/Arch)
* [Checs](https://github.com/dn9090/Checs)
* [DefaultEcs](https://github.com/Doraku/DefaultEcs)
* [HypEcs](https://github.com/Byteron/HypEcs)
</details>

|      Method | entityCount |           Mean |         Median |
| ----------- |------------ |--------------- |--------------- |
|        Arch |      100000 |   561,268.8 μs |   561,022.6 μs |
|        Arch |     1000000 | 2,643,755.4 μs | 2,645,443.2 μs |
|       Checs |      100000 |       265.7 μs |       257.9 μs |
|       Checs |     1000000 |     2,716.9 μs |     2,686.1 μs |
|  DefaultEcs |      100000 |     1,187.6 μs |     1,183.9 μs |
|  DefaultEcs |     1000000 |    22,191.9 μs |    22,184.9 μs |
|      HypEcs |      100000 |     3,379.6 μs |     3,366.5 μs |
|      HypEcs |     1000000 |    33,310.8 μs |    33,474.1 μs |


### RunSystem
<details>
	<summary>Environment and runtimes</summary>

```
BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19044.2486/21H2/November2021Update)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.102
  [Host]     : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
```
</details>
<details>
	<summary>Tested frameworks</summary>

* [Arch](https://github.com/genaray/Arch)
* [Checs](https://github.com/dn9090/Checs)
* [DefaultEcs](https://github.com/Doraku/DefaultEcs)
</details>

|      Method |       Mean |
| ----------- |----------- |
|        Arch |   757.3 μs |
|       Checs |   743.2 μs |
|  DefaultEcs | 2,227.1 μs |

## Contributing
### Adding a framework
...
### Create a new benchmark
...
