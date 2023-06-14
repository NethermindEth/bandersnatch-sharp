``` ini

BenchmarkDotNet=v0.13.2, OS=pop 22.04
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=7.0.302
  [Host]   : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=.NET 7.0  Runtime=.NET 7.0  

```
|                       Method |       EnvironmentVariables |      Mean |    Error |   StdDev |    Gen0 |    Gen1 |    Gen2 | Allocated |
|----------------------------- |--------------------------- |----------:|---------:|---------:|--------:|--------:|--------:|----------:|
| BenchmarkBasicMultiProof1000 |                      Empty | 270.92 ms | 3.007 ms | 2.665 ms |       - |       - |       - |   20.7 MB |
|    BenchmarkVerification1000 |                      Empty |  22.80 ms | 0.306 ms | 0.287 ms | 31.2500 | 31.2500 | 31.2500 |   2.44 MB |
| BenchmarkBasicMultiProof1000 | DOTNET_EnableHWIntrinsic=0 | 342.95 ms | 3.375 ms | 3.157 ms |       - |       - |       - |  20.65 MB |
|    BenchmarkVerification1000 | DOTNET_EnableHWIntrinsic=0 |  29.31 ms | 0.270 ms | 0.253 ms | 31.2500 | 31.2500 | 31.2500 |   2.44 MB |
