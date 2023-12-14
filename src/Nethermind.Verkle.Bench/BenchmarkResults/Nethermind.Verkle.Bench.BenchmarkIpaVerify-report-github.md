``` ini

BenchmarkDotNet=v0.13.2, OS=pop 22.04
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=7.0.302
  [Host]   : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=.NET 7.0  Runtime=.NET 7.0

```

| Method             | EnvironmentVariables       |     Mean |   Error |  StdDev | Allocated |
|--------------------|----------------------------|---------:|--------:|--------:|----------:|
| TestBasicIpaVerify | Empty                      | 144.8 ms | 1.19 ms | 1.11 ms | 210.26 KB |
| TestBasicIpaVerify | DOTNET_EnableHWIntrinsic=0 | 185.0 ms | 0.84 ms | 0.79 ms | 210.46 KB |
