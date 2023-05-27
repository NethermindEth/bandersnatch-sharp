// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Fields.FrEElement;

namespace Nethermind.Verkle.Bench;

[SimpleJob(RuntimeMoniker.Net70, baseline: true)]
[NoIntrinsicsJob(RuntimeMoniker.Net70)]
[MemoryDiagnoser]
public class BenchCurveOps
{
    private readonly FrE[] _a;
    private readonly CRS _crs;

    public BenchCurveOps()
    {
        using IEnumerator<FrE> rand = FrE.GetRandom().GetEnumerator();
        for (int i = 0; i < 10; i++) rand.MoveNext();

        _a = new FrE[256];
        for (int i = 0; i < 256; i++)
        {
            _a[i] = rand.Current;
            rand.MoveNext();
        }

        _crs = CRS.Instance;
    }

    [Benchmark]
    public void BenchmarkMultiScalarMul() => Banderwagon.MultiScalarMul(_crs.BasisG, _a);
}
