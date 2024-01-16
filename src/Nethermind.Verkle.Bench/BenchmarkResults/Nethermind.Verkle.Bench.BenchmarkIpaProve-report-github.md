```

BenchmarkDotNet v0.13.10, Pop!_OS 22.04 LTS
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 8.0.100
  [Host]   : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
  .NET 8.0 : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

Job=.NET 8.0  Runtime=.NET 8.0

```

| Method            | EnvironmentVariables       |      Mean |    Error |   StdDev |     Gen0 |     Gen1 |     Gen2 | Allocated |
|-------------------|----------------------------|----------:|---------:|---------:|---------:|---------:|---------:|----------:|
| TestBasicIpaProve | Empty                      |  90.59 ms | 1.171 ms | 1.096 ms | 166.6667 | 166.6667 | 166.6667 |   3.52 MB |
| TestBasicIpaProve | DOTNET_EnableHWIntrinsic=0 | 112.48 ms | 0.857 ms | 0.802 ms | 200.0000 | 200.0000 | 200.0000 |   3.52 MB |
