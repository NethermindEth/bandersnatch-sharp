```

BenchmarkDotNet v0.13.10, Pop!_OS 22.04 LTS
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0

```

| Method          | EnvironmentVariables       | A                   | B                   |     Mean |    Error |   StdDev | Ratio | Allocated | Alloc Ratio |
|-----------------|----------------------------|---------------------|---------------------|---------:|---------:|---------:|------:|----------:|------------:|
| Inverse_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 29.26 μs | 0.309 μs | 0.289 μs |  1.00 |         - |          NA |
| Inverse_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 30.51 μs | 0.147 μs | 0.137 μs |  1.04 |         - |          NA |
| Inverse_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 30.74 μs | 0.205 μs | 0.192 μs |  1.05 |         - |          NA |
| Inverse_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 30.30 μs | 0.105 μs | 0.098 μs |  1.04 |         - |          NA |
| Inverse_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 10.56 μs | 0.116 μs | 0.109 μs |  0.36 |         - |          NA |
| Inverse_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 10.75 μs | 0.039 μs | 0.037 μs |  0.37 |         - |          NA |
| Inverse_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 15.21 μs | 0.065 μs | 0.060 μs |  0.52 |         - |          NA |
| Inverse_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 10.68 μs | 0.021 μs | 0.020 μs |  0.37 |         - |          NA |
| Inverse_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 30.56 μs | 0.192 μs | 0.180 μs |  1.04 |         - |          NA |
| Inverse_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 29.78 μs | 0.168 μs | 0.157 μs |  1.02 |         - |          NA |
| Inverse_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 29.49 μs | 0.066 μs | 0.062 μs |  1.01 |         - |          NA |
| Inverse_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 30.16 μs | 0.149 μs | 0.139 μs |  1.03 |         - |          NA |
| Inverse_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 23.88 μs | 0.038 μs | 0.035 μs |  0.82 |         - |          NA |
| Inverse_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 23.80 μs | 0.015 μs | 0.012 μs |  0.81 |         - |          NA |
| Inverse_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 23.90 μs | 0.028 μs | 0.026 μs |  0.82 |         - |          NA |
| Inverse_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 23.53 μs | 0.030 μs | 0.028 μs |  0.80 |         - |          NA |
