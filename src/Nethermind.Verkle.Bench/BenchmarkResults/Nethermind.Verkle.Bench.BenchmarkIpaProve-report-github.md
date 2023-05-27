``` ini

BenchmarkDotNet=v0.13.2, OS=pop 22.04
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=7.0.302
  [Host]   : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=.NET 7.0  Runtime=.NET 7.0  

```
|            Method |       EnvironmentVariables |     Mean |   Error |  StdDev | Allocated |
|------------------ |--------------------------- |---------:|--------:|--------:|----------:|
| TestBasicIpaProve |                      Empty | 170.9 ms | 1.64 ms | 1.46 ms |   2.76 MB |
| TestBasicIpaProve | DOTNET_EnableHWIntrinsic=0 | 224.7 ms | 2.21 ms | 1.96 ms |   2.76 MB |
