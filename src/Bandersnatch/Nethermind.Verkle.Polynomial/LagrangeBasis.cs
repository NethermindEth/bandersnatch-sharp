using Nethermind.Field.Montgomery;

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

    private static LagrangeBasis arithmetic_op(LagrangeBasis lhs, LagrangeBasis rhs,
        Func<FrE, FrE, FrE> operation)
    {
        if (!lhs.Domain.SequenceEqual(rhs.Domain))
            throw new Exception();

        FrE[] result = new FrE[lhs.Evaluations.Length];

        for (int i = 0; i < lhs.Evaluations.Length; i++)
        {
            result[i] = operation(lhs.Evaluations[i], rhs.Evaluations[i]);
        }

        return new LagrangeBasis(result, lhs.Domain);
    }

    public static LagrangeBasis Add(LagrangeBasis lhs, LagrangeBasis rhs)
    {
        return arithmetic_op(lhs, rhs, (field, scalarField) => field + scalarField);
    }

    public static LagrangeBasis Sub(LagrangeBasis lhs, LagrangeBasis rhs)
    {
        return arithmetic_op(lhs, rhs, (field, scalarField) => field - scalarField);
    }

    public static LagrangeBasis Mul(LagrangeBasis lhs, LagrangeBasis rhs)
    {
        return arithmetic_op(lhs, rhs, (field, scalarField) => field * scalarField);
    }

    public static LagrangeBasis scale(LagrangeBasis poly, FrE constant)
    {
        FrE[] result = new FrE[poly.Evaluations.Length];

        for (int i = 0; i < poly.Evaluations.Length; i++)
        {
            result[i] = poly.Evaluations[i] * constant;
        }
        return new LagrangeBasis(result, poly.Domain);
    }

    public FrE evaluate_outside_domain(LagrangeBasis precomputed_weights, FrE z)
    {
        FrE r = FrE.Zero;
        MonomialBasis A = MonomialBasis.VanishingPoly(Domain);
        FrE Az = A.Evaluate(z);

        if (Az.IsZero)
            throw new Exception("vanishing polynomial evaluated to zero. z is therefore a point on the domain");


        List<FrE> inner = new List<FrE>();
        foreach (FrE x in Domain)
        {
            inner.Add(z - x);
        }

        FrE[] inverses = FrE.MultiInverse(inner.ToArray());

        for (int i = 0; i < inverses.Length; i++)
        {
            FrE x = inverses[i];
            r += Evaluations[i] * precomputed_weights.Evaluations[i] * x;
        }


        r = r * Az;

        return r;
    }

    public MonomialBasis Interpolate()
    {
        FrE[] xs = Domain;
        FrE[] ys = Evaluations;

        MonomialBasis root = MonomialBasis.VanishingPoly(xs);
        if (root.Length() != ys.Length + 1)
            throw new Exception();

        List<MonomialBasis> nums = new List<MonomialBasis>();
        foreach (FrE x in xs)
        {
            FrE[] s = new[] { x.Neg(), FrE.One };
            MonomialBasis elem = root / new MonomialBasis(s);
            nums.Add(elem);
        }

        List<FrE> denoms = new List<FrE>();
        for (int i = 0; i < xs.Length; i++)
        {
            denoms.Add(nums[i].Evaluate(xs[i]));
        }
        FrE[] invdenoms = FrE.MultiInverse(denoms.ToArray());

        FrE[] b = new FrE[ys.Length];
        for (int i = 0; i < b.Length; i++)
        {
            b[i] = FrE.Zero;
        }

        for (int i = 0; i < xs.Length; i++)
        {
            FrE ySlice = ys[i] * invdenoms[i];
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
        return scale(a, b);
    }

    public static LagrangeBasis operator *(in FrE a, in LagrangeBasis b)
    {
        return scale(b, a);
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
}
