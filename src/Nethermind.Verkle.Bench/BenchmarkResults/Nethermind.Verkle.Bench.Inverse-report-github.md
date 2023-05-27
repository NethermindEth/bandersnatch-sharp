``` ini

BenchmarkDotNet=v0.13.2, OS=pop 22.04
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=7.0.302
  [Host]   : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=.NET 7.0  Runtime=.NET 7.0  

```
|          Method |       EnvironmentVariables |                   A |                   B |     Mean |    Error |   StdDev |   Median | Ratio | RatioSD | Allocated | Alloc Ratio |
|---------------- |--------------------------- |-------------------- |-------------------- |---------:|---------:|---------:|---------:|------:|--------:|----------:|------------:|
|     **Inverse_FpE** |                      **Empty** | **(Net(...)935) [121]** | **(Net(...)935) [121]** | **38.88 μs** | **0.055 μs** | **0.052 μs** | **38.88 μs** |  **1.00** |    **0.00** |         **-** |          **NA** |
| Inverse_UInt256 |                      Empty | (Net(...)935) [121] | (Net(...)935) [121] | 51.56 μs | 1.005 μs | 1.196 μs | 52.42 μs |  1.32 |    0.03 |         - |          NA |
|     Inverse_FpE | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121] | (Net(...)935) [121] | 59.95 μs | 0.120 μs | 0.100 μs | 59.95 μs |  1.54 |    0.00 |         - |          NA |
| Inverse_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121] | (Net(...)935) [121] | 82.05 μs | 1.104 μs | 1.032 μs | 82.02 μs |  2.11 |    0.03 |         - |          NA |
|                 |                            |                     |                     |          |          |          |          |       |         |           |             |
|     **Inverse_FpE** |                      **Empty** | **(Net(...)935) [121]** | **(Net(...)658) [119]** | **28.84 μs** | **0.081 μs** | **0.076 μs** | **28.85 μs** |  **1.00** |    **0.00** |         **-** |          **NA** |
| Inverse_UInt256 |                      Empty | (Net(...)935) [121] | (Net(...)658) [119] | 52.15 μs | 0.706 μs | 0.660 μs | 52.14 μs |  1.81 |    0.02 |         - |          NA |
|     Inverse_FpE | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121] | (Net(...)658) [119] | 45.22 μs | 0.303 μs | 0.283 μs | 45.22 μs |  1.57 |    0.01 |         - |          NA |
| Inverse_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121] | (Net(...)658) [119] | 81.63 μs | 0.807 μs | 0.716 μs | 81.30 μs |  2.83 |    0.03 |         - |          NA |
|                 |                            |                     |                     |          |          |          |          |       |         |           |             |
|     **Inverse_FpE** |                      **Empty** | **(Net(...)658) [119]** | **(Net(...)935) [121]** | **39.86 μs** | **0.419 μs** | **0.392 μs** | **39.95 μs** |  **1.00** |    **0.00** |         **-** |          **NA** |
| Inverse_UInt256 |                      Empty | (Net(...)658) [119] | (Net(...)935) [121] | 52.24 μs | 0.559 μs | 0.437 μs | 52.28 μs |  1.31 |    0.02 |         - |          NA |
|     Inverse_FpE | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119] | (Net(...)935) [121] | 62.65 μs | 0.651 μs | 0.577 μs | 62.57 μs |  1.57 |    0.02 |         - |          NA |
| Inverse_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119] | (Net(...)935) [121] | 83.87 μs | 1.495 μs | 1.398 μs | 83.58 μs |  2.10 |    0.04 |         - |          NA |
|                 |                            |                     |                     |          |          |          |          |       |         |           |             |
|     **Inverse_FpE** |                      **Empty** | **(Net(...)658) [119]** | **(Net(...)658) [119]** | **29.04 μs** | **0.170 μs** | **0.151 μs** | **28.98 μs** |  **1.00** |    **0.00** |         **-** |          **NA** |
| Inverse_UInt256 |                      Empty | (Net(...)658) [119] | (Net(...)658) [119] | 51.43 μs | 1.004 μs | 1.340 μs | 50.83 μs |  1.80 |    0.04 |         - |          NA |
|     Inverse_FpE | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119] | (Net(...)658) [119] | 45.55 μs | 0.207 μs | 0.183 μs | 45.61 μs |  1.57 |    0.01 |         - |          NA |
| Inverse_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119] | (Net(...)658) [119] | 80.37 μs | 0.067 μs | 0.053 μs | 80.35 μs |  2.77 |    0.02 |         - |          NA |