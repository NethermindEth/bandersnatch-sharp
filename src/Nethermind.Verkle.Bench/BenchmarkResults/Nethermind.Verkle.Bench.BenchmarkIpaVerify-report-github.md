```

BenchmarkDotNet v0.13.10, Pop!_OS 22.04 LTS
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0

```

| Method             | EnvironmentVariables       |     Mean |     Error |    StdDev |   Gen0 | Allocated |
|--------------------|----------------------------|---------:|----------:|----------:|-------:|----------:|
| TestBasicIpaVerify | Empty                      | 4.913 ms | 0.0160 ms | 0.0149 ms | 7.8125 | 690.91 KB |
| TestBasicIpaVerify | DOTNET_EnableHWIntrinsic=0 | 5.302 ms | 0.0212 ms | 0.0188 ms | 7.8125 | 690.88 KB |
