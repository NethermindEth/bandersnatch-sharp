using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FE = Nethermind.Verkle.Fields.FpEElement.FpE;

namespace Nethermind.Verkle.Bench;

[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchSqrt
{
    [ParamsSource(nameof(SquareList))] public FE _randomSquare;
    public static IEnumerable<FE> SquareList => _randomSquareListHex.Select(x => FE.FromBytesReduced(Convert.FromHexString(x)));

    public static readonly string[] _randomSquareListHex =
    [
        "6D050190ABA180050907CECACEEAFB17A79680C697008E6EE4FE0141F55B4750",
        "04B868D3637F0A880CF9E67A9C2E12AAE75862C0C978BD392F150E834C982139",
        "BE7DC4158328B5278D9BBC41187010BF292CB3BA4FA54481797EBFA581B6FB58",
        "890FCBA8BB6D091895AB1259FE374606EC713AC9D340138CC38165B736602220",
        "97B4EF2EDA76503CF096A8D93AE53CB06FF06912104D7F3F7A8FFE911881A018",
    ];

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
    public FE SqrtTonelliShanks()
    {
        FE.SqrtOld(_randomSquare, out FE sqrtOld);
        return sqrtOld;
    }

    [Benchmark]
    public FE SqrtTonelliShanksPreComputedTables()
    {
        FE.Sqrt(_randomSquare, out FE sqrtNew);
        return sqrtNew;
    }
}
