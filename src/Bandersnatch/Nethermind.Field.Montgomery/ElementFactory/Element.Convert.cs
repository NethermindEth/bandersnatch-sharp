using System.Numerics;
using Nethermind.Int256;

namespace Nethermind.Field.Montgomery.ElementFactory;
public readonly partial struct Element
{
    public Element Dup()
    {
        return new Element(u0, u1, u2, u3);
    }

    public bool Bit(int n)
    {
        int bucket = (n / 64) % 4;
        int position = n % 64;
        return (this[bucket] & ((ulong)1 << position)) != 0;
    }

    public int BitLen()
    {
        return u3 != 0
            ? 192 + ElementUtils.Len64(u3)
            : u2 != 0
                ? 128 + ElementUtils.Len64(u2)
                : u1 != 0
                    ? 64 + ElementUtils.Len64(u1)
                    : ElementUtils.Len64(u0);
    }

    public static Element SetElement(ulong u0 = 0, ulong u1 = 0, ulong u2 = 0, ulong u3 = 0)
    {
        Element newElem = new Element(u0, u1, u2, u3);
        ToMontgomery(in newElem, out Element res);
        return res;
    }

    public static Element SetElement(BigInteger value)
    {
        Element newElem = new Element(value);
        ToMontgomery(in newElem, out Element res);
        return res;
    }

    public Span<byte> ToBytes()
    {
        ToRegular(in this, out Element x);
        return ElementUtils.ToLittleEndian(x.u0, x.u1, x.u2, x.u3);
    }

    public Span<byte> ToBytesBigEndian()
    {
        ToRegular(in this, out Element x);
        return ElementUtils.ToBigEndian(x.u0, x.u1, x.u2, x.u3);
    }

    public static Element FromBytes(byte[] byteEncoded, bool isBigEndian = false)
    {
        UInt256 val = new UInt256(byteEncoded, isBigEndian);
        if (val > _modulus.Value) throw new ArgumentException("FromBytes: byteEncoded should be less than modulus - use FromBytesReduced instead.");
        Element inp = new Element(val.u0, val.u1, val.u2, val.u3);
        ToMontgomery(inp, out Element resF);
        return resF;
    }

    public static Element FromBytesReduced(byte[] byteEncoded, bool isBigEndian = false)
    {
        UInt256 val = new UInt256(byteEncoded, isBigEndian);
        val.Mod(_modulus.Value, out UInt256 res);
        Element inp = new Element(res.u0, res.u1, res.u2, res.u3);
        ToMontgomery(inp, out Element resF);
        return resF;
    }
    public static void ToMontgomery(in Element x, out Element z)
    {
        MultiplyMod(x, rSquare, out z);
    }

    public static void ToRegular(in Element x, out Element z)
    {
        FromMontgomery(in x, out z);
    }

    public static void FromMontgomery(in Element x, out Element res)
    {
        ulong[] z = new ulong[4];
        z[0] = x[0];
        z[1] = x[1];
        z[2] = x[2];
        z[3] = x[3];

        ulong m = z[0] * QInvNeg;
        ulong c = ElementUtils.MAdd0(m, Q0, z[0]);
        (c, z[0]) = ElementUtils.MAdd2(m, Q1, z[1], c);
        (c, z[1]) = ElementUtils.MAdd2(m, Q2, z[2], c);
        (c, z[2]) = ElementUtils.MAdd2(m, Q3, z[3], c);
        z[3] = c;

        m = z[0] * QInvNeg;
        c = ElementUtils.MAdd0(m, Q0, z[0]);
        (c, z[0]) = ElementUtils.MAdd2(m, Q1, z[1], c);
        (c, z[1]) = ElementUtils.MAdd2(m, Q2, z[2], c);
        (c, z[2]) = ElementUtils.MAdd2(m, Q3, z[3], c);
        z[3] = c;

        m = z[0] * QInvNeg;
        c = ElementUtils.MAdd0(m, Q0, z[0]);
        (c, z[0]) = ElementUtils.MAdd2(m, Q1, z[1], c);
        (c, z[1]) = ElementUtils.MAdd2(m, Q2, z[2], c);
        (c, z[2]) = ElementUtils.MAdd2(m, Q3, z[3], c);
        z[3] = c;

        m = z[0] * QInvNeg;
        c = ElementUtils.MAdd0(m, Q0, z[0]);
        (c, z[0]) = ElementUtils.MAdd2(m, Q1, z[1], c);
        (c, z[1]) = ElementUtils.MAdd2(m, Q2, z[2], c);
        (c, z[2]) = ElementUtils.MAdd2(m, Q3, z[3], c);
        z[3] = c;

        if (LessThan(qElement, z))
        {
            ElementUtils.SubtractUnderflow(z[0], z[1], z[2], z[3], Q0, Q1, Q2, Q3, out z[0], out z[1], out z[2], out z[3]);
        }
        res = z;
    }
}
