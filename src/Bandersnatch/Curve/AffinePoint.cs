using Field;

namespace Curve;
using Fp = FixedFiniteField<BandersnatchBaseFieldStruct>;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;


public class AffinePoint
{
    public readonly Fp X;
    public readonly Fp Y;

    private static Fp A => CurveParams.A;
    private static Fp D => CurveParams.D;

    private const byte MCompressedNegative = 128;
    private const byte MCompressedPositive = 0;
    
    public AffinePoint(Fp x, Fp y)
    {
        X = x;
        Y = y;
    }

    public static AffinePoint Generator()
    {
        var yTe = CurveParams.YTe.Dup();
        var xTe = CurveParams.XTe.Dup();
        return new AffinePoint(xTe, yTe);
    }

    public static AffinePoint Neg(AffinePoint p)
    {
        return new AffinePoint(p.X.Neg(), p.Y);
    }

    public static AffinePoint Add(AffinePoint p, AffinePoint q)
    {
        var x1 = p.X;
        var y1 = p.Y;
        var x2 = q.X;
        var y2 = q.Y;

        var x1y2 = x1 * y2;
        var y1x2 = y1 * x2;
        var ax1x2 = x1 * x2 * A;
        var y1y2 = y1 * y2;

        var dx1x2y1y2 = x1y2 * y1x2 * D;

        var xNum = x1y2 + y1x2;

        var xDen = Fp.One + dx1x2y1y2;

        var yNum = y1y2 - ax1x2;

        var yDen = Fp.One - dx1x2y1y2;

        var x = xNum / xDen ?? throw new Exception();

        var y = yNum / yDen ?? throw new Exception();

        return new AffinePoint(x, y);
    }

    public static AffinePoint Sub(AffinePoint p, AffinePoint q)
    {
        var negQ = Neg(q);
        return Add(p, negQ);
    }
    
    public static AffinePoint Double(AffinePoint p)
    {
        return Add(p, p);
    }

    public bool IsOnCurve()
    {
        var xSq = X * X;
        var ySq = Y * Y;

        var dxySq = xSq * ySq * D;
        var aXSq = A * xSq;

        var one = Fp.One;

        var rhs = one + dxySq;
        var lhs = aXSq + ySq;

        return lhs == rhs;
    }

    public byte[] ToBytes()
    {
        var xBytes = X.ToBytes();

        var mask = MCompressedPositive;
        if (Y.LexicographicallyLargest())
            mask = MCompressedNegative;

        xBytes[31] |= mask;
        return xBytes;
    }

    public AffinePoint Dup() => new (X.Dup(), Y.Dup());

    public static AffinePoint ScalarMultiplication(AffinePoint point, Fr scalar)
    {
        var result = Identity();
        var temp = point.Dup();

        var bytes = scalar.ToBytes();

        foreach (var idx in bytes)
        {
            var binaryString = Convert.ToString(bytes[idx], 2);
            for (var i = 0; i < 8; i++)
            {
                if (i < binaryString.Length && binaryString[i] == '1')
                {
                    result = Add(result, temp);
                }
                temp = Double(temp);
            }
        }

        return new AffinePoint(result.X, result.Y);
    }

    public static AffinePoint Identity() => new (Fp.Zero, Fp.One);

    public static Fp? GetYCoordinate(Fp x, bool returnPositiveY)
    {
        Fp one = Fp.One;
        var num = x * x;
        var den = (num * D) - one;
        num = (num * A) - one;

        var y = num / den;

        if (y is null)
            return null;

        y = Fp.Sqrt(y);
        if (y is null)
            return null;

        var isLargest = y.LexicographicallyLargest();

        return isLargest == returnPositiveY ? y : y.Neg();
    }
    
    public static AffinePoint operator +(in AffinePoint a, in AffinePoint b)
    {
        return Add(a, b);
    }

    public static AffinePoint operator -(in AffinePoint a, in AffinePoint b)
    {
        return Sub(a, b);
    }

    public static AffinePoint operator *(in AffinePoint a, in Fr b)
    {
        return ScalarMultiplication(a, b);
    }
    
    public static AffinePoint operator *(in Fr a, in AffinePoint b)
    {
        return ScalarMultiplication(b, a);
    }
    
    public static bool operator ==(in AffinePoint a, in AffinePoint b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(in AffinePoint a, in AffinePoint b)
    {
        return !(a == b);
    }

    private bool Equals(AffinePoint a)
    {
        return X == a.X && Y == a.Y;
    }
    
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == this.GetType() && Equals((AffinePoint) obj);
    }

    public override int GetHashCode() => HashCode.Combine(X, Y);
}