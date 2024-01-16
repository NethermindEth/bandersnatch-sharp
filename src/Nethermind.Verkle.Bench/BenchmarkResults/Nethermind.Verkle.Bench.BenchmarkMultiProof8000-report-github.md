```

BenchmarkDotNet v0.13.10, Pop!_OS 22.04 LTS
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0

```

| Method                       | EnvironmentVariables       |      Mean |    Error |   StdDev |      Gen0 |      Gen1 |     Gen2 | Allocated |
|------------------------------|----------------------------|----------:|---------:|---------:|----------:|----------:|---------:|----------:|
| BenchmarkBasicMultiProof8000 | Empty                      | 204.44 ms | 2.015 ms | 1.786 ms | 3000.0000 | 2000.0000 | 500.0000 | 155.06 MB |
| BenchmarkVerification8000    | Empty                      |  75.69 ms | 0.473 ms | 0.442 ms |  142.8571 |  142.8571 | 142.8571 |  10.15 MB |
| BenchmarkBasicMultiProof8000 | DOTNET_EnableHWIntrinsic=0 | 240.63 ms | 4.654 ms | 5.716 ms | 3000.0000 | 2000.0000 | 500.0000 | 155.42 MB |
| BenchmarkVerification8000    | DOTNET_EnableHWIntrinsic=0 |  73.91 ms | 0.750 ms | 0.702 ms |  142.8571 |  142.8571 | 142.8571 |  10.15 MB |
