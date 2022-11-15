using Nethermind.Field;
using Nethermind.Verkle.Curve;

namespace Nethermind.Verkle.Polynomial;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public class MonomialBasis : IEqualityComparer<MonomialBasis>
{
    public readonly Fr?[] Coeffs;

    public MonomialBasis(Fr?[] coeffs)
    {
        Coeffs = coeffs;
    }

    public static MonomialBasis Empty() => new(new Fr?[] { });

    private static MonomialBasis Mul(MonomialBasis a, MonomialBasis b)
    {
        Fr[]? output = new Fr[a.Length() + b.Length() - 1];
        for (int i = 0; i < a.Length(); i++)
        {
            for (int j = 0; j < b.Length(); j++)
            {
                output[i + j] += a.Coeffs[i]! * b.Coeffs[j]!;
            }
        }
        return new MonomialBasis(output);
    }

    public static MonomialBasis Div(MonomialBasis a, MonomialBasis b)
    {
        if (a.Length() < b.Length())
        {
            throw new Exception();
        }

        Fr?[]? x = a.Coeffs.ToArray();
        List<Fr> output = new();

        int aPos = a.Length() - 1;
        int bPos = b.Length() - 1;

        int diff = aPos - bPos;
        while (diff >= 0)
        {
            Fr? quot = x[aPos]! / b.Coeffs[bPos]!;
            output.Insert(0, quot!);
            for (int i = bPos; i > -1; i--)
            {
                x[diff + i] = x[diff + i]! - b.Coeffs[i]! * quot!;
            }

            aPos -= 1;
            diff -= 1;
        }

        return new MonomialBasis(output.ToArray());
    }

    public Fr Evaluate(Fr x)
    {
        Fr? y = Fr.Zero;
        Fr? powerOfX = Fr.One;
        foreach (Fr? pCoeff in Coeffs)
        {
            y += powerOfX * pCoeff!;
            powerOfX *= x;
        }

        return y;
    }

    public static MonomialBasis FormalDerivative(MonomialBasis f)
    {
        Fr?[]? derivative = new Fr?[f.Length() - 1];
        for (int i = 1; i < f.Length(); i++)
        {
            Fr? x = new Fr(i) * f.Coeffs[i]!;
            derivative[i - 1] = x;
        }
        return new MonomialBasis(derivative.ToArray());
    }

    public static MonomialBasis VanishingPoly(IEnumerable<Fr> xs)
    {
        List<Fr> root = new() { Fr.One };
        foreach (Fr? x in xs)
        {
            root.Insert(0, Fr.Zero);
            for (int i = 0; i < root.Count - 1; i++)
            {
                root[i] -= root[i + 1] * x;
            }
        }

        return new MonomialBasis(root.ToArray());
    }

    public int Length()
    {
        return Coeffs.Length;
    }

    public static MonomialBasis operator /(in MonomialBasis a, in MonomialBasis b)
    {
        return Div(a, b);
    }

    public static MonomialBasis operator *(in MonomialBasis a, in MonomialBasis b)
    {
        return Mul(a, b);
    }

    public static bool operator ==(in MonomialBasis a, in MonomialBasis b)
    {
        return a.Coeffs == b.Coeffs;
    }

    public static bool operator !=(in MonomialBasis a, in MonomialBasis b)
    {
        return !(a == b);
    }

    public bool Equals(MonomialBasis? x, MonomialBasis? y)
    {
        return x!.Coeffs.SequenceEqual(y!.Coeffs);
    }

    public int GetHashCode(MonomialBasis obj)
    {
        return obj.Coeffs.GetHashCode();
    }

    private bool Equals(MonomialBasis other)
    {
        return Coeffs.Equals(other.Coeffs);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((MonomialBasis)obj);
    }

    public override int GetHashCode()
    {
        return Coeffs.GetHashCode();
    }
}
