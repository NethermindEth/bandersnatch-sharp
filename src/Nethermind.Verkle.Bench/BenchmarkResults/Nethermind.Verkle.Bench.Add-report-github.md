``` ini

BenchmarkDotNet=v0.13.2, OS=pop 22.04
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=7.0.302
  [Host]   : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=.NET 7.0  Runtime=.NET 7.0

```

| Method      | EnvironmentVariables       | A                       | B                       |         Mean |         Error |        StdDev |    Ratio |  RatioSD | Allocated | Alloc Ratio |
|-------------|----------------------------|-------------------------|-------------------------|-------------:|--------------:|--------------:|---------:|---------:|----------:|------------:|
| **Add_FpE** | **Empty**                  | **(Net(...)935) [121]** | **(Net(...)935) [121]** | **5.484 ns** | **0.0348 ns** | **0.0309 ns** | **1.00** | **0.00** |     **-** |      **NA** |
| Add_UInt256 | Empty                      | (Net(...)935) [121]     | (Net(...)935) [121]     |    56.569 ns |     0.2496 ns |     0.2212 ns |    10.32 |     0.06 |         - |          NA |
| Add_FpE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121]     | (Net(...)935) [121]     |     8.813 ns |     0.0531 ns |     0.0497 ns |     1.61 |     0.01 |         - |          NA |
| Add_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121]     | (Net(...)935) [121]     |    75.437 ns |     0.7964 ns |     0.7450 ns |    13.77 |     0.14 |         - |          NA |
|             |                            |                         |                         |              |               |               |          |          |           |             |
| **Add_FpE** | **Empty**                  | **(Net(...)935) [121]** | **(Net(...)658) [119]** | **5.464 ns** | **0.0346 ns** | **0.0307 ns** | **1.00** | **0.00** |     **-** |      **NA** |
| Add_UInt256 | Empty                      | (Net(...)935) [121]     | (Net(...)658) [119]     |    56.174 ns |     0.3372 ns |     0.3154 ns |    10.28 |     0.07 |         - |          NA |
| Add_FpE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121]     | (Net(...)658) [119]     |     8.920 ns |     0.0535 ns |     0.0500 ns |     1.63 |     0.01 |         - |          NA |
| Add_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121]     | (Net(...)658) [119]     |    76.863 ns |     0.2359 ns |     0.2091 ns |    14.07 |     0.08 |         - |          NA |
|             |                            |                         |                         |              |               |               |          |          |           |             |
| **Add_FpE** | **Empty**                  | **(Net(...)658) [119]** | **(Net(...)935) [121]** | **5.348 ns** | **0.0413 ns** | **0.0366 ns** | **1.00** | **0.00** |     **-** |      **NA** |
| Add_UInt256 | Empty                      | (Net(...)658) [119]     | (Net(...)935) [121]     |    56.101 ns |     0.4494 ns |     0.4203 ns |    10.49 |     0.13 |         - |          NA |
| Add_FpE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119]     | (Net(...)935) [121]     |     8.858 ns |     0.0380 ns |     0.0356 ns |     1.66 |     0.01 |         - |          NA |
| Add_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119]     | (Net(...)935) [121]     |    75.097 ns |     0.1854 ns |     0.1644 ns |    14.04 |     0.10 |         - |          NA |
|             |                            |                         |                         |              |               |               |          |          |           |             |
| **Add_FpE** | **Empty**                  | **(Net(...)658) [119]** | **(Net(...)658) [119]** | **5.062 ns** | **0.0777 ns** | **0.0727 ns** | **1.00** | **0.00** |     **-** |      **NA** |
| Add_UInt256 | Empty                      | (Net(...)658) [119]     | (Net(...)658) [119]     |    23.274 ns |     0.0815 ns |     0.0763 ns |     4.60 |     0.08 |         - |          NA |
| Add_FpE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119]     | (Net(...)658) [119]     |     8.804 ns |     0.0657 ns |     0.0582 ns |     1.74 |     0.03 |         - |          NA |
| Add_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119]     | (Net(...)658) [119]     |     9.067 ns |     0.0289 ns |     0.0271 ns |     1.79 |     0.03 |         - |          NA |
