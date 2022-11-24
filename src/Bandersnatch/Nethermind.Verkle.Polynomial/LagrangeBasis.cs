using Nethermind.Field.Montgomery.FrEElement;

namespace Nethermind.Verkle.Polynomial;

public class LagrangeBasis : IEqualityComparer<LagrangeBasis>
{
    public FrE[] Evaluations;
    public FrE[] Domain;

    private LagrangeBasis()
    {
        Evaluations = new FrE[] { };
        Domain = new FrE[] { };
    }
    private static LagrangeBasis Empty()
    {
        return new LagrangeBasis();
    }

    public LagrangeBasis(FrE[] evaluations, FrE[] domain)
    {
        Evaluations = evaluations;
        Domain = domain;
    }

    public FrE[] Values()
    {
        return Evaluations;
    }

    private static LagrangeBasis ArithmeticOp(LagrangeBasis lhs, LagrangeBasis rhs, ArithmeticOps op)
    {
        if (!lhs.Domain.SequenceEqual(rhs.Domain)) throw new Exception();

        FrE[] result = new FrE[lhs.Evaluations.Length];

        Parallel.For(0, lhs.Evaluations.Length, i =>
        {
            result[i] = op switch
            {
                ArithmeticOps.Add => lhs.Evaluations[i] + rhs.Evaluations[i],
                ArithmeticOps.Sub => lhs.Evaluations[i] - rhs.Evaluations[i],
                ArithmeticOps.Mul => lhs.Evaluations[i] * rhs.Evaluations[i],
                var _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
            };
        });

        return new LagrangeBasis(result, lhs.Domain);
    }

    public static LagrangeBasis Add(LagrangeBasis lhs, LagrangeBasis rhs)
    {
        return ArithmeticOp(lhs, rhs, ArithmeticOps.Add);
    }

    public static LagrangeBasis Sub(LagrangeBasis lhs, LagrangeBasis rhs)
    {
        return ArithmeticOp(lhs, rhs, ArithmeticOps.Sub);
    }

    public static LagrangeBasis Mul(LagrangeBasis lhs, LagrangeBasis rhs)
    {
        return ArithmeticOp(lhs, rhs, ArithmeticOps.Mul);
    }

    public static LagrangeBasis Scale(LagrangeBasis poly, FrE constant)
    {
        FrE[] result = new FrE[poly.Evaluations.Length];

        for (int i = 0; i < poly.Evaluations.Length; i++)
        {
            result[i] = poly.Evaluations[i] * constant;
        }
        return new LagrangeBasis(result, poly.Domain);
    }

    public FrE EvaluateOutsideDomain(LagrangeBasis precomputedWeights, FrE z)
    {
        FrE r = FrE.Zero;
        MonomialBasis a = MonomialBasis.VanishingPoly(Domain);
        FrE az = a.Evaluate(z);

        if (az.IsZero)
            throw new Exception("vanishing polynomial evaluated to zero. z is therefore a point on the domain");


        FrE[] inverses = FrE.MultiInverse(Domain.Select(x => z - x).ToArray());

        for (int i = 0; i < inverses.Length; i++)
        {
            FrE x = inverses[i];
            r += Evaluations[i] * precomputedWeights.Evaluations[i] * x;
        }

        r *= az;

        return r;
    }

    public MonomialBasis Interpolate()
    {
        FrE[] xs = Domain;
        FrE[] ys = Evaluations;

        MonomialBasis root = MonomialBasis.VanishingPoly(xs);
        if (root.Length() != ys.Length + 1)
            throw new Exception();

        List<MonomialBasis> nums = xs.Select(x => new[]
            {
                x.Negative(), FrE.One
            })
            .Select(s => root / new MonomialBasis(s))
            .ToList();

        FrE[] invDenoms = FrE.MultiInverse(xs.Select((t, i) => nums[i].Evaluate(t)).ToArray());

        FrE[] b = new FrE[ys.Length];
        for (int i = 0; i < b.Length; i++)
        {
            b[i] = FrE.Zero;
        }

        for (int i = 0; i < xs.Length; i++)
        {
            FrE ySlice = ys[i] * invDenoms[i];
            for (int j = 0; j < ys.Length; j++)
            {
                b[j] += nums[i].Coeffs[j] * ySlice;
            }
        }

        while (b.Length > 0 && b[^1].IsZero)
        {
            Array.Resize(ref b, b.Length - 1);
        }

        return new MonomialBasis(b);
    }

    public static LagrangeBasis operator +(in LagrangeBasis a, in LagrangeBasis b)
    {
        return Add(a, b);
    }

    public static LagrangeBasis operator -(in LagrangeBasis a, in LagrangeBasis b)
    {
        return Sub(a, b);
    }

    public static LagrangeBasis operator *(in LagrangeBasis a, in LagrangeBasis b)
    {
        return Mul(a, b);
    }

    public static LagrangeBasis operator *(in LagrangeBasis a, in FrE b)
    {
        return Scale(a, b);
    }

    public static LagrangeBasis operator *(in FrE a, in LagrangeBasis b)
    {
        return Scale(b, a);
    }

    public static bool operator ==(in LagrangeBasis a, in LagrangeBasis b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(in LagrangeBasis a, in LagrangeBasis b)
    {
        return !(a == b);
    }

    public bool Equals(LagrangeBasis x, LagrangeBasis y)
    {
        return x!.Evaluations.SequenceEqual(y!.Evaluations);
    }

    public int GetHashCode(LagrangeBasis obj)
    {
        return HashCode.Combine(obj.Evaluations, obj.Domain);
    }

    private enum ArithmeticOps
    {
        Add,
        Sub,
        Mul
    }
}
