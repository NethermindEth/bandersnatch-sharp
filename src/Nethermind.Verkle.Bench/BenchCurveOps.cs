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
    private FrE[] a;
    private CRS crs;

    public BenchCurveOps()
    {
        using IEnumerator<FrE> rand = FrE.GetRandom().GetEnumerator();
        for (int i = 0; i < 10; i++) rand.MoveNext();

        a = new FrE[256];
        for (int i = 0; i < 256; i++)
        {
            a[i] = rand.Current;
            rand.MoveNext();
        }

        crs = CRS.Instance;
    }

    [Benchmark]
    public void BenchmarkMultiScalarMul() => Banderwagon.MultiScalarMul(crs.BasisG, a);
}
