```

BenchmarkDotNet v0.13.10, Pop!_OS 22.04 LTS
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0

```

| Method       | EnvironmentVariables       | A                   | B                   | Mean | Error |
|--------------|----------------------------|---------------------|---------------------|-----:|------:|
| Sqrt_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] |   NA |    NA |
| Sqrt_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] |   NA |    NA |
| Sqrt_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] |   NA |    NA |
| Sqrt_FrE     | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] |   NA |    NA |
| Sqrt_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] |   NA |    NA |
| Sqrt_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] |   NA |    NA |
| Sqrt_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] |   NA |    NA |
| Sqrt_UInt256 | Empty                      | (Net(...)801) [120] | (Net(...)801) [120] |   NA |    NA |
| Sqrt_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] |   NA |    NA |
| Sqrt_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] |   NA |    NA |
| Sqrt_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] |   NA |    NA |
| Sqrt_FrE     | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] |   NA |    NA |
| Sqrt_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] |   NA |    NA |
| Sqrt_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] |   NA |    NA |
| Sqrt_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] |   NA |    NA |
| Sqrt_UInt256 | DOTNET_EnableHWIntrinsic=0 | (Net(...)801) [120] | (Net(...)801) [120] |   NA |    NA |

Benchmarks with issues:
Sqrt.Sqrt_FrE: .NET 8.0(Runtime=.NET 8.0) [A=(Net(...)801) [120], B=(Net(...)801) [120]]
Sqrt.Sqrt_FrE: .NET 8.0(Runtime=.NET 8.0) [A=(Net(...)801) [120], B=(Net(...)801) [120]]
Sqrt.Sqrt_FrE: .NET 8.0(Runtime=.NET 8.0) [A=(Net(...)801) [120], B=(Net(...)801) [120]]
Sqrt.Sqrt_FrE: .NET 8.0(Runtime=.NET 8.0) [A=(Net(...)801) [120], B=(Net(...)801) [120]]
Sqrt.Sqrt_UInt256: .NET 8.0(Runtime=.NET 8.0) [A=(Net(...)801) [120], B=(Net(...)801) [120]]
Sqrt.Sqrt_UInt256: .NET 8.0(Runtime=.NET 8.0) [A=(Net(...)801) [120], B=(Net(...)801) [120]]
Sqrt.Sqrt_UInt256: .NET 8.0(Runtime=.NET 8.0) [A=(Net(...)801) [120], B=(Net(...)801) [120]]
Sqrt.Sqrt_UInt256: .NET 8.0(Runtime=.NET 8.0) [A=(Net(...)801) [120], B=(Net(...)801) [120]]
Sqrt.Sqrt_FrE: .NET 8.0(EnvironmentVariables=DOTNET_EnableHWIntrinsic=0, Runtime=.NET 8.0) [A=(Net(...)801) [120], B=(
Net(...)801) [120]]
Sqrt.Sqrt_FrE: .NET 8.0(EnvironmentVariables=DOTNET_EnableHWIntrinsic=0, Runtime=.NET 8.0) [A=(Net(...)801) [120], B=(
Net(...)801) [120]]
Sqrt.Sqrt_FrE: .NET 8.0(EnvironmentVariables=DOTNET_EnableHWIntrinsic=0, Runtime=.NET 8.0) [A=(Net(...)801) [120], B=(
Net(...)801) [120]]
Sqrt.Sqrt_FrE: .NET 8.0(EnvironmentVariables=DOTNET_EnableHWIntrinsic=0, Runtime=.NET 8.0) [A=(Net(...)801) [120], B=(
Net(...)801) [120]]
Sqrt.Sqrt_UInt256: .NET 8.0(EnvironmentVariables=DOTNET_EnableHWIntrinsic=0, Runtime=.NET 8.0) [A=(Net(...)801) [120],
B=(Net(...)801) [120]]
Sqrt.Sqrt_UInt256: .NET 8.0(EnvironmentVariables=DOTNET_EnableHWIntrinsic=0, Runtime=.NET 8.0) [A=(Net(...)801) [120],
B=(Net(...)801) [120]]
Sqrt.Sqrt_UInt256: .NET 8.0(EnvironmentVariables=DOTNET_EnableHWIntrinsic=0, Runtime=.NET 8.0) [A=(Net(...)801) [120],
B=(Net(...)801) [120]]
Sqrt.Sqrt_UInt256: .NET 8.0(EnvironmentVariables=DOTNET_EnableHWIntrinsic=0, Runtime=.NET 8.0) [A=(Net(...)801) [120],
B=(Net(...)801) [120]]
