# The super inofficial .NET ECS benchmark
This projects compares different ECS frameworks and data structures in various benchmarks. The goal is to get a quick overview of the performance of ECS designs in selected situations.

> Note that the benchmarks try to achieve the best possible execution time. However, APIs may change or other APIs may be better suited for the benchmark. In such cases, please contact the author.

## Results

### CreateEntity
<details>
	<summary>Environment and runtimes</summary>

```
BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.2728)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.102
  [Host]     : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
  Job-HBPPIA : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
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
|        Arch |      100000 |              1 |     5.263 ms |     5.210 ms |   1000.0000 | 1000.0000 | 1000.0000 |    9959360 B |
|        Arch |      100000 |              2 |     6.830 ms |     6.817 ms |   1000.0000 | 1000.0000 | 1000.0000 |   10130240 B |
|        Arch |      100000 |              4 |     8.847 ms |     8.764 ms |   1000.0000 | 1000.0000 | 1000.0000 |   11063296 B |
|        Arch |      100000 |              8 |    13.559 ms |    13.579 ms |   1000.0000 | 1000.0000 | 1000.0000 |   12569032 B |
|        Arch |     1000000 |              1 |    48.223 ms |    48.113 ms |   3000.0000 | 2000.0000 | 1000.0000 |  136872960 B |
|        Arch |     1000000 |              2 |    61.815 ms |    61.397 ms |   3000.0000 | 2000.0000 | 1000.0000 |  139019232 B |
|        Arch |     1000000 |              4 |    85.164 ms |    85.178 ms |   3000.0000 | 2000.0000 | 1000.0000 |  148080136 B |
|        Arch |     1000000 |              8 |   130.792 ms |   130.746 ms |   5000.0000 | 4000.0000 | 2000.0000 |  163474640 B |
|       Checs |      100000 |              1 |     1.534 ms |     1.529 ms |           - |         - |         - |        480 B |
|       Checs |      100000 |              2 |     1.998 ms |     1.980 ms |           - |         - |         - |        480 B |
|       Checs |      100000 |              4 |     1.851 ms |     2.112 ms |           - |         - |         - |        480 B |
|       Checs |      100000 |              8 |     2.570 ms |     2.530 ms |           - |         - |         - |        480 B |
|       Checs |     1000000 |              1 |     5.545 ms |     5.500 ms |           - |         - |         - |        480 B |
|       Checs |     1000000 |              2 |     6.758 ms |     6.727 ms |           - |         - |         - |        480 B |
|       Checs |     1000000 |              4 |     8.694 ms |     8.753 ms |           - |         - |         - |        480 B |
|       Checs |     1000000 |              8 |    12.691 ms |    12.706 ms |           - |         - |         - |        480 B |
|  DefaultEcs |      100000 |              1 |     5.709 ms |     5.634 ms |   2000.0000 | 2000.0000 | 2000.0000 |   11594104 B |
|  DefaultEcs |      100000 |              2 |     7.823 ms |     7.786 ms |   2000.0000 | 2000.0000 | 2000.0000 |   15795704 B |
|  DefaultEcs |      100000 |              4 |    11.610 ms |    11.637 ms |   2000.0000 | 2000.0000 | 2000.0000 |   24187856 B |
|  DefaultEcs |      100000 |              8 |    21.168 ms |    21.171 ms |   2000.0000 | 2000.0000 | 2000.0000 |   40964072 B |
|  DefaultEcs |     1000000 |              1 |    55.360 ms |    55.304 ms |   3000.0000 | 2000.0000 | 2000.0000 |   99115208 B |
|  DefaultEcs |     1000000 |              2 |    73.487 ms |    73.083 ms |   3000.0000 | 2000.0000 | 2000.0000 |  132676664 B |
|  DefaultEcs |     1000000 |              4 |   104.576 ms |   104.220 ms |   5000.0000 | 4000.0000 | 3000.0000 |  199789672 B |
|  DefaultEcs |     1000000 |              8 |   225.199 ms |   226.311 ms |   5000.0000 | 4000.0000 | 3000.0000 |  334007232 B |
|      HypEcs |      100000 |              1 |    15.279 ms |    15.336 ms |   2000.0000 | 2000.0000 | 2000.0000 |   26445656 B |
|      HypEcs |      100000 |              2 |    29.136 ms |    29.110 ms |   2000.0000 | 2000.0000 | 2000.0000 |   46421208 B |
|      HypEcs |      100000 |              4 |    63.087 ms |    63.046 ms |   6000.0000 | 2000.0000 | 2000.0000 |   98373264 B |
|      HypEcs |      100000 |              8 |   176.880 ms |   176.330 ms |  15000.0000 | 3000.0000 | 2000.0000 |  250283160 B |
|      HypEcs |     1000000 |              1 |   137.851 ms |   138.079 ms |  12000.0000 | 4000.0000 | 4000.0000 |  240457248 B |
|      HypEcs |     1000000 |              2 |   277.651 ms |   277.794 ms |  22000.0000 | 4000.0000 | 4000.0000 |  437042512 B |
|      HypEcs |     1000000 |              4 |   599.436 ms |   599.247 ms |  52000.0000 | 4000.0000 | 4000.0000 |  950214736 B |
|      HypEcs |     1000000 |              8 | 1,749.252 ms | 1,748.354 ms | 140000.0000 | 6000.0000 | 5000.0000 | 2456569360 B |


### DestroyEntity
<details>
	<summary>Environment and runtimes</summary>

```
BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.2728)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.102
  [Host]     : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
  Job-HBPPIA : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
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
|        Arch |      100000 |   447,296.1 μs |   447,635.3 μs |
|        Arch |     1000000 | 2,708,942.9 μs | 2,710,664.4 μs |
|       Checs |      100000 |       264.6 μs |       260.0 μs |
|       Checs |     1000000 |     2,702.2 μs |     2,627.7 μs |
|  DefaultEcs |      100000 |     1,176.9 μs |     1,178.2 μs |
|  DefaultEcs |     1000000 |    22,147.2 μs |    22,108.5 μs |
|      HypEcs |      100000 |     3,406.9 μs |     3,383.8 μs |
|      HypEcs |     1000000 |    32,172.8 μs |    32,099.5 μs |


