``` ini

BenchmarkDotNet=v0.13.2, OS=pop 22.04
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=7.0.302
  [Host]   : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=.NET 7.0  Runtime=.NET 7.0

```

| Method                | EnvironmentVariables       |     Mean |   Error |  StdDev | Ratio | RatioSD | Allocated | Alloc Ratio |
|-----------------------|----------------------------|---------:|--------:|--------:|------:|--------:|----------:|------------:|
| BenchmarkInnerProduct | Empty                      | 399.1 ns | 4.10 ns | 3.84 ns |  1.00 |    0.00 |         - |          NA |
| BenchmarkInnerProduct | DOTNET_EnableHWIntrinsic=0 | 623.6 ns | 4.66 ns | 4.36 ns |  1.56 |    0.02 |         - |          NA |
