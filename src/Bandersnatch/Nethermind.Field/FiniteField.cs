using System.Numerics;
using Nethermind.Int256;

namespace Nethermind.Field;

public class FiniteField : IComparable<FiniteField>, IComparable, IEqualityComparer<FiniteField>
{
    protected UInt256 Value;
    protected UInt256 Modulus;
    protected UInt256 QMinOneDiv2 => (Modulus - 1) / 2;

    public FiniteField(UInt256 value, UInt256 modulus)
    {
        UInt256.Mod(value, modulus, out Value);
        Modulus = modulus;
    }

    public FiniteField(BigInteger value, UInt256 modulus)
    {
        Modulus = modulus;
        if (value.Sign < 0)
        {
            UInt256Extension.SubtractMod(UInt256.Zero, (UInt256)(-value), modulus, out Value);
        }
        else
        {
            UInt256.Mod((UInt256)value, modulus, out Value);
        }
    }

    protected FiniteField()
    {
    }

    private static FiniteField Zero(UInt256 modulus) => new(UInt256.Zero, modulus);
    private static FiniteField One(UInt256 modulus) => new(UInt256.One, modulus);


    public bool IsConstant(UInt256 constant)
    {
        return Value == constant;
    }

    public bool IsZero => Value.IsZero;
    public bool IsOne => Value.IsOne;

    public byte[] ToBytes() => Value.ToLittleEndian();

    public static FiniteField? FromBytes(byte[] byteEncoded, UInt256 modulus)
    {
        UInt256 value = new UInt256(byteEncoded);
        return value > modulus ? null : new FiniteField(value, modulus);
    }

    public static FiniteField FromBytesReduced(byte[] byteEncoded, UInt256 modulus)
    {
        return new FiniteField(new UInt256(byteEncoded), modulus);
    }

    public static bool LexicographicallyLargest(FiniteField x, UInt256 qMinOneDiv2)
    {
        return x.Value > qMinOneDiv2;
    }

    public bool LexicographicallyLargest(UInt256 qMinOneDiv2)
    {
        return Value > qMinOneDiv2;
    }

    public void CheckAllIntegersSameModulus(FiniteField a, FiniteField b)
    {
        if (Modulus != a.Modulus)
            throw new Exception();

        if (Modulus != b.Modulus)
            throw new Exception();
    }

    public FiniteField Neg()
    {
        FiniteField? result = new FiniteField
        {
            Modulus = Modulus
        };
        UInt256Extension.SubtractMod(UInt256.Zero, Value, Modulus, out result.Value);
        return result;
    }

    public static FiniteField Neg(FiniteField a)
    {
        FiniteField res = new()
        {
            Modulus = a.Modulus
        };
        UInt256Extension.SubtractMod(UInt256.Zero, a.Value, a.Modulus, out res.Value);
        return res;
    }

    public FiniteField Add(FiniteField a)
    {
        FiniteField res = new()
        {
            Modulus = a.Modulus
        };
        UInt256.AddMod(Value, a.Value, Modulus, out res.Value);
        return res;
    }

    public static FiniteField Add(FiniteField a, FiniteField b)
    {
        FiniteField res = new()
        {
            Modulus = a.Modulus
        };
        UInt256.AddMod(a.Value, b.Value, a.Modulus, out res.Value);
        return res;
    }

    public FiniteField Sub(FiniteField a)
    {
        FiniteField res = new()
        {
            Modulus = a.Modulus
        };
        UInt256Extension.SubtractMod(Value, a.Value, Modulus, out res.Value);
        return this;
    }

    public static FiniteField Sub(FiniteField a, FiniteField b)
    {
        FiniteField res = new();
        UInt256Extension.SubtractMod(a.Value, b.Value, a.Modulus, out res.Value);
        res.Modulus = a.Modulus;
        return res;
    }

    public static FiniteField Mul(FiniteField a, FiniteField b)
    {
        FiniteField result = new()
        {
            Modulus = a.Modulus
        };
        UInt256.MultiplyMod(a.Value, b.Value, a.Modulus, out result.Value);
        return result;
    }

    public FiniteField Mul(FiniteField a)
    {
        FiniteField result = new()
        {
            Modulus = a.Modulus
        };
        UInt256.MultiplyMod(Value, a.Value, Modulus, out result.Value);
        return result;
    }

