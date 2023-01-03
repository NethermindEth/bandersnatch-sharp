using System.Numerics;
using Nethermind.Int256;

namespace Nethermind.Field
{
    public interface IFieldDefinition
    {
        UInt256 FieldMod { get; }
    }

    public class FixedFiniteField<T> : FiniteField, IComparable<FixedFiniteField<T>>, IEqualityComparer<FixedFiniteField<T>>
        where T : struct, IFieldDefinition
    {
        public FixedFiniteField(UInt256 value)
        {
            Modulus = new T().FieldMod;
            UInt256.Mod(value, Modulus, out Value);
        }

        public FixedFiniteField(BigInteger value)
        {
            Modulus = new T().FieldMod;
            if (value.Sign < 0)
            {
                UInt256Extension.SubtractMod(UInt256.Zero, (UInt256)(-value), Modulus, out Value);
            }
            else
            {
                UInt256.Mod((UInt256)value, Modulus, out Value);
            }
        }

        private FixedFiniteField()
        {
            Modulus = new T().FieldMod;
        }

        public static FixedFiniteField<T> Zero => new FixedFiniteField<T>((UInt256)0);
        public static FixedFiniteField<T> One => new FixedFiniteField<T>((UInt256)1);

        public int CompareTo(FixedFiniteField<T>? other)
        {
            return Value.CompareTo(other!.Value);
        }

        public bool Equals(FixedFiniteField<T>? x, FixedFiniteField<T>? y)
        {
            return x.Value == y.Value;
        }
        public int GetHashCode(FixedFiniteField<T> obj)
        {
            return HashCode.Combine(obj.Value, obj.Modulus);
        }

        public static FixedFiniteField<T>? FromBytes(byte[] byteEncoded)
        {
            UInt256 value = new UInt256(byteEncoded);
            return value > new T().FieldMod ? null : new FixedFiniteField<T>(value);
        }

        public static FixedFiniteField<T> FromBytesReduced(byte[] byteEncoded)
        {
            return new FixedFiniteField<T>(new UInt256(byteEncoded));
        }

        public static new FixedFiniteField<T> FromBytesReduced(byte[] byteEncoded, UInt256 modulus)
        {
            throw new Exception("cannot get field with different modulus");
        }

        public static bool LexicographicallyLargest(FixedFiniteField<T> x, UInt256 qMinOneDiv2)
        {
            return x.Value > qMinOneDiv2;
        }

        public bool LexicographicallyLargest()
        {
            return Value > QMinOneDiv2;
        }

        public new FixedFiniteField<T> Neg()
        {
            FixedFiniteField<T>? result = new FixedFiniteField<T>();
            UInt256Extension.SubtractMod(UInt256.Zero, Value, Modulus, out result.Value);
            return result;
        }

        public static FixedFiniteField<T> Neg(FixedFiniteField<T> a)
        {
            FixedFiniteField<T> res = new FixedFiniteField<T>();
            UInt256Extension.SubtractMod(UInt256.Zero, a.Value, a.Modulus, out res.Value);
            return res;
        }

        public FixedFiniteField<T> Add(FixedFiniteField<T> a)
        {
            FixedFiniteField<T> res = new FixedFiniteField<T>();
            UInt256.AddMod(Value, a.Value, Modulus, out res.Value);
            return res;
        }

        public static FixedFiniteField<T> Add(FixedFiniteField<T> a, FixedFiniteField<T> b)
        {
            FixedFiniteField<T> res = new FixedFiniteField<T>();
            UInt256.AddMod(a.Value, b.Value, a.Modulus, out res.Value);
            return res;
        }

        public FixedFiniteField<T> Sub(FixedFiniteField<T> a)
        {
            FixedFiniteField<T> res = new FixedFiniteField<T>();
            UInt256Extension.SubtractMod(Value, a.Value, Modulus, out res.Value);
            return res;
        }

        public static FixedFiniteField<T> Sub(FixedFiniteField<T> a, FixedFiniteField<T> b)
        {
            FixedFiniteField<T> res = new FixedFiniteField<T>();
            UInt256Extension.SubtractMod(a.Value, b.Value, a.Modulus, out res.Value);
            return res;
        }

        public static FixedFiniteField<T> Mul(FixedFiniteField<T> a, FixedFiniteField<T> b)
        {
            FixedFiniteField<T> result = new FixedFiniteField<T>();
            UInt256.MultiplyMod(a.Value, b.Value, a.Modulus, out result.Value);
            return result;
        }

        public FixedFiniteField<T> Mul(FixedFiniteField<T> a)
        {
            FixedFiniteField<T> result = new FixedFiniteField<T>();
            UInt256.MultiplyMod(Value, a.Value, Modulus, out result.Value);
            return result;
        }

        public static FixedFiniteField<T>? Div(FixedFiniteField<T> a, FixedFiniteField<T> b)
        {
            FixedFiniteField<T>? bInv = Inverse(b);
            return bInv is null ? null : Mul(a, bInv);
        }

        public static FixedFiniteField<T>? ExpMod(FixedFiniteField<T> a, UInt256 b)
        {
            FixedFiniteField<T> result = new FixedFiniteField<T>();
            UInt256.ExpMod(a.Value, b, a.Modulus, out result.Value);
            return result;
        }

        public bool Equals(FixedFiniteField<T> a)
        {
            return Value.Equals(a.Value);
        }

        public new FixedFiniteField<T> Dup()
        {
            FixedFiniteField<T> ret = new FixedFiniteField<T>
            {
                Value = Value, Modulus = Modulus
            };
            return ret;
        }

        public new FixedFiniteField<T>? Inverse()
        {
            if (Value.IsZero) return null;
            FixedFiniteField<T> result = new FixedFiniteField<T>();
            UInt256.ExpMod(Value, Modulus - 2, Modulus, out result.Value);
            return result;
        }

        public static FixedFiniteField<T>? Inverse(FixedFiniteField<T> a)
        {
            if (a.Value.IsZero) return null;
            FixedFiniteField<T> inv = new FixedFiniteField<T>();
            UInt256.ExpMod(a.Value, a.Modulus - 2, a.Modulus, out inv.Value);
            return inv;
        }

        public static FixedFiniteField<T>[] MultiInverse(FixedFiniteField<T>[] values)
        {
            FixedFiniteField<T>[] partials = new FixedFiniteField<T>[values.Length + 1];
            partials[0] = One;
            for (int i = 0; i < values.Length; i++)
            {
                FixedFiniteField<T> x = Mul(partials[i], values[i]);
                partials[i + 1] = x.IsZero ? One : x;
            }

            FixedFiniteField<T>? inverse = Inverse(partials[^1]);

            FixedFiniteField<T>[] outputs = new FixedFiniteField<T>[values.Length];
            for (int i = values.Length - 1; i >= 0; i--)
            {
                outputs[i] = values[i].IsZero ? Zero : Mul(partials[i], inverse);
                inverse = inverse.Mul(values[i]);
                inverse = inverse.IsZero ? One : inverse;
            }
            return outputs;
        }

        public static FixedFiniteField<T>? Sqrt(FixedFiniteField<T> a)
        {
            FixedFiniteField<T> res = new FixedFiniteField<T>();

            UInt256? val = FieldMethods.ModSqrt(a.Value, a.Modulus);
            if (val is null)
                return null;
            res.Value = (UInt256)val;
            return res;
        }

        public static FixedFiniteField<T> operator +(in FixedFiniteField<T> a, in FixedFiniteField<T> b)
        {
            return Add(a, b);
        }

        public static FixedFiniteField<T> operator -(in FixedFiniteField<T> a, in FixedFiniteField<T> b)
        {
            return Sub(a, b);
        }

        public static FixedFiniteField<T> operator *(in FixedFiniteField<T> a, in FixedFiniteField<T> b)
        {
            return Mul(a, b);
        }

        public static FixedFiniteField<T>? operator /(in FixedFiniteField<T> a, in FixedFiniteField<T> b)
        {
            return Div(a, b);
        }

        public static bool operator ==(in FixedFiniteField<T> a, in FixedFiniteField<T> b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(in FixedFiniteField<T> a, in FixedFiniteField<T> b)
        {
            return !(a == b);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FixedFiniteField<T>)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Modulus);
        }

        public new int CompareTo(object? obj)
        {
            return obj is not FixedFiniteField<T> fixedFiniteField
                ? throw new InvalidOperationException()
                : CompareTo(fixedFiniteField);
        }
    }
}
