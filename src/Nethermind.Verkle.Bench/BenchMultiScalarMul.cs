// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using System.Threading.Tasks.Dataflow;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Fields.FrEElement;

namespace Nethermind.Verkle.Bench;

[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchMultiScalarMul
{
    [Params(1, 2, 4, 8, 16, 32, 256)]
    public int InputSize { get; set; }

    [Params(1, 4, 8, 12)]
    public int Parallelism { get; set; }

    private readonly FrE[] _a;

    public BenchMultiScalarMul()
    {
        using IEnumerator<FrE> rand = FrE.GetRandom().GetEnumerator();
        for (int i = 0; i < 10; i++) rand.MoveNext();

        _a = new FrE[256];
        for (int i = 0; i < 256; i++)
        {
            _a[i] = rand.Current;
            rand.MoveNext();
        }
    }

    private static CRS Crs => CRS.Instance;

    [Benchmark]
    public void BenchmarkMultiScalarMul()
    {
        Banderwagon.MultiScalarMul(Crs.BasisG[..InputSize], _a[..InputSize], Parallelism);
    }

    [Benchmark]
    public void BenchmarkScalarMul()
    {
        SpinLock accumulationLock = new();
        Banderwagon accumulation = Banderwagon.Identity;

        ActionBlock<int> accumulator = new((i) =>
        {
            Banderwagon multiplierd = Banderwagon.ScalarMul(Crs.BasisG[i], _a[i]);
            bool locked = false;
            accumulationLock.Enter(ref locked);
            accumulation += multiplierd;
            accumulationLock.Exit();
        }, new ExecutionDataflowBlockOptions()
        {
            MaxDegreeOfParallelism = Parallelism,
        });

        for (int i = 0; i < InputSize; i++)
        {
            accumulator.Post(i);
        }

        accumulator.Complete();
        accumulator.Completion.Wait();
    }
}
