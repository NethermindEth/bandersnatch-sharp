```

BenchmarkDotNet v0.13.10, Pop!_OS 22.04 LTS
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0

```

| Method           | EnvironmentVariables       | A                   | B                   |      Mean |    Error |   StdDev | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------|----------------------------|---------------------|---------------------|----------:|---------:|---------:|------:|--------:|----------:|------------:|
| Multiply_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] |  74.20 ns | 0.528 ns | 0.494 ns |  1.00 |    0.00 |         - |          NA |
| Multiply_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] |  79.60 ns | 0.488 ns | 0.456 ns |  1.07 |    0.01 |         - |          NA |
| Multiply_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] |  79.54 ns | 0.437 ns | 0.409 ns |  1.07 |    0.01 |         - |          NA |
| Multiply_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] |  82.15 ns | 0.380 ns | 0.356 ns |  1.11 |    0.01 |         - |          NA |
| Multiply_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 117.36 ns | 1.447 ns | 1.354 ns |  1.58 |    0.02 |         - |          NA |
| Multiply_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 111.26 ns | 0.509 ns | 0.476 ns |  1.50 |    0.01 |         - |          NA |
| Multiply_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 116.14 ns | 0.183 ns | 0.171 ns |  1.57 |    0.01 |         - |          NA |
| Multiply_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 124.27 ns | 0.512 ns | 0.454 ns |  1.68 |    0.01 |         - |          NA |
| Multiply_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] |  89.66 ns | 1.780 ns | 2.251 ns |  1.21 |    0.03 |         - |          NA |
| Multiply_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] |  79.66 ns | 0.413 ns | 0.366 ns |  1.07 |    0.01 |         - |          NA |
| Multiply_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] |  79.65 ns | 0.613 ns | 0.573 ns |  1.07 |    0.01 |         - |          NA |
| Multiply_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] |  80.21 ns | 0.385 ns | 0.341 ns |  1.08 |    0.01 |         - |          NA |
| Multiply_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 188.13 ns | 0.536 ns | 0.475 ns |  2.54 |    0.02 |         - |          NA |
| Multiply_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 187.22 ns | 0.405 ns | 0.379 ns |  2.52 |    0.02 |         - |          NA |
| Multiply_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 185.08 ns | 0.512 ns | 0.479 ns |  2.49 |    0.02 |         - |          NA |
| Multiply_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 189.64 ns | 0.931 ns | 0.825 ns |  2.56 |    0.02 |         - |          NA |
