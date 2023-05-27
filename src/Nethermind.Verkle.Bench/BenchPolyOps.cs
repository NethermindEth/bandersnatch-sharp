// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nethermind.Verkle.Fields.FrEElement;
using Nethermind.Verkle.Proofs;

namespace Nethermind.Verkle.Bench;

[SimpleJob(RuntimeMoniker.Net70, baseline: true)]
[NoIntrinsicsJob(RuntimeMoniker.Net70)]
[MemoryDiagnoser]
public class BenchPolyOps
{
    private FrE[] a;
    private FrE[] b;

    public BenchPolyOps()
    {
        a = new FrE[]
        {
            FrE.SetElement(1), FrE.SetElement(2), FrE.SetElement(3), FrE.SetElement(4), FrE.SetElement(5)
        };

        b = new FrE[]
        {
            FrE.SetElement(10), FrE.SetElement(12), FrE.SetElement(13), FrE.SetElement(14), FrE.SetElement(15)
        };
    }

    [Benchmark]
    public void BenchmarkInnerProduct() => Ipa.InnerProduct(a, b);
}
