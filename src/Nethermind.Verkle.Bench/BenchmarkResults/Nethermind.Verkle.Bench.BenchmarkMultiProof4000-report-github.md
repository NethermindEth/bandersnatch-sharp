``` ini

BenchmarkDotNet=v0.13.2, OS=pop 22.04
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=7.0.302
  [Host]   : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=.NET 7.0  Runtime=.NET 7.0

```

| Method                       | EnvironmentVariables       |      Mean |    Error |    StdDev |      Gen0 |     Gen1 |     Gen2 | Allocated |
|------------------------------|----------------------------|----------:|---------:|----------:|----------:|---------:|---------:|----------:|
| BenchmarkBasicMultiProof4000 | Empty                      | 410.33 ms | 7.929 ms |  7.417 ms |         - |        - |        - |  79.75 MB |
| BenchmarkVerification4000    | Empty                      |  65.68 ms | 1.310 ms |  1.703 ms |  125.0000 | 125.0000 | 125.0000 |   7.02 MB |
| BenchmarkBasicMultiProof4000 | DOTNET_EnableHWIntrinsic=0 | 526.31 ms | 9.815 ms | 10.080 ms | 1000.0000 |        - |        - |  80.63 MB |
| BenchmarkVerification4000    | DOTNET_EnableHWIntrinsic=0 |  78.26 ms | 1.186 ms |  1.052 ms |  142.8571 | 142.8571 | 142.8571 |   7.02 MB |
