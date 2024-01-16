```

BenchmarkDotNet v0.13.10, Pop!_OS 22.04 LTS
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0

```

| Method                       | EnvironmentVariables       |      Mean |    Error |   StdDev |     Gen0 | Allocated |
|------------------------------|----------------------------|----------:|---------:|---------:|---------:|----------:|
| BenchmarkBasicMultiProof1000 | Empty                      | 169.55 ms | 1.408 ms | 1.317 ms | 333.3333 |  20.01 MB |
| BenchmarkVerification1000    | Empty                      |  15.96 ms | 0.100 ms | 0.094 ms |        - |   2.28 MB |
| BenchmarkBasicMultiProof1000 | DOTNET_EnableHWIntrinsic=0 | 176.84 ms | 1.023 ms | 0.957 ms | 333.3333 |  20.01 MB |
| BenchmarkVerification1000    | DOTNET_EnableHWIntrinsic=0 |  16.47 ms | 0.183 ms | 0.171 ms |        - |   2.28 MB |
