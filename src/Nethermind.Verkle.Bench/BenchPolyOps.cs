// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nethermind.Verkle.Fields.FrEElement;
using Nethermind.Verkle.Proofs;

namespace Nethermind.Verkle.Bench;

[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchPolyOps
{
    private readonly FrE[] _a;
    private readonly FrE[] _b;

    public BenchPolyOps()
    {
        _a = new[] { FrE.SetElement(1), FrE.SetElement(2), FrE.SetElement(3), FrE.SetElement(4), FrE.SetElement(5) };

        _b = new[]
        {
            FrE.SetElement(10), FrE.SetElement(12), FrE.SetElement(13), FrE.SetElement(14), FrE.SetElement(15)
        };
    }

    [Benchmark]
    public void BenchmarkInnerProduct()
    {
        Ipa.InnerProduct(_a, _b);
    }
}
