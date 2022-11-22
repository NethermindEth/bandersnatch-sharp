using Nethermind.Field;
using Nethermind.Field.Montgomery;

namespace Nethermind.Verkle.Curve;



public class AffinePoint
{
    public readonly FpE X;
    public readonly FpE Y;

    private static FpE A => CurveParams.A;
    private static FpE D => CurveParams.D;

    private const byte MCompressedNegative = 128;
    private const byte MCompressedPositive = 0;

    public AffinePoint(FpE x, FpE y)
    {
        X = x;
        Y = y;
    }

    public static AffinePoint Generator()
    {
        FpE yTe = CurveParams.YTe.Dup();
        FpE xTe = CurveParams.XTe.Dup();
        return new AffinePoint(xTe, yTe);
    }

    public static AffinePoint Neg(AffinePoint p)
    {
        return new AffinePoint(p.X.Neg(), p.Y);
    }

    public static AffinePoint Add(AffinePoint p, AffinePoint q)
    {
        FpE x1 = p.X;
        FpE y1 = p.Y;
        FpE x2 = q.X;
        FpE y2 = q.Y;

        FpE x1y2 = x1 * y2;
        FpE y1x2 = y1 * x2;
        FpE ax1x2 = x1 * x2 * A;
        FpE y1y2 = y1 * y2;

        FpE dx1x2y1y2 = x1y2 * y1x2 * D;

        FpE xNum = x1y2 + y1x2;

        FpE xDen = FpE.One + dx1x2y1y2;

        FpE yNum = y1y2 - ax1x2;

        FpE yDen = FpE.One - dx1x2y1y2;

        FpE x = xNum / xDen;

        FpE y = yNum / yDen;

        return new AffinePoint(x, y);
    }

    public static AffinePoint Sub(AffinePoint p, AffinePoint q)
    {
        AffinePoint? negQ = Neg(q);
        return Add(p, negQ);
    }

    public static AffinePoint Double(AffinePoint p)
    {
        return Add(p, p);
    }

    public bool IsOnCurve()
    {
        FpE xSq = X * X;
        FpE ySq = Y * Y;

        FpE dxySq = xSq * ySq * D;
        FpE aXSq = A * xSq;

        FpE one = FpE.One;

        FpE rhs = one + dxySq;
        FpE lhs = aXSq + ySq;

        return lhs.Equals(rhs);
    }

    public byte[] ToBytes()
    {
        byte[] xBytes = X.ToBytes().ToArray();

        byte mask = MCompressedPositive;
        if (Y.LexicographicallyLargest())
            mask = MCompressedNegative;

        xBytes[31] |= mask;
        return xBytes;
    }

    public AffinePoint Dup() => new(X.Dup(), Y.Dup());

    public static AffinePoint ScalarMultiplication(AffinePoint point, FrE scalar)
    {
        AffinePoint? result = Identity();
        AffinePoint? temp = point.Dup();

        byte[] bytes = scalar.ToBytes().ToArray();

        foreach (byte idx in bytes)
        {
            string? binaryString = Convert.ToString(bytes[idx], 2);
            for (int i = 0; i < 8; i++)
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

    public static AffinePoint Identity() => new(FpE.Zero, FpE.One);

    public static FpE? GetYCoordinate(FpE x, bool returnPositiveY)
    {
        FpE one = FpE.One;
        FpE? num = x * x;
        FpE? den = (num * D) - one;
        num = (num * A) - one;

        FpE? y = num / den;

        if (y is null)
            return null;

        if (!FpE.Sqrt(y.Value, out var z))
            return null;

        bool isLargest = z.LexicographicallyLargest();

        return isLargest == returnPositiveY ? z : z.Neg();
    }

    public static AffinePoint operator +(in AffinePoint a, in AffinePoint b)
    {
        return Add(a, b);
    }

    public static AffinePoint operator -(in AffinePoint a, in AffinePoint b)
    {
        return Sub(a, b);
    }

    public static AffinePoint operator *(in AffinePoint a, in FrE b)
    {
        return ScalarMultiplication(a, b);
    }

    public static AffinePoint operator *(in FrE a, in AffinePoint b)
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
        return X.Equals(a.X) && Y.Equals(a.Y);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == this.GetType() && Equals((AffinePoint)obj);
    }

    public override int GetHashCode() => HashCode.Combine(X, Y);
}
