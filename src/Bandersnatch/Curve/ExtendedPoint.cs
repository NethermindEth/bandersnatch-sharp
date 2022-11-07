using Field;

namespace Curve;
using Fp = FixedFiniteField<BandersnatchBaseFieldStruct>;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public class ExtendedPoint
{
    public readonly Fp X;
    public readonly Fp Y;
    public readonly Fp T;
    public readonly Fp Z;
    
    private static Fp A => CurveParams.A;
    private static Fp D => CurveParams.D;

    public ExtendedPoint(Fp x, Fp y)
    {
        X = x;
        Y = y;
        T = x * y;
        Z = Fp.One;
    }

    private ExtendedPoint(Fp x, Fp y, Fp t, Fp z)
    {
        X = x;
        Y = y;
        T = t;
        Z = z;
    }

    private ExtendedPoint(AffinePoint p)
    {
        X = p.X;
        Y = p.Y;
        T = X * Y;
        Z = Fp.One;
    }

    public bool IsZero => X.IsZero && Y == Z && !Y.IsZero && T.IsZero;
    public static ExtendedPoint Identity() => new (AffinePoint.Identity());
    
    public static ExtendedPoint Generator() => new (AffinePoint.Generator());
    public ExtendedPoint Dup() => new(X.Dup(), Y.Dup(), T.Dup(), Z.Dup());
    
    public static bool Equals(ExtendedPoint p, ExtendedPoint q)
    {
        if (p.IsZero) return q.IsZero;
        if (q.IsZero) return false;

        return (p.X * q.Z == p.Z * q.X) && (p.Y * q.Z == q.Y * p.Z);
    }
    
    public static ExtendedPoint Neg(ExtendedPoint p) => new (p.X.Neg(), p.Y, p.T.Neg(), p.Z);
    public static ExtendedPoint Add(ExtendedPoint p, ExtendedPoint q)
    {
        var x1 = p.X;
        var y1 = p.Y;
        var t1 = p.T;
        var z1 = p.Z;

        var x2 = q.X;
        var y2 = q.Y;
        var t2 = q.T;
        var z2 = q.Z;

        var a = x1 * x2;
        var b = y1 * y2;

        var c = D * t1 * t2;

        var d = z1 * z2;

        var h = b - (a * A);

        var e = (x1 + y1) * (x2 + y2) - a - b;

        var f = d - c;

        var g = d + c;

        return new ExtendedPoint(e * f, g * h, e * h, f * g);
    }
    public static ExtendedPoint Sub(ExtendedPoint p, ExtendedPoint q) => Add(p, Neg(q));
    public static ExtendedPoint Double(ExtendedPoint p) => Add(p, p);

    public static ExtendedPoint ScalarMultiplication(ExtendedPoint point, Fr scalar)
    {
        var result = Identity();
        var temp = point.Dup();

        var bytes = scalar.ToBytes();
        // TODO: use BitLen to simplify this
        var carry = 0;
        foreach (var elem in bytes)
        {
            if (elem == 0)
            {
                carry += 8;
                continue;
            }
            
            for (int i = carry; i > 0; i--)
            {
                temp = Double(temp);
            }
            
            var binaryString = Convert.ToString(elem, 2);
            var binLength = binaryString.Length;
            for (int i = binLength - 1; i >= 0; i--)
            {
                if (binaryString[i] == '1')
                {
                    result = Add(result, temp);
                }
                temp = Double(temp);
            }

            carry = 8 - binLength;
        }

        return new ExtendedPoint(result.X, result.Y, result.T, result.Z);
    }

    public AffinePoint ToAffine()
    {
        if (IsZero) return AffinePoint.Identity();
        if (Z.IsZero) throw new Exception();
        if (Z.IsOne) return new AffinePoint(X, Y);

        var zInv = Fp.Inverse(Z)?? throw new Exception();

        var xAff = X * zInv;
        var yAff = Y * zInv;

        return new AffinePoint(xAff, yAff);
    }

    public byte[] ToBytes() => ToAffine().ToBytes();

    public static ExtendedPoint operator +(in ExtendedPoint a, in ExtendedPoint b)
    {
        return Add(a, b);
    }

    public static ExtendedPoint operator -(in ExtendedPoint a, in ExtendedPoint b)
    {
        return Sub(a, b);
    }

    public static ExtendedPoint operator *(in ExtendedPoint a, in Fr b)
    {
        return ScalarMultiplication(a, b);
    }
    
    public static ExtendedPoint operator *(in Fr a, in ExtendedPoint b)
    {
        return ScalarMultiplication(b, a);
    }
    
    public static bool operator ==(in ExtendedPoint a, in ExtendedPoint b)
    {
        return Equals(a, b);
    }

    public static bool operator !=(in ExtendedPoint a, in ExtendedPoint b)
    {
        return !(a == b);
    }
    
    private bool Equals(ExtendedPoint other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && T.Equals(other.T) && Z.Equals(other.Z);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == this.GetType() && Equals((ExtendedPoint) obj);
    }

    public override int GetHashCode() => HashCode.Combine(X, Y, T, Z);

}