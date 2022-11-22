using Nethermind.Field;
using Nethermind.Field.Montgomery;

namespace Nethermind.Verkle.Curve;

public class Banderwagon
{
    private readonly ExtendedPoint _point;
    private static readonly FpE A = CurveParams.A;

    public Banderwagon(byte[]? serialisedBytesBigEndian, ExtendedPoint? unsafeBandersnatchPoint = null)
    {
        if (unsafeBandersnatchPoint is not null)
        {
            _point = unsafeBandersnatchPoint;
        }
        else
        {
            if (serialisedBytesBigEndian is null)
            {
                throw new Exception();
            }

            (FpE X, FpE Y) point = FromBytes(serialisedBytesBigEndian) ?? throw new Exception();

            _point = new ExtendedPoint(point.X, point.Y);
        }
    }

    public Banderwagon(string serialisedBytesBigEndian)
    {
        (FpE X, FpE Y) point = FromBytes(Convert.FromHexString(serialisedBytesBigEndian)) ?? throw new Exception();
        _point = new ExtendedPoint(point.X, point.Y);
    }

    private Banderwagon(ExtendedPoint exPoint)
    {
        _point = exPoint;
    }

    public static (FpE X, FpE Y)? FromBytes(IEnumerable<byte> serialisedBytesBigEndian)
    {
        IEnumerable<byte> bytes = serialisedBytesBigEndian.Reverse();

        FpE? x = FpE.FromBytes(bytes.ToArray());
        if (x is null) return null;

        FpE? y = AffinePoint.GetYCoordinate(x.Value, true);
        if (y is null) return null;

        return SubgroupCheck(x.Value) != 1 ? null : (x.Value, y.Value);
    }

    public static int SubgroupCheck(FpE x)
    {
        FpE.MulMod(x, x, out FpE res);
        FpE.MulMod(res, A, out res);
        res = res.Neg();
        FpE.AddMod(res, FpE.One, out res);

        return FpE.Legendre(in res);
    }

    public static bool Equals(Banderwagon x, Banderwagon y)
    {
        FpE x1 = x._point.X;
        FpE y1 = x._point.Y;
        FpE x2 = y._point.X;
        FpE y2 = y._point.Y;

        if (x1.IsZero && y1.IsZero) return false;

        if (x2.IsZero && y2.IsZero) return false;

        FpE lhs = x1 * y2;
        FpE rhs = x2 * y1;

        return lhs.Equals(rhs);
    }

    public static Banderwagon Generator() => new Banderwagon(ExtendedPoint.Generator());


    public static Banderwagon Neg(Banderwagon p) => new Banderwagon(ExtendedPoint.Neg(p._point));
    public static Banderwagon Add(Banderwagon p, Banderwagon q) => new Banderwagon(p._point + q._point);
    public static Banderwagon Sub(Banderwagon p, Banderwagon q) => new Banderwagon(p._point - q._point);

    private FpE? _mapToField() => _point.X / _point.Y;

    public byte[] MapToField()
    {
        return _mapToField()?.ToBytes().ToArray() ?? throw new Exception();
    }

    public byte[] ToBytes()
    {
        AffinePoint? affine = _point.ToAffine();
        FpE? x = affine.X.Dup();
        if (affine.Y.LexicographicallyLargest() == false)
        {
            x = affine.X.Neg();
        }

        return x.Value.ToBytesBigEndian().ToArray();
    }

    public static Banderwagon Double(Banderwagon p) => new(ExtendedPoint.Double(p._point));

    public bool IsOnCurve() => _point.ToAffine().IsOnCurve();

    public Banderwagon Dup() => new(_point.Dup());

    public static Banderwagon ScalarMul(Banderwagon element, FrE scalar) => new(element._point * scalar);
    public static Banderwagon Identity() => new(ExtendedPoint.Identity());


    public static Banderwagon TwoTorsionPoint()
    {
        AffinePoint? affinePoint = new AffinePoint(FpE.Zero, FpE.One.Neg());
        return new Banderwagon(new ExtendedPoint(affinePoint.X, affinePoint.Y));
    }

    public static Banderwagon MSM(Banderwagon[] points, FrE[] scalars)
    {
        Banderwagon? res = Identity();
        for (int i = 0; i < points.Length; i++)
        {
            Banderwagon? partialRes = scalars[i] * points[i];
            res += partialRes;
        }
        return res;
    }

    public static Banderwagon operator +(in Banderwagon a, in Banderwagon b)
    {
        return Add(a, b);
    }

    public static Banderwagon operator -(in Banderwagon a, in Banderwagon b)
    {
        return Sub(a, b);
    }

    public static Banderwagon operator *(in Banderwagon a, in FrE b)
    {
        return ScalarMul(a, b);
    }

    public static Banderwagon operator *(in FrE a, in Banderwagon b)
    {
        return ScalarMul(b, a);
    }

    public static bool operator ==(in Banderwagon a, in Banderwagon b)
    {
        return Equals(a, b);
    }

    public static bool operator !=(in Banderwagon a, in Banderwagon b)
    {
        return !(a == b);
    }

    private bool Equals(Banderwagon other)
    {
        return _point.Equals(other._point);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Banderwagon)obj);
    }

    public override int GetHashCode()
    {
        return _point.GetHashCode();
    }
}
