``` ini

BenchmarkDotNet=v0.13.2, OS=pop 22.04
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=7.0.302
  [Host]   : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=.NET 7.0  Runtime=.NET 7.0

```

| Method                       | EnvironmentVariables       |     Mean |    Error |   StdDev |      Gen0 |      Gen1 | Allocated |
|------------------------------|----------------------------|---------:|---------:|---------:|----------:|----------:|----------:|
| BenchmarkBasicMultiProof8000 | Empty                      | 603.0 ms | 11.55 ms | 15.42 ms | 2000.0000 | 1000.0000 | 160.78 MB |
| BenchmarkVerification8000    | Empty                      | 104.2 ms |  1.78 ms |  1.66 ms |         - |         - |  10.87 MB |
| BenchmarkBasicMultiProof8000 | DOTNET_EnableHWIntrinsic=0 | 741.7 ms | 14.58 ms | 14.97 ms | 2000.0000 | 1000.0000 |  161.3 MB |
| BenchmarkVerification8000    | DOTNET_EnableHWIntrinsic=0 | 126.8 ms |  1.29 ms |  1.14 ms |         - |         - |  10.87 MB |
