using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nethermind.Int256;
using Nethermind.Verkle.Fields.FpEElement;

namespace Nethermind.Verkle.Bench;

public class FpEBenchmarkBase
{
    protected static UInt256 _uMod = FpE._modulus.Value;
    private IEnumerable<BigInteger> Values => new[] { Numbers.UInt256Max }.Concat(UnaryOps.RandomUnsigned(1));

    public IEnumerable<(FpE, UInt256)> ValuesTuple => Values.Select(x => ((FpE)x, (UInt256)x));

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
                if (t.IsOne)
                {
                    break;
                }

                UInt256.ExpMod(t, 2, p, out UInt256 tt);
                t = tt;
            }

            if (m.IsZero)
                return x;

            UInt256.Exp(2, r - m - 1, out UInt256 expGS);
            UInt256.ExpMod(g, expGS, p, out UInt256 gs);

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

public class TwoParamFpEBenchmarkBase : FpEBenchmarkBase
{
    [ParamsSource(nameof(ValuesTuple))] public (FpE, UInt256) A;

    [ParamsSource(nameof(ValuesTuple))] public (FpE, UInt256) B;
}


[SimpleJob(RuntimeMoniker.Net70, baseline: true)]
[NoIntrinsicsJob(RuntimeMoniker.Net70)]
[MemoryDiagnoser]
public class Add : TwoParamFpEBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public FpE Add_FpE()
    {
        return (A.Item1 + B.Item1);
    }

    [Benchmark]
    public UInt256 Add_UInt256()
    {
        UInt256.AddMod(A.Item2, B.Item2, _uMod, out UInt256 res);
        return res;
    }
}


[SimpleJob(RuntimeMoniker.Net70, baseline: true)]
[NoIntrinsicsJob(RuntimeMoniker.Net70)]
[MemoryDiagnoser]
public class Subtract : TwoParamFpEBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public FpE Subtract_FpE()
    {
        return (A.Item1 - B.Item1);
    }

    [Benchmark]
    public UInt256 Subtract_UInt256()
    {
        UInt256.SubtractMod(A.Item2, B.Item2, _uMod, out UInt256 res);
        return res;
    }
}

[SimpleJob(RuntimeMoniker.Net70, baseline: true)]
[NoIntrinsicsJob(RuntimeMoniker.Net70)]
[MemoryDiagnoser]
public class Multiply : TwoParamFpEBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public FpE Multiply_FpE()
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


[SimpleJob(RuntimeMoniker.Net70, baseline: true)]
[NoIntrinsicsJob(RuntimeMoniker.Net70)]
[MemoryDiagnoser]
public class ExpMod : TwoParamFpEBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public FpE ExpMod_FpE()
    {
        FpE.Exp(A.Item1, B.Item2, out FpE res);
        return res;
    }

    [Benchmark]
    public UInt256 ExpMod_UInt256()
    {
        UInt256.ExpMod(A.Item2, B.Item2, _uMod, out UInt256 res);
        return res;
    }
}

[SimpleJob(RuntimeMoniker.Net70, baseline: true)]
[NoIntrinsicsJob(RuntimeMoniker.Net70)]
[MemoryDiagnoser]
public class Inverse : TwoParamFpEBenchmarkBase
{
    [Benchmark(Baseline = true)]
    public FpE Inverse_FpE()
    {
        FpE.Exp(A.Item1, B.Item2, out FpE res);
        return res;
    }

    [Benchmark]
    public UInt256 Inverse_UInt256()
    {
        UInt256.ExpMod(A.Item2, _uMod - 2, _uMod, out UInt256 res);
        return res;
    }
}

[SimpleJob(RuntimeMoniker.Net70)]
[NoIntrinsicsJob(RuntimeMoniker.Net70)]
[MemoryDiagnoser]
public class Sqrt : TwoParamFpEBenchmarkBase
{
    [Benchmark]
    public FpE? Sqrt_FpE()
    {
        return FpE.Sqrt(A.Item1, out FpE res) ? res : null;
    }

    [Benchmark]
    public UInt256? Sqrt_UInt256()
    {
        return ModSqrt(A.Item2, _uMod);
    }
}
