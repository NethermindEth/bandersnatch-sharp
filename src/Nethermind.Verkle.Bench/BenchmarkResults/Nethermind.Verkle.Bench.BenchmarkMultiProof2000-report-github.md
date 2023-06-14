``` ini

BenchmarkDotNet=v0.13.2, OS=pop 22.04
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=7.0.302
  [Host]   : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=.NET 7.0  Runtime=.NET 7.0  

```
|                       Method |       EnvironmentVariables |      Mean |    Error |   StdDev |     Gen0 |    Gen1 |    Gen2 | Allocated |
|----------------------------- |--------------------------- |----------:|---------:|---------:|---------:|--------:|--------:|----------:|
| BenchmarkBasicMultiProof2000 |                      Empty | 317.55 ms | 2.621 ms | 2.452 ms | 500.0000 |       - |       - |   40.7 MB |
|    BenchmarkVerification2000 |                      Empty |  36.56 ms | 0.531 ms | 0.497 ms |  71.4286 | 71.4286 | 71.4286 |   4.01 MB |
| BenchmarkBasicMultiProof2000 | DOTNET_EnableHWIntrinsic=0 | 403.78 ms | 3.098 ms | 2.898 ms |        - |       - |       - |  40.53 MB |
|    BenchmarkVerification2000 | DOTNET_EnableHWIntrinsic=0 |  47.02 ms | 0.550 ms | 0.488 ms |        - |       - |       - |   4.01 MB |
