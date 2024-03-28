using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FE = Nethermind.Verkle.Fields.FpEElement.FpE;

namespace Nethermind.Verkle.Bench;

[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchSqrt
{
    private FE _randomSquare;

    [GlobalSetup]
    public void Setup()
    {
        using IEnumerator<FE> set = FE.GetRandom().GetEnumerator();
        while (set.MoveNext())
        {
            FE x = set.Current;
            if (FE.Legendre(x) != 1) continue;
            _randomSquare = x;
            break;
        }
    }

    [Benchmark(Baseline = true)]
    public void SqrtOld()
    {
        FE.Sqrt(_randomSquare, out FE sqrtOld);
    }

    [Benchmark]
    public void SqrtNew()
    {
        FE.SqrtNew(_randomSquare, out FE sqrtNew);
    }
}
