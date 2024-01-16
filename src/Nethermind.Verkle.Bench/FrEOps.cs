using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nethermind.Int256;
using Nethermind.Verkle.Fields.FrEElement;

namespace Nethermind.Verkle.Bench;

public class FrEBenchmarkBase
{
    protected static UInt256 _uMod = FrE._modulus.Value;
    private static IEnumerable<BigInteger> Values => new[] { Numbers.UInt256Max }.Concat(UnaryOps.RandomUnsigned(1));

    public static IEnumerable<(FrE, UInt256)> ValuesTuple => Values.Select(x => (FrE.qElement, FrE._modulus.Value));

    protected static UInt256? ModSqrt(UInt256 a, UInt256 p)
    {
        if (LegendreSymbol(a, p) != 1)
            return null;
        if (a.IsZero)
            return UInt256.Zero;
        if (a == 2)
            return UInt256.Zero;
        UInt256.Mod(a, 4, out UInt256 res);
        if (res == 4)
        {
            UInt256.Divide(a + 1, 4, out UInt256 exp);
            UInt256.ExpMod(a, exp, p, out UInt256 ls);
            return ls;
        }

        UInt256 s = p - 1;
        UInt256 e = 0;

        UInt256.Mod(s, 2, out UInt256 loopVar);
        while (loopVar.IsZero)
        {
            UInt256.Divide(s, 2, out UInt256 ss);
            s = ss;
            e += 1;
            UInt256.Mod(s, 2, out loopVar);
        }

        UInt256 n = 2;
        while (LegendreSymbol(n, p) != -1)
            n += 1;

        UInt256.Divide(s + 1, 2, out UInt256 expX);
        UInt256.ExpMod(a, expX, p, out UInt256 x);

        UInt256.ExpMod(a, s, p, out UInt256 b);
        UInt256.ExpMod(n, s, p, out UInt256 g);

        UInt256 r = e;

        while (true)
        {
            UInt256 t = b;
            UInt256 m = UInt256.Zero;

            for (; m < r; m++)
            {
                if (t.IsOne) break;

                UInt256.ExpMod(t, 2, p, out UInt256 tt);
                t = tt;
            }

            if (m.IsZero)
                return x;

            UInt256.Exp(2, r - m - 1, out UInt256 expGs);
            UInt256.ExpMod(g, expGs, p, out UInt256 gs);

            UInt256.MultiplyMod(gs, gs, p, out g);
            UInt256.MultiplyMod(x, gs, p, out x);
            UInt256.MultiplyMod(b, g, p, out b);

            r = m;
        }
    }

    private static int LegendreSymbol(UInt256 a, UInt256 p)
    {
        UInt256.Divide(p - 1, 2, out UInt256 exp);
        UInt256.ExpMod(a, exp, p, out UInt256 ls);
        return ls == p - 1 ? -1 : 1;
    }
}

public class TwoParamFrEBenchmarkBase : FrEBenchmarkBase
{
    [ParamsSource(nameof(ValuesTuple))] public (FrE, UInt256) A;

    [ParamsSource(nameof(ValuesTuple))] public (FrE, UInt256) B;
}

[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class Add : TwoParamFrEBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public FrE Add_FrE()
    {
        return A.Item1 + B.Item1;
    }

    [Benchmark]
    public UInt256 Add_UInt256()
    {
        UInt256.AddMod(A.Item2, B.Item2, _uMod, out UInt256 res);
        return res;
    }
}

[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class Subtract : TwoParamFrEBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public FrE Subtract_FrE()
    {
        return A.Item1 - B.Item1;
    }

    [Benchmark]
    public UInt256 Subtract_UInt256()
    {
        UInt256.SubtractMod(A.Item2, B.Item2, _uMod, out UInt256 res);
        return res;
    }
}

[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class Multiply : TwoParamFrEBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public FrE Multiply_FrE()
    {
        return A.Item1 * B.Item1;
    }

    [Benchmark]
    public UInt256 Multiply_UInt256()
    {
        UInt256.MultiplyMod(A.Item2, B.Item2, _uMod, out UInt256 res);
        return res;
    }
}

[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class ExpMod : TwoParamFrEBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public FrE ExpMod_FrE()
    {
        FrE.Exp(A.Item1, B.Item2, out FrE res);
        return res;
    }

    [Benchmark]
    public UInt256 ExpMod_UInt256()
    {
        UInt256.ExpMod(A.Item2, B.Item2, _uMod, out UInt256 res);
        return res;
    }
}

[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class Inverse : TwoParamFrEBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public FrE Inverse_FrE()
    {
        FrE.Exp(A.Item1, B.Item2, out FrE res);
        return res;
    }

    [Benchmark]
    public UInt256 Inverse_UInt256()
    {
        UInt256.ExpMod(A.Item2, _uMod - 2, _uMod, out UInt256 res);
        return res;
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class Sqrt : TwoParamFrEBenchmarkBase
{
    [Benchmark]
    public FrE? Sqrt_FrE()
    {
        return FrE.Sqrt(A.Item1, out FrE res) ? res : null!;
    }

    [Benchmark]
    public UInt256? Sqrt_UInt256()
    {
        return ModSqrt(A.Item2, _uMod);
    }
}
