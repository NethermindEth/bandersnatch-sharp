```

BenchmarkDotNet v0.13.10, Pop!_OS 22.04 LTS
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0

```

| Method                  | EnvironmentVariables       |     Mean |     Error |    StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
|-------------------------|----------------------------|---------:|----------:|----------:|------:|-------:|----------:|------------:|
| BenchmarkMultiScalarMul | Empty                      | 2.205 ms | 0.0067 ms | 0.0063 ms |  1.00 | 7.8125 | 504.26 KB |        1.00 |
| BenchmarkMultiScalarMul | DOTNET_EnableHWIntrinsic=0 | 2.175 ms | 0.0062 ms | 0.0052 ms |  0.99 | 7.8125 | 504.26 KB |        1.00 |
