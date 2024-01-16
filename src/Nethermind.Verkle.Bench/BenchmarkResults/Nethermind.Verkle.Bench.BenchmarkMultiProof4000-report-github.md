```

BenchmarkDotNet v0.13.10, Pop!_OS 22.04 LTS
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0

```

| Method                       | EnvironmentVariables       |      Mean |    Error |   StdDev |      Gen0 |     Gen1 |     Gen2 | Allocated |
|------------------------------|----------------------------|----------:|---------:|---------:|----------:|---------:|---------:|----------:|
| BenchmarkBasicMultiProof4000 | Empty                      | 168.95 ms | 1.217 ms | 1.138 ms | 1333.3333 | 666.6667 |        - |   77.9 MB |
| BenchmarkVerification4000    | Empty                      |  42.08 ms | 0.663 ms | 0.620 ms |  166.6667 | 166.6667 | 166.6667 |   6.41 MB |
| BenchmarkBasicMultiProof4000 | DOTNET_EnableHWIntrinsic=0 | 197.95 ms | 1.176 ms | 1.100 ms | 1333.3333 | 666.6667 |        - |  77.85 MB |
| BenchmarkVerification4000    | DOTNET_EnableHWIntrinsic=0 |  43.18 ms | 0.535 ms | 0.500 ms |  166.6667 | 166.6667 | 166.6667 |    6.4 MB |
