```

BenchmarkDotNet v0.13.10, Pop!_OS 22.04 LTS
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0

```

| Method                       | EnvironmentVariables       |      Mean |    Error |   StdDev |     Gen0 |     Gen1 |    Gen2 | Allocated |
|------------------------------|----------------------------|----------:|---------:|---------:|---------:|---------:|--------:|----------:|
| BenchmarkBasicMultiProof2000 | Empty                      | 147.12 ms | 0.648 ms | 0.574 ms | 500.0000 | 250.0000 |       - |  39.22 MB |
| BenchmarkVerification2000    | Empty                      |  25.43 ms | 0.226 ms | 0.211 ms |  62.5000 |  31.2500 | 31.2500 |    3.7 MB |
| BenchmarkBasicMultiProof2000 | DOTNET_EnableHWIntrinsic=0 | 180.70 ms | 0.485 ms | 0.405 ms | 666.6667 | 333.3333 |       - |   39.2 MB |
| BenchmarkVerification2000    | DOTNET_EnableHWIntrinsic=0 |  25.93 ms | 0.120 ms | 0.113 ms |  62.5000 |  31.2500 | 31.2500 |    3.7 MB |
