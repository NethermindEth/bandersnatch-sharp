using Nethermind.Field;
using Nethermind.Verkle.Curve;

namespace Nethermind.Verkle.Polynomial;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public class LagrangeBasis : IEqualityComparer<LagrangeBasis>
{
    public Fr?[] Evaluations;
    public Fr[] Domain;

    private LagrangeBasis()
    {
        Evaluations = new Fr?[] { };
        Domain = new Fr[] { };
    }
    private static LagrangeBasis Empty()
    {
        return new LagrangeBasis();
    }

    public LagrangeBasis(Fr?[] evaluations, Fr[] domain)
    {
        Evaluations = evaluations;
        Domain = domain;
    }

    public Fr?[] Values()
    {
        return Evaluations;
    }

    private static LagrangeBasis arithmetic_op(LagrangeBasis lhs, LagrangeBasis rhs,
        Func<Fr, Fr, Fr> operation)
    {
        if (!lhs.Domain.SequenceEqual(rhs.Domain))
            throw new Exception();

        Fr[] result = new Fr[lhs.Evaluations.Length];

        for (int i = 0; i < lhs.Evaluations.Length; i++)
        {
            result[i] = operation(lhs.Evaluations[i]!, rhs.Evaluations[i]!);
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

    public static LagrangeBasis scale(LagrangeBasis poly, Fr constant)
    {
        Fr[] result = new Fr[poly.Evaluations.Length];

        for (int i = 0; i < poly.Evaluations.Length; i++)
        {
            result[i] = poly.Evaluations[i]! * constant;
        }
        return new LagrangeBasis(result, poly.Domain);
    }

    public Fr evaluate_outside_domain(LagrangeBasis precomputed_weights, Fr z)
    {
        Fr? r = Fr.Zero;
        MonomialBasis? A = MonomialBasis.VanishingPoly(Domain);
        Fr? Az = A.Evaluate(z);

        if (Az.IsZero == true)
            throw new Exception("vanishing polynomial evaluated to zero. z is therefore a point on the domain");


        List<Fr>? inner = new List<Fr>();
        foreach (Fr? x in Domain)
        {
            inner.Add(z - x);
        }

        Fr[]? inverses = Fr.MultiInverse(inner.ToArray());

        for (int i = 0; i < inverses.Length; i++)
        {
            Fr? x = inverses[i];
            r += Evaluations[i] * precomputed_weights.Evaluations[i] * x;
        }


        r = r * Az;

        return r;
    }

    public MonomialBasis Interpolate()
    {
        Fr[]? xs = Domain;
        Fr?[]? ys = Evaluations;

        MonomialBasis? root = MonomialBasis.VanishingPoly(xs);
        if (root.Length() != ys.Length + 1)
            throw new Exception();

        List<MonomialBasis>? nums = new List<MonomialBasis>();
        foreach (Fr? x in xs)
        {
            Fr[]? s = new[] { x.Neg(), Fr.One };
            MonomialBasis? elem = root / new MonomialBasis(s);
            nums.Add(elem);
        }

        List<Fr>? denoms = new List<Fr>();
        for (int i = 0; i < xs.Length; i++)
        {
            denoms.Add(nums[i].Evaluate(xs[i]));
        }
        Fr[]? invdenoms = Fr.MultiInverse(denoms.ToArray());

        Fr[]? b = new Fr[ys.Length];
        for (int i = 0; i < b.Length; i++)
        {
            b[i] = Fr.Zero;
        }

        for (int i = 0; i < xs.Length; i++)
        {
            Fr? ySlice = ys[i] * invdenoms[i];
            for (int j = 0; j < ys.Length; j++)
            {
                if (nums[i].Coeffs[j] is not null && ys[i] is not null)
                {
                    b[j] += nums[i].Coeffs[j] * ySlice;
                }
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

    public static LagrangeBasis operator *(in LagrangeBasis a, in Fr b)
    {
        return scale(a, b);
    }

    public static LagrangeBasis operator *(in Fr a, in LagrangeBasis b)
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

    public bool Equals(LagrangeBasis? x, LagrangeBasis? y)
    {
        return x!.Evaluations.SequenceEqual(y!.Evaluations);
    }

    public int GetHashCode(LagrangeBasis obj)
    {
        return HashCode.Combine(obj.Evaluations, obj.Domain);
    }
}