### AddComponent
<details>
	<summary>Environment and runtimes</summary>

```
BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.2728)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.102
  [Host]     : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
  Job-HBPPIA : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
```
</details>
<details>
	<summary>Tested frameworks</summary>

* [Arch](https://github.com/genaray/Arch)
* [Checs](https://github.com/dn9090/Checs)
* [DefaultEcs](https://github.com/Doraku/DefaultEcs)
</details>

|      Method | entityCount |         Mean |
| ----------- |------------ |------------- |
|        Arch |      100000 |   585.767 ms |
|        Arch |     1000000 | 4,801.389 ms |
|       Checs |      100000 |     7.142 ms |
|       Checs |     1000000 |   284.797 ms |
|  DefaultEcs |      100000 |     1.470 ms |
|  DefaultEcs |     1000000 |    14.741 ms |


### RemoveComponent
<details>
	<summary>Environment and runtimes</summary>

```
BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.2728)
AMD Ryzen 5 5600X, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.102
  [Host]     : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
  Job-HBPPIA : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
```
</details>
<details>
	<summary>Tested frameworks</summary>

* [Arch](https://github.com/genaray/Arch)
* [Checs](https://github.com/dn9090/Checs)
* [DefaultEcs](https://github.com/Doraku/DefaultEcs)
</details>

|      Method | entityCount |       Mean |
| ----------- |------------ |----------- |
|        Arch |      100000 | 857.725 ms |
|        Arch |     1000000 |         NA |
|       Checs |      100000 |   4.990 ms |
|       Checs |     1000000 | 190.684 ms |
|  DefaultEcs |      100000 |   1.040 ms |
|  DefaultEcs |     1000000 |   9.325 ms |


### RunSystem
<details>
	<summary>Environment and runtimes</summary>

```
BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.2728)
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
|        Arch |   772.2 μs |
|       Checs |   784.9 μs |
|  DefaultEcs | 2,245.8 μs |

## Contributing
### Adding a framework
To add a framework to existing tests, copy the test and replace the setup, execution and cleanup methods. 
If the framework is new, also add an entry to the `Categories` class. 
The name of the execution method should match the name of the framework/category.
### Create a new benchmark
...
