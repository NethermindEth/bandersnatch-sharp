```

BenchmarkDotNet v0.13.10, Pop!_OS 22.04 LTS
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0

```

| Method      | EnvironmentVariables       | A                   | B                   |      Mean |     Error |    StdDev | Ratio | RatioSD | Allocated | Alloc Ratio |
|-------------|----------------------------|---------------------|---------------------|----------:|----------:|----------:|------:|--------:|----------:|------------:|
| Add_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] |  5.039 ns | 0.0371 ns | 0.0347 ns |  1.00 |    0.00 |         - |          NA |
| Add_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] |  5.238 ns | 0.0385 ns | 0.0360 ns |  1.04 |    0.01 |         - |          NA |
| Add_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] |  4.859 ns | 0.0616 ns | 0.0546 ns |  0.96 |    0.01 |         - |          NA |
| Add_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] |  4.756 ns | 0.0485 ns | 0.0453 ns |  0.94 |    0.01 |         - |          NA |
| Add_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 40.870 ns | 0.0778 ns | 0.0728 ns |  8.11 |    0.06 |         - |          NA |
| Add_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 41.455 ns | 0.1296 ns | 0.1212 ns |  8.23 |    0.07 |         - |          NA |
| Add_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 40.052 ns | 0.1170 ns | 0.1095 ns |  7.95 |    0.06 |         - |          NA |
| Add_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] | 46.162 ns | 0.0475 ns | 0.0421 ns |  9.16 |    0.06 |         - |          NA |
| Add_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] |  8.025 ns | 0.0205 ns | 0.0181 ns |  1.59 |    0.01 |         - |          NA |
| Add_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] |  8.053 ns | 0.0050 ns | 0.0047 ns |  1.60 |    0.01 |         - |          NA |
| Add_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] |  8.234 ns | 0.0309 ns | 0.0289 ns |  1.63 |    0.01 |         - |          NA |
| Add_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] |  8.127 ns | 0.0043 ns | 0.0041 ns |  1.61 |    0.01 |         - |          NA |
| Add_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 57.044 ns | 0.1892 ns | 0.1677 ns | 11.32 |    0.08 |         - |          NA |
| Add_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 57.269 ns | 0.1727 ns | 0.1615 ns | 11.37 |    0.09 |         - |          NA |
| Add_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 58.763 ns | 0.1373 ns | 0.1284 ns | 11.66 |    0.09 |         - |          NA |
| Add_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] | 63.562 ns | 0.3111 ns | 0.2910 ns | 12.61 |    0.09 |         - |          NA |
