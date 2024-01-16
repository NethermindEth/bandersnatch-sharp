```

BenchmarkDotNet v0.13.10, Pop!_OS 22.04 LTS
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0

```

| Method                | EnvironmentVariables       |     Mean |     Error |    StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
|-----------------------|----------------------------|---------:|----------:|----------:|------:|-------:|----------:|------------:|
| BenchmarkInnerProduct | Empty                      | 1.241 μs | 0.0013 μs | 0.0011 μs |  1.00 | 0.0038 |     368 B |        1.00 |
| BenchmarkInnerProduct | DOTNET_EnableHWIntrinsic=0 | 1.298 μs | 0.0012 μs | 0.0010 μs |  1.05 | 0.0038 |     368 B |        1.00 |
