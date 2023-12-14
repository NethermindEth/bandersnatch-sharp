``` ini

BenchmarkDotNet=v0.13.2, OS=pop 22.04
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=7.0.302
  [Host]   : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=.NET 7.0  Runtime=.NET 7.0

```

| Method                        | EnvironmentVariables       |       Mean |    Error |   StdDev |      Gen0 |      Gen1 | Allocated |
|-------------------------------|----------------------------|-----------:|---------:|---------:|----------:|----------:|----------:|
| BenchmarkBasicMultiProof16000 | Empty                      |   945.3 ms | 16.55 ms | 20.32 ms | 3000.0000 |         - | 319.47 MB |
| BenchmarkVerification16000    | Empty                      |   165.5 ms |  2.36 ms |  2.20 ms |         - |         - |  20.45 MB |
| BenchmarkBasicMultiProof16000 | DOTNET_EnableHWIntrinsic=0 | 1,187.9 ms | 22.84 ms | 22.43 ms | 4000.0000 | 2000.0000 | 322.25 MB |
| BenchmarkVerification16000    | DOTNET_EnableHWIntrinsic=0 |   206.8 ms |  1.62 ms |  1.26 ms |         - |         - |  20.45 MB |
