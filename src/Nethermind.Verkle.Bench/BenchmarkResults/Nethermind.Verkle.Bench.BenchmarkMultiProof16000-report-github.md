```

BenchmarkDotNet v0.13.10, Pop!_OS 22.04 LTS
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0

```

| Method                        | EnvironmentVariables       |     Mean |   Error |  StdDev |      Gen0 |      Gen1 |     Gen2 | Allocated |
|-------------------------------|----------------------------|---------:|--------:|--------:|----------:|----------:|---------:|----------:|
| BenchmarkBasicMultiProof16000 | Empty                      | 275.6 ms | 3.99 ms | 3.73 ms | 6000.0000 | 4500.0000 | 500.0000 | 308.57 MB |
| BenchmarkVerification16000    | Empty                      | 120.9 ms | 0.49 ms | 0.41 ms |         - |         - |        - |  17.95 MB |
| BenchmarkBasicMultiProof16000 | DOTNET_EnableHWIntrinsic=0 | 301.8 ms | 5.93 ms | 5.55 ms | 6000.0000 | 4500.0000 | 500.0000 | 308.95 MB |
| BenchmarkVerification16000    | DOTNET_EnableHWIntrinsic=0 | 118.4 ms | 0.95 ms | 0.88 ms |         - |         - |        - |  17.95 MB |
