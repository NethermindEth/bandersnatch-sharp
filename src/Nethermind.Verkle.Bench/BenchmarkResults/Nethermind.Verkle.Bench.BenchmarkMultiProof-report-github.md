``` ini

BenchmarkDotNet=v0.13.2, OS=pop 22.04
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=7.0.302
  [Host]   : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=.NET 7.0  Runtime=.NET 7.0  

```
|                               Method |       EnvironmentVariables |     Mean |   Error |  StdDev | Ratio | RatioSD |  Allocated | Alloc Ratio |
|------------------------------------- |--------------------------- |---------:|--------:|--------:|------:|--------:|-----------:|------------:|
|             BenchmarkBasicMultiProof |                      Empty | 181.3 ms | 1.60 ms | 1.50 ms |  1.00 |    0.00 |  3378.1 KB |        1.00 |
|             BenchmarkBasicMultiProof | DOTNET_EnableHWIntrinsic=0 | 236.7 ms | 2.25 ms | 2.10 ms |  1.31 |    0.02 | 3378.93 KB |        1.00 |
|                                      |                            |          |         |         |       |         |            |             |
| BenchmarkBasicMultiProofVerification |                      Empty | 143.5 ms | 0.71 ms | 0.66 ms |  1.00 |    0.00 |  313.49 KB |        1.00 |
| BenchmarkBasicMultiProofVerification | DOTNET_EnableHWIntrinsic=0 | 185.4 ms | 0.42 ms | 0.37 ms |  1.29 |    0.01 |  313.79 KB |        1.00 |
