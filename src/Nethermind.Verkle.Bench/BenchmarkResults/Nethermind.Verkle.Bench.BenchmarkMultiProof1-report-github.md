``` ini

BenchmarkDotNet=v0.13.2, OS=pop 22.04
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=7.0.302
  [Host]   : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=.NET 7.0  Runtime=.NET 7.0  

```
|                    Method |       EnvironmentVariables |       Mean |     Error |    StdDev |  Allocated |
|-------------------------- |--------------------------- |-----------:|----------:|----------:|-----------:|
| BenchmarkBasicMultiProof1 |                      Empty | 184.576 ms | 1.3973 ms | 1.3071 ms |  3393.7 KB |
|    BenchmarkVerification1 |                      Empty |   9.627 ms | 0.1909 ms | 0.5509 ms |  792.45 KB |
| BenchmarkBasicMultiProof1 | DOTNET_EnableHWIntrinsic=0 | 233.458 ms | 1.9130 ms | 1.7894 ms | 3396.52 KB |
|    BenchmarkVerification1 | DOTNET_EnableHWIntrinsic=0 |  12.260 ms | 0.2875 ms | 0.8477 ms |  792.69 KB |
