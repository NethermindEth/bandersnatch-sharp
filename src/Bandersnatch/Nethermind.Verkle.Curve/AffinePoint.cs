using Nethermind.Field;

namespace Nethermind.Verkle.Curve;
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
        Fp? yTe = CurveParams.YTe.Dup();
        Fp? xTe = CurveParams.XTe.Dup();
        return new AffinePoint(xTe, yTe);
    }

    public static AffinePoint Neg(AffinePoint p)
    {
        return new AffinePoint(p.X.Neg(), p.Y);
    }

    public static AffinePoint Add(AffinePoint p, AffinePoint q)
    {
        Fp? x1 = p.X;
        Fp? y1 = p.Y;
        Fp? x2 = q.X;
        Fp? y2 = q.Y;

        Fp? x1y2 = x1 * y2;
        Fp? y1x2 = y1 * x2;
        Fp? ax1x2 = x1 * x2 * A;
        Fp? y1y2 = y1 * y2;

        Fp? dx1x2y1y2 = x1y2 * y1x2 * D;

        Fp? xNum = x1y2 + y1x2;

        Fp? xDen = Fp.One + dx1x2y1y2;

        Fp? yNum = y1y2 - ax1x2;

        Fp? yDen = Fp.One - dx1x2y1y2;

        Fp? x = xNum / xDen ?? throw new Exception();

        Fp? y = yNum / yDen ?? throw new Exception();

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
        Fp? xSq = X * X;
        Fp? ySq = Y * Y;

        Fp? dxySq = xSq * ySq * D;
        Fp? aXSq = A * xSq;

        Fp? one = Fp.One;

        Fp? rhs = one + dxySq;
        Fp? lhs = aXSq + ySq;

        return lhs == rhs;
    }

    public byte[] ToBytes()
    {
        byte[]? xBytes = X.ToBytes();

        byte mask = MCompressedPositive;
        if (Y.LexicographicallyLargest())
            mask = MCompressedNegative;

        xBytes[31] |= mask;
        return xBytes;
    }

    public AffinePoint Dup() => new(X.Dup(), Y.Dup());

    public static AffinePoint ScalarMultiplication(AffinePoint point, Fr scalar)
    {
        AffinePoint? result = Identity();
        AffinePoint? temp = point.Dup();

        byte[]? bytes = scalar.ToBytes();

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

    public static AffinePoint Identity() => new(Fp.Zero, Fp.One);

    public static Fp? GetYCoordinate(Fp x, bool returnPositiveY)
    {
        Fp one = Fp.One;
        Fp? num = x * x;
        Fp? den = (num * D) - one;
        num = (num * A) - one;

        Fp? y = num / den;

        if (y is null)
            return null;

        y = Fp.Sqrt(y);
        if (y is null)
            return null;

        bool isLargest = y.LexicographicallyLargest();

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
        return obj.GetType() == this.GetType() && Equals((AffinePoint)obj);
    }

    public override int GetHashCode() => HashCode.Combine(X, Y);
}