    public static FiniteField? Div(FiniteField a, FiniteField b)
    {
        FiniteField? bInv = Inverse(b);
        return bInv is null ? null : Mul(a, bInv);
    }

    public static FiniteField? ExpMod(FiniteField a, UInt256 b)
    {
        FiniteField result = new()
        {
            Modulus = a.Modulus
        };
        UInt256.ExpMod(a.Value, b, a.Modulus, out result.Value);
        return result;
    }

    public bool Equals(FiniteField a)
    {
        return Value.Equals(a.Value);
    }

    public FiniteField Dup()
    {
        FiniteField? ret = new FiniteField
        {
            Modulus = Modulus,
            Value = Value
        };
        return ret;
    }

    public FiniteField? Inverse()
    {
        if (Value.IsZero) return null;
        FiniteField result = new()
        {
            Modulus = Modulus
        };
        UInt256.ExpMod(Value, Modulus - 2, Modulus, out result.Value);
        return result;
    }

    public static FiniteField? Inverse(FiniteField a)
    {
        if (a.Value.IsZero) return null;

        FiniteField? inv = new FiniteField
        {
            Modulus = a.Modulus
        };
        UInt256.ExpMod(a.Value, a.Modulus - 2, a.Modulus, out inv.Value);
        return inv;
    }

    public string ToString(string format) => Value.ToString(format);

    public static FiniteField[] MultiInverse(FiniteField[] values)
    {
        UInt256 modulus = values[0].Modulus;

        FiniteField? zero = Zero(modulus);
        FiniteField? one = One(modulus);

        FiniteField[] partials = new FiniteField[values.Length + 1];
        partials[0] = one;
        for (int i = 0; i < values.Length; i++)
        {
            FiniteField? x = Mul(partials[i], values[i]);
            partials[i + 1] = x.IsZero ? one : x;
        }

        FiniteField? inverse = Inverse(partials[^1]);

        FiniteField[] outputs = new FiniteField[values.Length];
        outputs[0] = zero;
        for (int i = values.Length - 1; i >= 0; i--)
        {
            outputs[i] = values[i].IsZero ? zero : Mul(partials[i], inverse);
            inverse = inverse.Mul(values[i]);
            inverse = inverse.IsZero ? one : inverse;
        }

        return outputs;
    }

    public static FiniteField? Sqrt(FiniteField a)
    {
        FiniteField res = new()
        {
            Modulus = a.Modulus
        };

        UInt256? val = FieldMethods.ModSqrt(a.Value, a.Modulus);
        if (val is null) return null;
        res.Value = (UInt256)val;
        return res;
    }

    public int Legendre() => FieldMethods.LegendreSymbol(Value, Modulus);

    public static FiniteField operator +(in FiniteField a, in FiniteField b)
    {
        return Add(a, b);
    }

    public static FiniteField operator -(in FiniteField a, in FiniteField b)
    {
        return Sub(a, b);
    }

    public static FiniteField operator *(in FiniteField a, in FiniteField b)
    {
        return Mul(a, b);
    }

    public static FiniteField? operator /(in FiniteField a, in FiniteField b)
    {
        return Div(a, b);
    }

    public static bool operator ==(in FiniteField a, in FiniteField b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(in FiniteField a, in FiniteField b)
    {
        return !(a == b);
    }

    private static void EnsureSameModulus(FiniteField a, FiniteField? b)
    {
        if (a.Modulus != b!.Modulus)
            throw new ArithmeticException("Fields with different modulus cannot be compared");
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((FiniteField)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value, Modulus);
    }

    public int CompareTo(object? obj) => obj is not FiniteField finiteField
        ? throw new InvalidOperationException()
        : CompareTo(finiteField);

    public int CompareTo(FiniteField? other)
    {
        EnsureSameModulus(this, other);
        return Value.CompareTo(other!.Value);
    }

    public bool Equals(FiniteField? x, FiniteField? y)
    {
        return (x!.Modulus == y!.Modulus) && (x.Value == y.Value);
    }
    public int GetHashCode(FiniteField obj)
    {
        return HashCode.Combine(obj.Value, obj.Modulus);
    }

    public int ToInt()
    {
        return (int)Value;
    }
}
