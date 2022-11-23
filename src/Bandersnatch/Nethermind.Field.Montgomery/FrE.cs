// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Nethermind.Int256;

namespace Nethermind.Field.Montgomery;

[StructLayout(LayoutKind.Explicit)]
public readonly struct FrE
{
    const int Limbs = 4;
    const int Bits = 253;
    const int Bytes = Limbs * 8;
    private const ulong SqrtR = 5;
    const ulong QInvNeg = 17410672245482742751;

    public static readonly FrE Zero = new FrE(0, 0, 0, 0);

    private const ulong One0 = 6347764673676886264;
    private const ulong One1 = 253265890806062196;
    private const ulong One2 = 11064306276430008312;
    private const ulong One3 = 1739710354780652911;
    public static readonly FrE One = new FrE(One0, One1, One2, One3);

    private const ulong Q0 = 8429901452645165025;
    private const ulong Q1 = 18415085837358793841;
    private const ulong Q2 = 922804724659942912;
    private const ulong Q3 = 2088379214866112338;
    private static readonly FrE qElement = new FrE(Q0, Q1, Q2, Q3);

    private const ulong R0 = 15831548891076708299;
    private const ulong R1 = 4682191799977818424;
    private const ulong R2 = 12294384630081346794;
    private const ulong R3 = 785759240370973821;
    private static readonly FrE rSquare = new FrE(R0, R1, R2, R3);

    private const ulong G0 = 5415081136944170355;
    private const ulong G1 = 16923187137941795325;
    private const ulong G2 = 11911047149493888393;
    private const ulong G3 = 436996551065533341;
    private static readonly FrE gResidue = new FrE(G0, G1, G2, G3);

    private const ulong QM0 = 5415081136944170355;
    private const ulong QM1 = 16923187137941795325;
    private const ulong QM2 = 11911047149493888393;
    private const ulong QM3 = 436996551065533341;
    private static readonly FrE qMinOne = new FrE(QM0, QM1, QM2, QM3);

    public static Lazy<UInt256> _modulus = new Lazy<UInt256>(() =>
    {
        UInt256.TryParse("13108968793781547619861935127046491459309155893440570251786403306729687672801", out UInt256 output);
        return output;
    });
    public static Lazy<UInt256> _bLegendreExponentElement = new Lazy<UInt256>(() =>
    {
        UInt256 output = new UInt256(Convert.FromHexString("e7db4ea6533afa906673b0101343b007fc7c3803a0c8238ba7e835a943b73f0"), true);
        return output;
    });
    public static Lazy<UInt256> _bSqrtExponentElement = new Lazy<UInt256>(() =>
    {
        UInt256 output = new UInt256(Convert.FromHexString("73eda753299d7d483339d80809a1d803fe3e1c01d06411c5d3f41ad4a1db9f"), true);
        return output;
    });

    /* in little endian order so u3 is the most significant ulong */
    [FieldOffset(0)]
    public readonly ulong u0;
    [FieldOffset(8)]
    public readonly ulong u1;
    [FieldOffset(16)]
    public readonly ulong u2;
    [FieldOffset(24)]
    public readonly ulong u3;

    public ulong this[int index]
    {
        get
        {
            return index switch
            {
                0 => u0,
                1 => u1,
                2 => u2,
                3 => u3,
                var _ => throw new IndexOutOfRangeException()
            };
        }
    }

    internal FrE(ulong u0 = 0, ulong u1 = 0, ulong u2 = 0, ulong u3 = 0)
    {
        this.u0 = u0;
        this.u1 = u1;
        this.u2 = u2;
        this.u3 = u3;
    }

    public static FrE SetElement(ulong u0 = 0, ulong u1 = 0, ulong u2 = 0, ulong u3 = 0)
    {
        FrE newElem = new FrE(u0, u1, u2, u3);
        ToMont(in newElem, out FrE res);
        return res;
    }

    public static FrE SetElement(BigInteger value)
    {
        FrE newElem = new FrE(value);
        ToMont(in newElem, out FrE res);
        return res;
    }

    public FrE(in ReadOnlySpan<byte> bytes, bool isBigEndian = false)
    {
        UInt256 val = new UInt256(bytes, isBigEndian);
        val.Mod(_modulus.Value, out UInt256 res);
        FrE inp = new FrE(res.u0, res.u1, res.u2, res.u3);
        ToMont(inp, out this);
    }

    public FrE Dup()
    {
        return new FrE(u0, u1, u2, u3);
    }

    internal FrE(BigInteger value)
    {
        UInt256 res;
        if (value.Sign < 0)
        {
            ElementUtils.SubtractMod(UInt256.Zero, (UInt256)(-value), _modulus.Value, out res);
        }
        else
        {
            UInt256.Mod((UInt256)value, _modulus.Value, out res);
        }
        u0 = res.u0;
        u1 = res.u1;
        u2 = res.u2;
        u3 = res.u3;
    }

    public static IEnumerable<FrE> SetRandom()
    {
        byte[] data = new byte[32];
        Random rand = new(0);
        rand.NextBytes(data);
        yield return new FrE(data);
    }

    public FrE Neg()
    {
        SubMod(FrE.Zero, this, out FrE res);
        return res;
    }

    public bool LexicographicallyLargest()
    {
        FromMont(in this, out FrE mont);
        return !SubtractUnderflow(mont, qMinOne, out FrE _);
    }

    public Span<byte> ToBytes()
    {
        ToRegular(in this, out FrE x);
        return ElementUtils.ToLittleEndian(x.u0, x.u1, x.u2, x.u3);
    }

    public Span<byte> ToBytesBigEndian()
    {
        ToRegular(in this, out FrE x);
        return ElementUtils.ToBigEndian(x.u0, x.u1, x.u2, x.u3);
    }

    public static FrE? FromBytes(byte[] byteEncoded, bool isBigEndian = false)
    {
        UInt256 val = new UInt256(byteEncoded, isBigEndian);
        if (val > _modulus.Value) return null;
        FrE inp = new FrE(val.u0, val.u1, val.u2, val.u3);
        ToMont(inp, out FrE resF);
        return resF;
    }

    public static FrE FromBytesReduced(byte[] byteEncoded, bool isBigEndian = false)
    {
        UInt256 val = new UInt256(byteEncoded, isBigEndian);
        val.Mod(_modulus.Value, out UInt256 res);
        FrE inp = new FrE(res.u0, res.u1, res.u2, res.u3);
        ToMont(inp, out FrE resF);
        return resF;
    }

    public bool IsZero => (u0 | u1 | u2 | u3) == 0;

    public bool IsOne => Equals(One);

    public static bool Sqrt(in FrE x, out FrE z)
    {
        Exp(in x, _bSqrtExponentElement.Value, out var w);
        MulMod(x, w, out var y);
        MulMod(w, y, out var b);

        ulong r = SqrtR;
        FrE t = b;

        for (ulong i = 0; i < r - 1; i++)
        {
            MulMod(in t, in t, out t);
        }

        if (t.IsZero)
        {
            z = Zero;
            return true;
        }

        if (!t.IsOne)
        {
            z = Zero;
            return false;
        }

        FrE g = gResidue;
        while (true)
        {
            ulong m = 0;
            t = b;

            while (!t.IsOne)
            {
                MulMod(in t, in t, out t);
                m++;
            }

            if (m == 0)
            {
                z = y;
                return true;
            }
            int ge = (int)(r - m - 1);
            t = g;

            while (ge > 0)
            {
                MulMod(in t, in t, out t);
                ge--;
            }

            MulMod(in t, in t, out g);
            MulMod(in y, in t, out y);
            MulMod(in b, in g, out b);
            r = m;
        }
    }

    public static int Legendre(in FrE z)
    {
        Exp(z, _bLegendreExponentElement.Value, out FrE res);
        if (res.IsZero) return 0;

        if (res.IsOne) return 1;
        return -1;
    }

    public static void Exp(in FrE b, in UInt256 e, out FrE result)
    {
        result = One;
        FrE bs = b;
        int len = e.BitLen;
        for (int i = 0; i < len; i++)
        {
            if (e.Bit(i))
            {
                MulMod(result, bs, out result);
            }
            MulMod(bs, bs, out bs);
        }
    }

    public static FrE[] MultiInverse(FrE[] values)
    {
        if (values.Length == 0) return Array.Empty<FrE>();

        FrE[] results = new FrE[values.Length];
        bool[] zeros = new bool[values.Length];

        FrE accumulator = One;

        for (int i = 0; i < values.Length; i++)
        {
            if (values[i].IsZero)
            {
                zeros[i] = true;
                continue;
            }
            results[i] = accumulator;
            MulMod(in accumulator, in values[i], out accumulator);
        }

        Inverse(in accumulator, out accumulator);

        for (int i = values.Length - 1; i >= 0; i--)
        {
            if (zeros[i]) continue;
            MulMod(in results[i], in accumulator, out results[i]);
            MulMod(in accumulator, in values[i], out accumulator);
        }

        return values;
    }

    public static void ToMont(in FrE x, out FrE z)
    {
        MulMod(x, rSquare, out z);
    }

    public static void ToRegular(in FrE x, out FrE z)
    {
        FromMont(in x, out z);
    }

    public static void FromMont(in FrE x, out FrE res)
    {
        ulong[] z = new ulong[4];
        z[0] = x[0];
        z[1] = x[1];
        z[2] = x[2];
        z[3] = x[3];
        ulong m, c;

        m = z[0] * QInvNeg;
        c = MAdd0(m, Q0, z[0]);
        (c, z[0]) = MAdd2(m, Q1, z[1], c);
        (c, z[1]) = MAdd2(m, Q2, z[2], c);
        (c, z[2]) = MAdd2(m, Q3, z[3], c);
        z[3] = c;

        m = z[0] * QInvNeg;
        c = MAdd0(m, Q0, z[0]);
        (c, z[0]) = MAdd2(m, Q1, z[1], c);
        (c, z[1]) = MAdd2(m, Q2, z[2], c);
        (c, z[2]) = MAdd2(m, Q3, z[3], c);
        z[3] = c;

        m = z[0] * QInvNeg;
        c = MAdd0(m, Q0, z[0]);
        (c, z[0]) = MAdd2(m, Q1, z[1], c);
        (c, z[1]) = MAdd2(m, Q2, z[2], c);
        (c, z[2]) = MAdd2(m, Q3, z[3], c);
        z[3] = c;

        m = z[0] * QInvNeg;
        c = MAdd0(m, Q0, z[0]);
        (c, z[0]) = MAdd2(m, Q1, z[1], c);
        (c, z[1]) = MAdd2(m, Q2, z[2], c);
        (c, z[2]) = MAdd2(m, Q3, z[3], c);
        z[3] = c;

        if (LessThan(qElement, z))
        {
            ulong b = 0;
            SubtractWithBorrow(z[0], Q0, ref b, out z[0]);
            SubtractWithBorrow(z[1], Q1, ref b, out z[1]);
            SubtractWithBorrow(z[2], Q2, ref b, out z[2]);
            SubtractWithBorrow(z[3], Q3, ref b, out z[3]);
        }
        res = z;
    }


    public static void Inverse(in FrE x, out FrE z)
    {
        if (x.IsZero)
        {
            z = Zero;
            return;
        }

        // initialize u = q
        FrE u = qElement;
        // initialize s = r^2
        FrE s = rSquare;
        FrE r = new FrE();
        FrE v = x;


        while (true)
        {
            while ((v[0] & 1) == 0)
            {
                v >>= 1;
                if ((s[0] & 1) == 1) Add(in s, in qElement, out s);
                s >>= 1;
            }

            while ((u[0] & 1) == 0)
            {
                u >>= 1;
                if ((r[0] & 1) == 1) Add(in r, in qElement, out r);
                r >>= 1;
            }

            if (!LessThan(v, u))
            {
                SubtractUnderflow(in v, in u, out v);
                if (SubtractUnderflow(s, r, out s)) Add(in s, in qElement, out s);
            }
            else
            {
                SubtractUnderflow(in u, in v, out u);
                if (SubtractUnderflow(r, s, out r)) Add(in r, in qElement, out r);
            }


            if ((u[0] == 1) && ((u[3] | u[2] | u[1]) == 0))
            {
                z = r;
                return;
            }
            if ((v[0] == 1) && ((v[3] | v[2] | v[1]) == 0))
            {
                z = s;
                return;
            }
        }
    }


    public static void MulMod(in FrE x, in FrE y, out FrE res)
    {
        ulong[] t = new ulong[4];
        ulong[] c = new ulong[3];
        ulong[] z = new ulong[4];

        {
            // round 0

            ulong v = x[0];
            (c[1], c[0]) = Multiply64(v, y[0]);
            ulong m = c[0] * QInvNeg;
            c[2] = MAdd0(m, Q0, c[0]);
            (c[1], c[0]) = MAdd1(v, y[1], c[1]);
            (c[2], t[0]) = MAdd2(m, Q1, c[2], c[0]);
            (c[1], c[0]) = MAdd1(v, y[2], c[1]);
            (c[2], t[1]) = MAdd2(m, Q2, c[2], c[0]);
            (c[1], c[0]) = MAdd1(v, y[3], c[1]);
            (t[3], t[2]) = MAdd3(m, Q3, c[0], c[2], c[1]);
        }
        {
            // round 1
            ulong v = x[1];
            (c[1], c[0]) = MAdd1(v, y[0], t[0]);
            ulong m = (c[0] * QInvNeg);
            c[2] = MAdd0(m, Q0, c[0]);
            (c[1], c[0]) = MAdd2(v, y[1], c[1], t[1]);
            (c[2], t[0]) = MAdd2(m, Q1, c[2], c[0]);
            (c[1], c[0]) = MAdd2(v, y[2], c[1], t[2]);
            (c[2], t[1]) = MAdd2(m, Q2, c[2], c[0]);
            (c[1], c[0]) = MAdd2(v, y[3], c[1], t[3]);
            (t[3], t[2]) = MAdd3(m, Q3, c[0], c[2], c[1]);
        }
        {
            // round 2

            ulong v = x[2];
            (c[1], c[0]) = MAdd1(v, y[0], t[0]);
            ulong m = (c[0] * QInvNeg);
            c[2] = MAdd0(m, Q0, c[0]);
            (c[1], c[0]) = MAdd2(v, y[1], c[1], t[1]);
            (c[2], t[0]) = MAdd2(m, Q1, c[2], c[0]);
            (c[1], c[0]) = MAdd2(v, y[2], c[1], t[2]);
            (c[2], t[1]) = MAdd2(m, Q2, c[2], c[0]);
            (c[1], c[0]) = MAdd2(v, y[3], c[1], t[3]);
            (t[3], t[2]) = MAdd3(m, Q3, c[0], c[2], c[1]);
        }
        {
            // round 3

            ulong v = x[3];
            (c[1], c[0]) = MAdd1(v, y[0], t[0]);
            ulong m = (c[0] * QInvNeg);
            c[2] = MAdd0(m, Q0, c[0]);
            (c[1], c[0]) = MAdd2(v, y[1], c[1], t[1]);
            (c[2], z[0]) = MAdd2(m, Q1, c[2], c[0]);
            (c[1], c[0]) = MAdd2(v, y[2], c[1], t[2]);
            (c[2], z[1]) = MAdd2(m, Q2, c[2], c[0]);
            (c[1], c[0]) = MAdd2(v, y[3], c[1], t[3]);
            (z[3], z[2]) = MAdd3(m, Q3, c[0], c[2], c[1]);
        }
        if (LessThan(qElement, z))
        {
            ulong b = 0;
            SubtractWithBorrow(z[0], Q0, ref b, out z[0]);
            SubtractWithBorrow(z[1], Q1, ref b, out z[1]);
            SubtractWithBorrow(z[2], Q2, ref b, out z[2]);
            SubtractWithBorrow(z[3], Q3, ref b, out z[3]);
        }
        res = z;
    }

    public static void AddMod(in FrE a, in FrE b, out FrE res)
    {
        Add(a, b, out FrE z);
        if (LessThan(qElement, z))
            res = z - qElement;
        else
            res = z;

    }

    public static void SubMod(in FrE a, in FrE b, out FrE res)
    {
        if (SubtractUnderflow(a, b, out res)) res += qElement;
    }

    public static void Divide(in FrE x, in FrE y, out FrE z)
    {
        Inverse(y, out FrE yInv);
        MulMod(x, yInv, out z);
    }

    public static void Lsh(in FrE x, int n, out FrE res)
    {
        if ((n % 64) == 0)
        {
            switch (n)
            {
                case 0:
                    res = x;
                    return;
                case 64:
                    x.Lsh64(out res);
                    return;
                case 128:
                    x.Lsh128(out res);
                    return;
                case 192:
                    x.Lsh192(out res);
                    return;
                default:
                    res = Zero;
                    return;
            }
        }

        res = Zero;
        ulong z0 = res.u0, z1 = res.u1, z2 = res.u2, z3 = res.u3;
        ulong a = 0, b = 0;
        // Big swaps first
        if (n > 192)
        {
            if (n > 256)
            {
                res = Zero;
                return;
            }

            x.Lsh192(out res);
            n -= 192;
            goto sh192;
        }
        else if (n > 128)
        {
            x.Lsh128(out res);
            n -= 128;
            goto sh128;
        }
        else if (n > 64)
        {
            x.Lsh64(out res);
            n -= 64;
            goto sh64;
        }
        else
        {
            res = x;
        }

        // remaining shifts
        a = Rsh(res.u0, 64 - n);
        z0 = Lsh(res.u0, n);

sh64:
        b = Rsh(res.u1, 64 - n);
        z1 = Lsh(res.u1, n) | a;

sh128:
        a = Rsh(res.u2, 64 - n);
        z2 = Lsh(res.u2, n) | b;

sh192:
        z3 = Lsh(res.u3, n) | a;

        res = new FrE(z0, z1, z2, z3);
    }

    public void LeftShift(int n, out FrE res)
    {
        Lsh(this, n, out res);
    }



    public bool Bit(int n)
    {
        int bucket = (n / 64) % 4;
        int position = n % 64;
        return (this[bucket] & ((ulong)1 << position)) != 0;
    }
    public int BitLen =>
        u3 != 0
            ? 192 + Len64(u3)
            : u2 != 0
                ? 128 + Len64(u2)
                : u1 != 0
                    ? 64 + Len64(u1)
                    : Len64(u0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Len64(ulong x)
    {
        int n = 0;
        if (x >= (1ul << 32))
        {
            x >>= 32;
            n = 32;
        }
        if (x >= (1ul << 16))
        {
            x >>= 16;
            n += 16;
        }
        if (x >= (1ul << 8))
        {
            x >>= 8;
            n += 8;
        }

        return n + len8tab[x];
    }
    private static readonly byte[] len8tab = new byte[] {
        0x00, 0x01, 0x02, 0x02, 0x03, 0x03, 0x03, 0x03, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x04,
        0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05,
        0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06,
        0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x06,
        0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
        0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
        0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
        0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07,
        0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08,
        0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08,
        0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08,
        0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08,
        0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08,
        0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08,
        0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08,
        0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x08,
    };


    public static void Rsh(in FrE x, int n, out FrE res)
    {
        // n % 64 == 0
        if ((n & 0x3f) == 0)
        {
            switch (n)
            {
                case 0:
                    res = x;
                    return;
                case 64:
                    x.Rsh64(out res);
                    return;
                case 128:
                    x.Rsh128(out res);
                    return;
                case 192:
                    x.Rsh192(out res);
                    return;
                default:
                    res = Zero;
                    return;
            }
        }

        res = Zero;
        ulong z0 = res.u0, z1 = res.u1, z2 = res.u2, z3 = res.u3;
        ulong a = 0, b = 0;
        // Big swaps first
        if (n > 192)
        {
            if (n > 256)
            {
                res = Zero;
                return;
            }

            x.Rsh192(out res);
            z0 = res.u0;
            z1 = res.u1;
            z2 = res.u2;
            z3 = res.u3;
            n -= 192;
            goto sh192;
        }
        else if (n > 128)
        {
            x.Rsh128(out res);
            z0 = res.u0;
            z1 = res.u1;
            z2 = res.u2;
            z3 = res.u3;
            n -= 128;
            goto sh128;
        }
        else if (n > 64)
        {
            x.Rsh64(out res);
            z0 = res.u0;
            z1 = res.u1;
            z2 = res.u2;
            z3 = res.u3;
            n -= 64;
            goto sh64;
        }
        else
        {
            res = x;
            z0 = res.u0;
            z1 = res.u1;
            z2 = res.u2;
            z3 = res.u3;
        }

        // remaining shifts
        a = Lsh(res.u3, 64 - n);
        z3 = Rsh(res.u3, n);

sh64:
        b = Lsh(res.u2, 64 - n);
        z2 = Rsh(res.u2, n) | a;

sh128:
        a = Lsh(res.u1, 64 - n);
        z1 = Rsh(res.u1, n) | b;

sh192:
        z0 = Rsh(res.u0, n) | a;

        res = new FrE(z0, z1, z2, z3);
    }

    public void RightShift(int n, out FrE res) => Rsh(this, n, out res);



    internal void Lsh64(out FrE res)
    {
        res = new FrE(0, u0, u1, u2);
    }

    internal void Lsh128(out FrE res)
    {
        res = new FrE(0, 0, u0, u1);
    }

    internal void Lsh192(out FrE res)
    {
        res = new FrE(0, 0, 0, u0);
    }

    internal void Rsh64(out FrE res)
    {
        res = new FrE(u1, u2, u3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Rsh128(out FrE res)
    {
        res = new FrE(u2, u3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Rsh192(out FrE res)
    {
        res = new FrE(u3);
    }

    // It avoids c#'s way of shifting a 64-bit number by 64-bit, i.e. in c# a << 64 == a, in our version a << 64 == 0.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Lsh(ulong a, int n)
    {
        var n1 = n >> 1;
        var n2 = n - n1;
        return (a << n1) << n2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Rsh(ulong a, int n)
    {
        var n1 = n >> 1;
        var n2 = n - n1;
        return (a >> n1) >> n2;
    }


    // Add sets res to the sum a+b
    public static void Add(in FrE a, in FrE b, out FrE res)
    {
        ulong carry = 0ul;
        AddWithCarry(a.u0, b.u0, ref carry, out ulong res1);
        AddWithCarry(a.u1, b.u1, ref carry, out ulong res2);
        AddWithCarry(a.u2, b.u2, ref carry, out ulong res3);
        AddWithCarry(a.u3, b.u3, ref carry, out ulong res4);
        res = new FrE(res1, res2, res3, res4);
    }
    public static bool SubtractUnderflow(in FrE a, in FrE b, out FrE res)
    {
        ulong borrow = 0;
        SubtractWithBorrow(a[0], b[0], ref borrow, out ulong z0);
        SubtractWithBorrow(a[1], b[1], ref borrow, out ulong z1);
        SubtractWithBorrow(a[2], b[2], ref borrow, out ulong z2);
        SubtractWithBorrow(a[3], b[3], ref borrow, out ulong z3);
        res = new FrE(z0, z1, z2, z3);
        return borrow != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SubtractWithBorrow(ulong a, ulong b, ref ulong borrow, out ulong res)
    {
        res = a - b - borrow;
        borrow = (((~a) & b) | (~(a ^ b)) & res) >> 63;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddWithCarry(ulong x, ulong y, ref ulong carry, out ulong sum)
    {
        sum = x + y + carry;
        // both msb bits are 1 or one of them is 1 and we had carry from lower bits
        carry = ((x & y) | ((x | y) & (~sum))) >> 63;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static (ulong high, ulong low) Multiply64(ulong a, ulong b)
    {
        ulong high = Math.BigMul(a, b, out ulong low);
        return (high, low);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong MAdd0(ulong a, ulong b, ulong c)
    {
        ulong carry = 0;
        (ulong hi, ulong lo) = Multiply64(a, b);
        AddWithCarry(lo, c, ref carry, out lo);
        AddWithCarry(hi, 0, ref carry, out hi);
        return hi;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (ulong, ulong) MAdd1(ulong a, ulong b, ulong c)
    {
        (ulong hi, ulong lo) = Multiply64(a, b);
        ulong carry = 0;
        AddWithCarry(lo, c, ref carry, out lo);
        AddWithCarry(hi, 0, ref carry, out hi);
        return (hi, lo);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (ulong, ulong) MAdd2(ulong a, ulong b, ulong c, ulong d)
    {
        (ulong hi, ulong lo) = Multiply64(a, b);
        ulong carry = 0;
        AddWithCarry(c, d, ref carry, out c);
        AddWithCarry(hi, 0, ref carry, out hi);
        carry = 0;
        AddWithCarry(lo, c, ref carry, out lo);
        AddWithCarry(hi, 0, ref carry, out hi);
        return (hi, lo);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (ulong, ulong) MAdd3(ulong a, ulong b, ulong c, ulong d, ulong e)
    {
        (ulong hi, ulong lo) = Multiply64(a, b);
        ulong carry = 0;
        AddWithCarry(c, d, ref carry, out c);
        AddWithCarry(hi, 0, ref carry, out hi);
        carry = 0;
        AddWithCarry(lo, c, ref carry, out lo);
        AddWithCarry(hi, e, ref carry, out hi);
        return (hi, lo);
    }

    public static implicit operator FrE(ulong value) => new FrE(value, 0ul, 0ul, 0ul);
    public static implicit operator FrE(ulong[] value) => new FrE(value[0], value[1], value[2], value[3]);

    public static explicit operator FrE(in BigInteger value)
    {
        byte[] bytes32 = value.ToBytes32(true);
        return new FrE(bytes32, true);
    }

    public static FrE operator +(in FrE a, in FrE b)
    {
        AddMod(in a, in b, out FrE res);
        return res;
    }

    public static FrE operator -(in FrE a, in FrE b)
    {
        SubMod(in a, in b, out FrE c);
        return c;
    }

    public static FrE operator >>(in FrE a, int n)
    {
        a.RightShift(n, out FrE res);
        return res;
    }
    public static FrE operator <<(in FrE a, int n)
    {
        a.LeftShift(n, out FrE res);
        return res;
    }

    public bool Equals(FrE other) => u0 == other.u0 && u1 == other.u1 && u2 == other.u2 && u3 == other.u3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Equals(in FrE other) =>
        u0 == other.u0 &&
        u1 == other.u1 &&
        u2 == other.u2 &&
        u3 == other.u3;

    public int CompareTo(FrE b) => this < b ? -1 : Equals(b) ? 0 : 1;

    public override int GetHashCode() => HashCode.Combine(u0, u1, u2, u3);


    public static FrE operator /(in FrE a, in FrE b)
    {
        Divide(in a, in b, out FrE c);
        return c;
    }

    public static bool operator <(in FrE a, in FrE b) => LessThan(in a, in b);
    public static bool operator <(in FrE a, int b) => LessThan(in a, b);
    public static bool operator <(int a, in FrE b) => LessThan(a, in b);
    public static bool operator <(in FrE a, uint b) => LessThan(in a, b);
    public static bool operator <(uint a, in FrE b) => LessThan(a, in b);
    public static bool operator <(in FrE a, long b) => LessThan(in a, b);
    public static bool operator <(long a, in FrE b) => LessThan(a, in b);
    public static bool operator <(in FrE a, ulong b) => LessThan(in a, b);
    public static bool operator <(ulong a, in FrE b) => LessThan(a, in b);
    public static bool operator <=(in FrE a, in FrE b) => !LessThan(in b, in a);
    public static bool operator <=(in FrE a, int b) => !LessThan(b, in a);
    public static bool operator <=(int a, in FrE b) => !LessThan(in b, a);
    public static bool operator <=(in FrE a, uint b) => !LessThan(b, in a);
    public static bool operator <=(uint a, in FrE b) => !LessThan(in b, a);
    public static bool operator <=(in FrE a, long b) => !LessThan(b, in a);
    public static bool operator <=(long a, in FrE b) => !LessThan(in b, a);
    public static bool operator <=(in FrE a, ulong b) => !LessThan(b, in a);
    public static bool operator <=(ulong a, FrE b) => !LessThan(in b, a);
    public static bool operator >(in FrE a, in FrE b) => LessThan(in b, in a);
    public static bool operator >(in FrE a, int b) => LessThan(b, in a);
    public static bool operator >(int a, in FrE b) => LessThan(in b, a);
    public static bool operator >(in FrE a, uint b) => LessThan(b, in a);
    public static bool operator >(uint a, in FrE b) => LessThan(in b, a);
    public static bool operator >(in FrE a, long b) => LessThan(b, in a);
    public static bool operator >(long a, in FrE b) => LessThan(in b, a);
    public static bool operator >(in FrE a, ulong b) => LessThan(b, in a);
    public static bool operator >(ulong a, in FrE b) => LessThan(in b, a);
    public static bool operator >=(in FrE a, in FrE b) => !LessThan(in a, in b);
    public static bool operator >=(in FrE a, int b) => !LessThan(in a, b);
    public static bool operator >=(int a, in FrE b) => !LessThan(a, in b);
    public static bool operator >=(in FrE a, uint b) => !LessThan(in a, b);
    public static bool operator >=(uint a, in FrE b) => !LessThan(a, in b);
    public static bool operator >=(in FrE a, long b) => !LessThan(in a, b);
    public static bool operator >=(long a, in FrE b) => !LessThan(a, in b);
    public static bool operator >=(in FrE a, ulong b) => !LessThan(in a, b);
    public static bool operator >=(ulong a, in FrE b) => !LessThan(a, in b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool LessThan(in FrE a, long b) => b >= 0 && a.u3 == 0 && a.u2 == 0 && a.u1 == 0 && a.u0 < (ulong)b;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool LessThan(long a, in FrE b) => a < 0 || b.u1 != 0 || b.u2 != 0 || b.u3 != 0 || (ulong)a < b.u0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool LessThan(in FrE a, ulong b) => a.u3 == 0 && a.u2 == 0 && a.u1 == 0 && a.u0 < b;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool LessThan(ulong a, in FrE b) => b.u3 != 0 || b.u2 != 0 || b.u1 != 0 || a < b.u0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool LessThan(in FrE a, in FrE b)
    {
        if (a.u3 != b.u3)
            return a.u3 < b.u3;
        if (a.u2 != b.u2)
            return a.u2 < b.u2;
        if (a.u1 != b.u1)
            return a.u1 < b.u1;
        return a.u0 < b.u0;
    }

    public static bool LessThanSubModulus(FrE x)
    {
        return LessThan(x, qElement);
    }

    public static bool operator ==(in FrE a, int b) => a.Equals(b);
    public static bool operator ==(int a, in FrE b) => b.Equals(a);
    public static bool operator ==(in FrE a, uint b) => a.Equals(b);
    public static bool operator ==(uint a, in FrE b) => b.Equals(a);
    public static bool operator ==(in FrE a, long b) => a.Equals(b);
    public static bool operator ==(long a, in FrE b) => b.Equals(a);
    public static bool operator ==(in FrE a, ulong b) => a.Equals(b);
    public static bool operator ==(ulong a, in FrE b) => b.Equals(a);
    public static bool operator !=(in FrE a, int b) => !a.Equals(b);
    public static bool operator !=(int a, in FrE b) => !b.Equals(a);
    public static bool operator !=(in FrE a, uint b) => !a.Equals(b);
    public static bool operator !=(uint a, in FrE b) => !b.Equals(a);
    public static bool operator !=(in FrE a, long b) => !a.Equals(b);
    public static bool operator !=(long a, in FrE b) => !b.Equals(a);
    public static bool operator !=(in FrE a, ulong b) => !a.Equals(b);
    public static bool operator !=(ulong a, in FrE b) => !b.Equals(a);

    public bool Equals(int other) => other >= 0 && u0 == (uint)other && u1 == 0 && u2 == 0 && u3 == 0;

    public bool Equals(uint other) => u0 == other && u1 == 0 && u2 == 0 && u3 == 0;

    public bool Equals(long other) => other >= 0 && u0 == (ulong)other && u1 == 0 && u2 == 0 && u3 == 0;

    public bool Equals(ulong other) => u0 == other && u1 == 0 && u2 == 0 && u3 == 0;


    public static FrE operator *(in FrE a, in FrE b)
    {
        MulMod(a, b, out FrE x);
        return x;
    }
}
