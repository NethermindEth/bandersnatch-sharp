``` ini

BenchmarkDotNet=v0.13.2, OS=pop 22.04
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=7.0.302
  [Host]   : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=.NET 7.0  Runtime=.NET 7.0

```

| Method           | EnvironmentVariables       | A                       | B                       |         Mean |         Error |        StdDev |    Ratio |  RatioSD | Allocated | Alloc Ratio |
|------------------|----------------------------|-------------------------|-------------------------|-------------:|--------------:|--------------:|---------:|---------:|----------:|------------:|
| **Subtract_FpE** | **Empty**                  | **(Net(...)935) [121]** | **(Net(...)935) [121]** | **2.189 ns** | **0.0228 ns** | **0.0214 ns** | **1.00** | **0.00** |     **-** |      **NA** |
| Subtract_UInt256 | Empty                      | (Net(...)935) [121]     | (Net(...)935) [121]     |     4.229 ns |     0.0326 ns |     0.0305 ns |     1.93 |     0.02 |         - |          NA |
| Subtract_FpE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121]     | (Net(...)935) [121]     |     6.331 ns |     0.0242 ns |     0.0226 ns |     2.89 |     0.02 |         - |          NA |
| Subtract_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121]     | (Net(...)935) [121]     |     5.292 ns |     0.0125 ns |     0.0105 ns |     2.41 |     0.03 |         - |          NA |
|                  |                            |                         |                         |              |               |               |          |          |           |             |
| **Subtract_FpE** | **Empty**                  | **(Net(...)935) [121]** | **(Net(...)658) [119]** | **4.534 ns** | **0.0116 ns** | **0.0103 ns** | **1.00** | **0.00** |     **-** |      **NA** |
| Subtract_UInt256 | Empty                      | (Net(...)935) [121]     | (Net(...)658) [119]     |    48.007 ns |     0.1585 ns |     0.1483 ns |    10.59 |     0.04 |         - |          NA |
| Subtract_FpE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121]     | (Net(...)658) [119]     |     7.957 ns |     0.0238 ns |     0.0199 ns |     1.75 |     0.01 |         - |          NA |
| Subtract_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121]     | (Net(...)658) [119]     |    56.306 ns |     0.1749 ns |     0.1636 ns |    12.42 |     0.05 |         - |          NA |
|                  |                            |                         |                         |              |               |               |          |          |           |             |
| **Subtract_FpE** | **Empty**                  | **(Net(...)658) [119]** | **(Net(...)935) [121]** | **2.212 ns** | **0.0037 ns** | **0.0034 ns** | **1.00** | **0.00** |     **-** |      **NA** |
| Subtract_UInt256 | Empty                      | (Net(...)658) [119]     | (Net(...)935) [121]     |    56.897 ns |     0.7511 ns |     0.7026 ns |    25.72 |     0.31 |         - |          NA |
| Subtract_FpE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119]     | (Net(...)935) [121]     |     6.365 ns |     0.0061 ns |     0.0057 ns |     2.88 |     0.00 |         - |          NA |
| Subtract_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119]     | (Net(...)935) [121]     |    62.624 ns |     0.1901 ns |     0.1779 ns |    28.31 |     0.09 |         - |          NA |
|                  |                            |                         |                         |              |               |               |          |          |           |             |
| **Subtract_FpE** | **Empty**                  | **(Net(...)658) [119]** | **(Net(...)658) [119]** | **2.199 ns** | **0.0037 ns** | **0.0035 ns** | **1.00** | **0.00** |     **-** |      **NA** |
| Subtract_UInt256 | Empty                      | (Net(...)658) [119]     | (Net(...)658) [119]     |     4.272 ns |     0.0354 ns |     0.0331 ns |     1.94 |     0.02 |         - |          NA |
| Subtract_FpE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119]     | (Net(...)658) [119]     |     6.314 ns |     0.0242 ns |     0.0227 ns |     2.87 |     0.01 |         - |          NA |
| Subtract_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119]     | (Net(...)658) [119]     |     5.304 ns |     0.0514 ns |     0.0455 ns |     2.41 |     0.02 |         - |          NA |
