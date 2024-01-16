```

BenchmarkDotNet v0.13.10, Pop!_OS 22.04 LTS
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0

```

| Method           | EnvironmentVariables       | A                   | B                   |     Mean |     Error |    StdDev | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------|----------------------------|---------------------|---------------------|---------:|----------:|----------:|------:|--------:|----------:|------------:|
| Subtract_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 2.188 ns | 0.0045 ns | 0.0037 ns |  1.00 |    0.00 |         - |          NA |
| Subtract_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 2.159 ns | 0.0040 ns | 0.0033 ns |  0.99 |    0.00 |         - |          NA |
| Subtract_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 2.169 ns | 0.0031 ns | 0.0026 ns |  0.99 |    0.00 |         - |          NA |
| Subtract_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 2.156 ns | 0.0013 ns | 0.0012 ns |  0.99 |    0.00 |         - |          NA |
| Subtract_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 3.939 ns | 0.0223 ns | 0.0208 ns |  1.80 |    0.01 |         - |          NA |
| Subtract_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 4.014 ns | 0.0023 ns | 0.0019 ns |  1.83 |    0.00 |         - |          NA |
| Subtract_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 4.074 ns | 0.0090 ns | 0.0080 ns |  1.86 |    0.00 |         - |          NA |
| Subtract_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 4.003 ns | 0.0204 ns | 0.0181 ns |  1.83 |    0.01 |         - |          NA |
| Subtract_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 6.227 ns | 0.0228 ns | 0.0214 ns |  2.85 |    0.01 |         - |          NA |
| Subtract_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 6.202 ns | 0.0054 ns | 0.0051 ns |  2.83 |    0.01 |         - |          NA |
| Subtract_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 6.222 ns | 0.0187 ns | 0.0175 ns |  2.85 |    0.01 |         - |          NA |
| Subtract_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 6.255 ns | 0.0018 ns | 0.0016 ns |  2.86 |    0.00 |         - |          NA |
| Subtract_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 7.428 ns | 0.0071 ns | 0.0067 ns |  3.39 |    0.01 |         - |          NA |
| Subtract_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 7.804 ns | 0.0374 ns | 0.0349 ns |  3.57 |    0.02 |         - |          NA |
| Subtract_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 7.539 ns | 0.0041 ns | 0.0034 ns |  3.45 |    0.01 |         - |          NA |
| Subtract_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 7.541 ns | 0.0090 ns | 0.0084 ns |  3.45 |    0.01 |         - |          NA |
