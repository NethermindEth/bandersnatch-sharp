``` ini

BenchmarkDotNet=v0.13.2, OS=pop 22.04
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=7.0.302
  [Host]   : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=.NET 7.0  Runtime=.NET 7.0  

```
|                  Method |       EnvironmentVariables |     Mean |     Error |    StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
|------------------------ |--------------------------- |---------:|----------:|----------:|------:|-------:|----------:|------------:|
| BenchmarkMultiScalarMul |                      Empty | 3.098 ms | 0.0145 ms | 0.0136 ms |  1.00 | 7.8125 | 504.21 KB |        1.00 |
| BenchmarkMultiScalarMul | DOTNET_EnableHWIntrinsic=0 | 4.127 ms | 0.0125 ms | 0.0111 ms |  1.33 | 7.8125 | 504.22 KB |        1.00 |
