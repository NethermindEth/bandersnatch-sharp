// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using System.Runtime.CompilerServices;
using Nethermind.Int256;
using Nethermind.Verkle.Fields.FpEElement;
using Nethermind.Verkle.Fields.FrEElement;

namespace Nethermind.Verkle.Curve;

public readonly partial struct Banderwagon
{
    private static FpE A = CurveParams.A;
    private readonly ExtendedPoint _point;

    public static Banderwagon Identity = new(ExtendedPoint.Identity);
    public static Banderwagon Generator = new(ExtendedPoint.Generator);

    public Banderwagon(byte[]? serialisedBytesBigEndian, ExtendedPoint? unsafeBandersnatchPoint = null)
    {
        if (unsafeBandersnatchPoint is not null)
        {
            _point = unsafeBandersnatchPoint.Value;
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

    public static (FpE X, FpE Y)? FromBytes(byte[] serialisedBytesBigEndian)
    {
        FpE x = FpE.FromBytes(serialisedBytesBigEndian, true);

        FpE? y = AffinePoint.GetYCoordinate(x, true);
        if (y is null) return null;

        return SubgroupCheck(x) != 1 ? null : (x, y.Value);
    }

    public static int SubgroupCheck(FpE x)
    {
        FpE.MultiplyMod(x, x, out FpE res);
        FpE.MultiplyMod(res, A, out res);
        res = res.Negative();
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

    public static Banderwagon Neg(Banderwagon p)
    {
        return new Banderwagon(ExtendedPoint.Neg(p._point));
    }

    public static Banderwagon Add(Banderwagon p, Banderwagon q)
    {
        return new Banderwagon(p._point + q._point);
    }

    public static Banderwagon Sub(Banderwagon p, Banderwagon q)
    {
        return new Banderwagon(p._point - q._point);
    }

    public FrE MapToScalarField()
    {
        FpE.Inverse(in _point.Y, out FpE map);
        FpE.MultiplyMod(in _point.X, in map, out map);
        FpE.FromMontgomery(in map, out map);
        Unsafe.As<FpE, UInt256>(ref Unsafe.AsRef(in map)).Mod(FrE._modulus.Value, out UInt256 inter);
        return FrE.SetElement(inter.u0, inter.u1, inter.u2, inter.u3);
    }

    public FrE MapToScalarField(in FpE inv)
    {
        FpE.MultiplyMod(in _point.X, in inv, out FpE map);
        FpE.FromMontgomery(in map, out map);
        Unsafe.As<FpE, UInt256>(ref Unsafe.AsRef(in map)).Mod(FrE._modulus.Value, out UInt256 inter);
        return FrE.SetElement(inter.u0, inter.u1, inter.u2, inter.u3);
    }

    public static FrE[] BatchMapToScalarField(Banderwagon[] points)
    {
        FpE[] inverses = points.Select(x => x._point.Y).ToArray();
        inverses = FpE.MultiInverse(inverses);

        FrE[] fields = new FrE[points.Length];
        Parallel.For(0, points.Length, i =>
        {
            fields[i] = points[i].MapToScalarField(in inverses[i]);
        });
        return fields;
    }

    public byte[] ToBytes()
    {
        AffinePoint affine = _point.ToAffine();
        FpE x = affine.X;
        if (affine.Y.LexicographicallyLargest() == false)
        {
            x = affine.X.Negative();
        }

        return x.ToBytesBigEndian();
    }

    public byte[] ToBytesLittleEndian()
    {
        AffinePoint affine = _point.ToAffine();
        FpE x = affine.X;
        if (affine.Y.LexicographicallyLargest() == false)
        {
            x = affine.X.Negative();
        }

        return x.ToBytes();
    }

    public static Banderwagon Double(Banderwagon p)
    {
        return new Banderwagon(ExtendedPoint.Double(p._point));
    }

    public bool IsOnCurve()
    {
        return _point.ToAffine().IsOnCurve();
    }

    public static Banderwagon ScalarMul(in Banderwagon element, in FrE scalar)
    {
        return new Banderwagon(element._point * scalar);
    }



    public static Banderwagon TwoTorsionPoint()
    {
        AffinePoint affinePoint = new(FpE.Zero, FpE.One.Negative());
        return new Banderwagon(new ExtendedPoint(affinePoint.X, affinePoint.Y));
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
        return obj.GetType() == GetType() && Equals((Banderwagon)obj);
    }

    public override int GetHashCode()
    {
        return _point.GetHashCode();
    }
}