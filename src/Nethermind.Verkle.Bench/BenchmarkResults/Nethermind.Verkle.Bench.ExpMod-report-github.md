```

BenchmarkDotNet v0.13.10, Pop!_OS 22.04 LTS
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0

```

| Method         | EnvironmentVariables       | A                   | B                   |     Mean |    Error |   StdDev | Ratio | Allocated | Alloc Ratio |
|----------------|----------------------------|---------------------|---------------------|---------:|---------:|---------:|------:|----------:|------------:|
| ExpMod_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 29.68 μs | 0.214 μs | 0.200 μs |  1.00 |         - |          NA |
| ExpMod_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 30.13 μs | 0.090 μs | 0.084 μs |  1.01 |         - |          NA |
| ExpMod_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 29.80 μs | 0.132 μs | 0.123 μs |  1.00 |         - |          NA |
| ExpMod_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 30.58 μs | 0.101 μs | 0.095 μs |  1.03 |         - |          NA |
| ExpMod_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 10.07 μs | 0.014 μs | 0.013 μs |  0.34 |         - |          NA |
| ExpMod_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 10.44 μs | 0.011 μs | 0.011 μs |  0.35 |         - |          NA |
| ExpMod_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 10.60 μs | 0.034 μs | 0.031 μs |  0.36 |         - |          NA |
| ExpMod_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 10.44 μs | 0.022 μs | 0.018 μs |  0.35 |         - |          NA |
| ExpMod_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 30.70 μs | 0.150 μs | 0.140 μs |  1.03 |         - |          NA |
| ExpMod_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 30.97 μs | 0.089 μs | 0.083 μs |  1.04 |         - |          NA |
| ExpMod_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 30.15 μs | 0.129 μs | 0.121 μs |  1.02 |         - |          NA |
| ExpMod_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 30.18 μs | 0.072 μs | 0.067 μs |  1.02 |         - |          NA |
| ExpMod_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 23.32 μs | 0.019 μs | 0.018 μs |  0.79 |         - |          NA |
| ExpMod_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 23.80 μs | 0.042 μs | 0.037 μs |  0.80 |         - |          NA |
| ExpMod_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 23.81 μs | 0.064 μs | 0.060 μs |  0.80 |         - |          NA |
| ExpMod_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 23.65 μs | 0.017 μs | 0.016 μs |  0.80 |         - |          NA |
