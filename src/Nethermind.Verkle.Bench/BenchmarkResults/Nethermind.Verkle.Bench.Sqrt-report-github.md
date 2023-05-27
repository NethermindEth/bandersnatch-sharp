``` ini

BenchmarkDotNet=v0.13.2, OS=pop 22.04
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=7.0.302
  [Host]   : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=.NET 7.0  Runtime=.NET 7.0  

```
|       Method |       EnvironmentVariables |                   A |                   B |      Mean |    Error |   StdDev | Allocated |
|------------- |--------------------------- |-------------------- |-------------------- |----------:|---------:|---------:|----------:|
|     **Sqrt_FpE** |                      **Empty** | **(Net(...)935) [121]** | **(Net(...)935) [121]** |  **49.57 μs** | **0.349 μs** | **0.327 μs** |         **-** |
| Sqrt_UInt256 |                      Empty | (Net(...)935) [121] | (Net(...)935) [121] | 463.09 μs | 9.083 μs | 8.052 μs |         - |
|     Sqrt_FpE | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121] | (Net(...)935) [121] |  82.90 μs | 0.188 μs | 0.157 μs |         - |
| Sqrt_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121] | (Net(...)935) [121] | 723.95 μs | 1.946 μs | 1.725 μs |       1 B |
|     **Sqrt_FpE** |                      **Empty** | **(Net(...)935) [121]** | **(Net(...)658) [119]** |  **49.27 μs** | **0.276 μs** | **0.244 μs** |         **-** |
| Sqrt_UInt256 |                      Empty | (Net(...)935) [121] | (Net(...)658) [119] | 467.85 μs | 9.338 μs | 8.734 μs |         - |
|     Sqrt_FpE | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121] | (Net(...)658) [119] |  81.66 μs | 0.330 μs | 0.309 μs |         - |
| Sqrt_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)935) [121] | (Net(...)658) [119] | 723.16 μs | 8.351 μs | 7.812 μs |       1 B |
|     **Sqrt_FpE** |                      **Empty** | **(Net(...)658) [119]** | **(Net(...)935) [121]** |        **NA** |       **NA** |       **NA** |         **-** |
| Sqrt_UInt256 |                      Empty | (Net(...)658) [119] | (Net(...)935) [121] |  47.30 μs | 0.857 μs | 0.802 μs |         - |
|     Sqrt_FpE | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119] | (Net(...)935) [121] |        NA |       NA |       NA |         - |
| Sqrt_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119] | (Net(...)935) [121] |  76.49 μs | 1.069 μs | 1.000 μs |         - |
|     **Sqrt_FpE** |                      **Empty** | **(Net(...)658) [119]** | **(Net(...)658) [119]** |        **NA** |       **NA** |       **NA** |         **-** |
| Sqrt_UInt256 |                      Empty | (Net(...)658) [119] | (Net(...)658) [119] |  48.51 μs | 0.932 μs | 0.957 μs |         - |
|     Sqrt_FpE | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119] | (Net(...)658) [119] |        NA |       NA |       NA |         - |
| Sqrt_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)658) [119] | (Net(...)658) [119] |  75.96 μs | 0.806 μs | 0.715 μs |         - |

Benchmarks with issues:
  Sqrt.Sqrt_FpE: .NET 7.0(Runtime=.NET 7.0) [A=(Net(...)658) [119], B=(Net(...)935) [121]]
  Sqrt.Sqrt_FpE: .NET 7.0(EnvironmentVariables=DOTNET_EnableHWIntrinsic=0, Runtime=.NET 7.0) [A=(Net(...)658) [119], B=(Net(...)935) [121]]
  Sqrt.Sqrt_FpE: .NET 7.0(Runtime=.NET 7.0) [A=(Net(...)658) [119], B=(Net(...)658) [119]]
  Sqrt.Sqrt_FpE: .NET 7.0(EnvironmentVariables=DOTNET_EnableHWIntrinsic=0, Runtime=.NET 7.0) [A=(Net(...)658) [119], B=(Net(...)658) [119]]
