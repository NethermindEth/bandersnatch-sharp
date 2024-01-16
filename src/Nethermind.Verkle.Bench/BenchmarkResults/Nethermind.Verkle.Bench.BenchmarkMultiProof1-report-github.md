```

BenchmarkDotNet v0.13.10, Pop!_OS 22.04 LTS
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0

```

| Method                    | EnvironmentVariables       |       Mean |     Error |    StdDev |   Gen0 |  Allocated |
|---------------------------|----------------------------|-----------:|----------:|----------:|-------:|-----------:|
| BenchmarkBasicMultiProof1 | Empty                      | 102.014 ms | 0.3419 ms | 0.3198 ms |      - | 3354.38 KB |
| BenchmarkVerification1    | Empty                      |   5.602 ms | 0.0116 ms | 0.0097 ms | 7.8125 |  793.11 KB |
| BenchmarkBasicMultiProof1 | DOTNET_EnableHWIntrinsic=0 | 114.958 ms | 0.2365 ms | 0.2212 ms |      - |    3356 KB |
| BenchmarkVerification1    | DOTNET_EnableHWIntrinsic=0 |   5.792 ms | 0.0313 ms | 0.0292 ms | 7.8125 |  793.12 KB |
