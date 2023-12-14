``` ini

BenchmarkDotNet=v0.13.2, OS=pop 22.04
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=7.0.302
  [Host]   : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=.NET 7.0  Runtime=.NET 7.0

```

| Method           | EnvironmentVariables       | A                       | B                       |         Mean |        Error |       StdDev |    Ratio |  RatioSD | Allocated | Alloc Ratio |
|------------------|----------------------------|-------------------------|-------------------------|-------------:|-------------:|-------------:|---------:|---------:|----------:|------------:|
| **Multiply_FpE** | **Empty**                  | **(Net(...)935) [121]** | **(Net(...)935) [121]** | **72.28 ns** | **0.427 ns** | **0.357 ns** | **1.00** | **0.00** |     **-** |      **NA** |
| Multiply_UInt256 | Empty                      | (Net(...)935) [121]     | (Net(...)935) [121]     |    126.60 ns |     1.599 ns |     1.418 ns |     1.75 |     0.02 |         - |          NA |
| Multiply_FpE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121]     | (Net(...)935) [121]     |    125.46 ns |     2.500 ns |     3.070 ns |     1.71 |     0.03 |         - |          NA |
| Multiply_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121]     | (Net(...)935) [121]     |    188.51 ns |     1.346 ns |     1.259 ns |     2.61 |     0.02 |         - |          NA |
|                  |                            |                         |                         |              |              |              |          |          |           |             |
| **Multiply_FpE** | **Empty**                  | **(Net(...)935) [121]** | **(Net(...)658) [119]** | **63.18 ns** | **0.369 ns** | **0.288 ns** | **1.00** | **0.00** |     **-** |      **NA** |
| Multiply_UInt256 | Empty                      | (Net(...)935) [121]     | (Net(...)658) [119]     |    118.17 ns |     0.706 ns |     0.590 ns |     1.87 |     0.01 |         - |          NA |
| Multiply_FpE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121]     | (Net(...)658) [119]     |    110.73 ns |     1.794 ns |     1.678 ns |     1.75 |     0.03 |         - |          NA |
| Multiply_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121]     | (Net(...)658) [119]     |    188.07 ns |     0.723 ns |     0.677 ns |     2.98 |     0.01 |         - |          NA |
|                  |                            |                         |                         |              |              |              |          |          |           |             |
| **Multiply_FpE** | **Empty**                  | **(Net(...)658) [119]** | **(Net(...)935) [121]** | **65.82 ns** | **0.611 ns** | **0.572 ns** | **1.00** | **0.00** |     **-** |      **NA** |
| Multiply_UInt256 | Empty                      | (Net(...)658) [119]     | (Net(...)935) [121]     |    120.94 ns |     0.640 ns |     0.599 ns |     1.84 |     0.02 |         - |          NA |
| Multiply_FpE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119]     | (Net(...)935) [121]     |    107.22 ns |     0.496 ns |     0.464 ns |     1.63 |     0.01 |         - |          NA |
| Multiply_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119]     | (Net(...)935) [121]     |    189.24 ns |     1.067 ns |     0.998 ns |     2.88 |     0.04 |         - |          NA |
|                  |                            |                         |                         |              |              |              |          |          |           |             |
| **Multiply_FpE** | **Empty**                  | **(Net(...)658) [119]** | **(Net(...)658) [119]** | **66.65 ns** | **0.135 ns** | **0.113 ns** | **1.00** | **0.00** |     **-** |      **NA** |
| Multiply_UInt256 | Empty                      | (Net(...)658) [119]     | (Net(...)658) [119]     |    118.62 ns |     0.233 ns |     0.195 ns |     1.78 |     0.00 |         - |          NA |
| Multiply_FpE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119]     | (Net(...)658) [119]     |    108.33 ns |     0.501 ns |     0.468 ns |     1.63 |     0.01 |         - |          NA |
| Multiply_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119]     | (Net(...)658) [119]     |    187.29 ns |     0.767 ns |     0.680 ns |     2.81 |     0.01 |         - |          NA |
