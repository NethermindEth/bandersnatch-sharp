``` ini

BenchmarkDotNet=v0.13.2, OS=pop 22.04
AMD Ryzen 9 7900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK=7.0.203
  [Host]   : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  .NET 7.0 : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

Job=.NET 7.0  Runtime=.NET 7.0  

```
|                 Method |                  _a |                  _b |      Mean |     Error |    StdDev |    Median | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|----------------------- |-------------------- |-------------------- |----------:|----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **SubtractMod_BigInteger** | **(115(...)FpE) [201]** | **(115(...)FpE) [201]** | **18.932 ns** | **0.2235 ns** | **0.1981 ns** | **18.966 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
|    SubtractMod_UInt256 | (115(...)FpE) [201] | (115(...)FpE) [201] |  5.176 ns | 0.0510 ns | 0.0452 ns |  5.168 ns |  0.27 |    0.00 |      - |         - |          NA |
|    SubtractMod_Element | (115(...)FpE) [201] | (115(...)FpE) [201] |  2.400 ns | 0.0027 ns | 0.0024 ns |  2.399 ns |  0.13 |    0.00 |      - |         - |          NA |
|                        |                     |                     |           |           |           |           |       |         |        |           |             |
| **SubtractMod_BigInteger** | **(115(...)FpE) [201]** | **(619(...)FpE) [197]** | **40.290 ns** | **0.7801 ns** | **1.5030 ns** | **39.588 ns** |  **1.00** |    **0.00** | **0.0013** |     **112 B** |        **1.00** |
|    SubtractMod_UInt256 | (115(...)FpE) [201] | (619(...)FpE) [197] | 39.736 ns | 0.2089 ns | 0.1852 ns | 39.777 ns |  0.96 |    0.04 |      - |         - |        0.00 |
|    SubtractMod_Element | (115(...)FpE) [201] | (619(...)FpE) [197] |  4.341 ns | 0.0092 ns | 0.0077 ns |  4.342 ns |  0.11 |    0.00 |      - |         - |        0.00 |
|                        |                     |                     |           |           |           |           |       |         |        |           |             |
| **SubtractMod_BigInteger** | **(619(...)FpE) [197]** | **(115(...)FpE) [201]** | **38.967 ns** | **0.4819 ns** | **0.4272 ns** | **38.835 ns** |  **1.00** |    **0.00** | **0.0013** |     **112 B** |        **1.00** |
|    SubtractMod_UInt256 | (619(...)FpE) [197] | (115(...)FpE) [201] | 41.297 ns | 0.2908 ns | 0.2578 ns | 41.246 ns |  1.06 |    0.02 |      - |         - |        0.00 |
|    SubtractMod_Element | (619(...)FpE) [197] | (115(...)FpE) [201] |  2.419 ns | 0.0292 ns | 0.0273 ns |  2.421 ns |  0.06 |    0.00 |      - |         - |        0.00 |
|                        |                     |                     |           |           |           |           |       |         |        |           |             |
| **SubtractMod_BigInteger** | **(619(...)FpE) [197]** | **(619(...)FpE) [197]** | **18.176 ns** | **0.0948 ns** | **0.0887 ns** | **18.173 ns** |  **1.00** |    **0.00** |      **-** |         **-** |          **NA** |
|    SubtractMod_UInt256 | (619(...)FpE) [197] | (619(...)FpE) [197] |  5.064 ns | 0.0389 ns | 0.0325 ns |  5.056 ns |  0.28 |    0.00 |      - |         - |          NA |
|    SubtractMod_Element | (619(...)FpE) [197] | (619(...)FpE) [197] |  2.482 ns | 0.0061 ns | 0.0057 ns |  2.481 ns |  0.14 |    0.00 |      - |         - |          NA |