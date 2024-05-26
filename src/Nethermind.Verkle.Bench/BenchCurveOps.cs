// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nethermind.RustVerkle;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Fields.FrEElement;

namespace Nethermind.Verkle.Bench;

[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchCurveOps
{
    private readonly FrE[] _a;
    private readonly byte[] _input;

    private static IntPtr VerkleContext = RustVerkleLib.VerkleContextNew();

    public BenchCurveOps()
    {
        using IEnumerator<FrE> rand = FrE.GetRandom().GetEnumerator();
        for (int i = 0; i < 10; i++) rand.MoveNext();

        _a = new FrE[256];
        var inp = new List<byte>();
        for (int i = 0; i < 256; i++)
        {
            _a[i] = rand.Current;
            inp.AddRange(_a[i].ToBytes());
            rand.MoveNext();
        }

        _input = inp.ToArray();
    }

    private static CRS Crs => CRS.Instance;

    [Benchmark]
    public void BenchmarkMultiScalarMul()
    {
        Banderwagon.MultiScalarMul(Crs.BasisG, _a);
    }

    [Benchmark]
    public void BenchmarkMultiScalarMulRust()
    {
        var outhash = new byte[32];
        RustVerkleLib.VerkleMSM(VerkleContext, _input, 8192, outhash);
    }
}
