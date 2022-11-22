// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Nethermind.Int256;

namespace Nethermind.Field.Montgomery;

[StructLayout(LayoutKind.Explicit)]
public readonly struct FpE
{
    const int Limbs = 4;
    const int Bits = 255;
    const int Bytes = Limbs * 8;
    private const ulong SqrtR = 32;
    const ulong QInvNeg = 18446744069414584319;

    public static readonly FpE Zero = new FpE(0, 0, 0, 0);

    private const ulong One0 = 8589934590;
    private const ulong One1 = 6378425256633387010;
    private const ulong One2 = 11064306276430008309;
    private const ulong One3 = 1739710354780652911;
    public static readonly FpE One = new FpE(One0, One1, One2, One3);

    private const  ulong Q0 = 18446744069414584321;
    private const  ulong Q1 = 6034159408538082302;
    private const  ulong Q2 = 3691218898639771653;
    private const  ulong Q3 = 8353516859464449352;
    private static readonly FpE qElement = new FpE(Q0, Q1, Q2, Q3);

    private  const ulong R0 = 14526898881837571181;
    private  const ulong R1 = 3129137299524312099;
    private  const ulong R2 = 419701826671360399;
    private  const ulong R3 = 524908885293268753;
    private static readonly FpE rSquare = new FpE(R0, R1, R2, R3);

    private const ulong G0 = 11289237133041595516;
    private const ulong G1 = 2081200955273736677;
    private const ulong G2 = 967625415375836421;
    private const ulong G3 = 4543825880697944938;
    private static readonly FpE gResidue = new FpE(G0, G1, G2, G3);

    private const ulong QM0 = 9223372034707292161;
    private const ulong QM1 = 12240451741123816959;
    private const ulong QM2 = 1845609449319885826;
    private const ulong QM3 = 4176758429732224676;
    private static readonly FpE qMinOne = new FpE(QM0, QM1, QM2, QM3);

    public static Lazy<UInt256> _modulus = new Lazy<UInt256>(() =>
    {
        UInt256.TryParse("52435875175126190479447740508185965837690552500527637822603658699938581184513", out UInt256 output);
        return output;
    });
    public static Lazy<UInt256> _bLegendreExponentElement = new Lazy<UInt256>(() =>
    {
        UInt256 output = new UInt256(Convert.FromHexString("39f6d3a994cebea4199cec0404d0ec02a9ded2017fff2dff7fffffff80000000"), true);
        return output;
    });
    public static Lazy<UInt256> _bSqrtExponentElement = new Lazy<UInt256>(() =>
    {
        UInt256 output = new UInt256(Convert.FromHexString("39f6d3a994cebea4199cec0404d0ec02a9ded2017fff2dff7fffffff"), true);
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

    internal FpE(ulong u0 = 0, ulong u1 = 0, ulong u2 = 0, ulong u3 = 0)
    {
        this.u0 = u0;
        this.u1 = u1;
        this.u2 = u2;
        this.u3 = u3;
    }

    public static FpE SetElement(ulong u0 = 0, ulong u1 = 0, ulong u2 = 0, ulong u3 = 0)
    {
        FpE newElem = new FpE(u0, u1, u2, u3);
        ToMont(in newElem, out FpE res);
        return res;
    }

    public static FpE SetElement(BigInteger value)
    {
        FpE newElem = new FpE(value);
        ToMont(in newElem, out FpE res);
        return res;
    }

    public FpE(in ReadOnlySpan<byte> bytes, bool isBigEndian = false)
    {
        UInt256 val = new UInt256(bytes, isBigEndian);
        val.Mod(_modulus.Value, out UInt256 res);
        FpE inp = new FpE(res.u0, res.u1, res.u2, res.u3);
        ToMont(inp, out this);
    }

    public FpE Dup()
    {
        return new FpE(u0, u1, u2, u3);
    }

    internal FpE(BigInteger value)
    {
        UInt256 res;
        if (value.Sign < 0)
        {
            ElementUtils.SubtractMod(UInt256.Zero,(UInt256)(-value), _modulus.Value, out res);
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

    public static IEnumerable<FpE> SetRandom()
    {
        byte[] data = new byte[32];
        Random rand = new(0);
        rand.NextBytes(data);
        yield return new FpE(data);
    }

    public FpE Neg()
    {
        SubMod(FpE.Zero, this, out FpE res);
        return res;
    }

    public bool LexicographicallyLargest()
    {
        FromMont(in this, out FpE mont);
        return !SubtractUnderflow(mont, qMinOne, out FpE _);
    }

    public Span<byte> ToBytes()
    {
        ToRegular(in this, out FpE x);
        return ElementUtils.ToLittleEndian(x.u0, x.u1, x.u2, x.u3);
    }

    public Span<byte> ToBytesBigEndian()
    {
        ToRegular(in this, out FpE x);
        return ElementUtils.ToBigEndian(x.u0, x.u1, x.u2, x.u3);
    }

    public static FpE? FromBytes(byte[] byteEncoded, bool isBigEndian=false)
    {
        UInt256 val = new UInt256(byteEncoded, isBigEndian);
        if (val > _modulus.Value) return null;
        FpE inp = new FpE(val.u0, val.u1, val.u2, val.u3);
        ToMont(inp, out FpE resF);
        return resF;
    }

    public static FpE FromBytesReduced(byte[] byteEncoded, bool isBigEndian=false)
    {
        UInt256 val = new UInt256(byteEncoded, isBigEndian);
        val.Mod(_modulus.Value, out UInt256 res);
        FpE inp = new FpE(res.u0, res.u1, res.u2, res.u3);
        ToMont(inp, out FpE resF);
        return resF;
    }

    public bool IsZero => (u0 | u1 | u2 | u3) == 0;

    public bool IsOne => Equals(One);

    public static bool Sqrt(in FpE x, out FpE z)
    {
        Exp(in x, _bSqrtExponentElement.Value, out var w);
        MulMod(x, w, out var y);
        MulMod(w, y, out var b);

        ulong r = SqrtR;
        FpE t = b;

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

        FpE g = gResidue;
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

    public static int Legendre(in FpE z)
    {
        Exp(z, _bLegendreExponentElement.Value, out FpE res);
        if (res.IsZero) return 0;

        if (res.IsOne) return 1;
        return -1;
    }

    public static void Exp(in FpE b, in UInt256 e, out FpE result)
    {
        result = One;
        FpE bs = b;
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

    public static FpE[] MultiInverse(FpE[] values)
    {
        if (values.Length == 0) return Array.Empty<FpE>();

        FpE[] results = new FpE[values.Length];
        bool[] zeros = new bool[values.Length];

        FpE accumulator = One;

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
            if(zeros[i]) continue;
            MulMod(in results[i], in accumulator, out results[i]);
            MulMod(in accumulator, in values[i], out accumulator);
        }

        return values;
    }

    public static void ToMont(in FpE x, out FpE z)
    {
        MulMod(x, rSquare, out z);
    }

    public static void ToRegular(in FpE x, out FpE z)
    {
        FromMont(in x, out z);
    }

    public static void FromMont(in FpE x, out FpE res)
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


    public static void Inverse(in FpE x, out FpE z)
    {
        if (x.IsZero)
        {
            z = Zero;
            return;
        }

        // initialize u = q
        FpE u = qElement;
        // initialize s = r^2
        FpE s = rSquare;
        FpE r = new FpE();
        FpE v = x;


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


    public static void MulMod(in FpE x, in FpE y, out FpE res)
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

    public static void AddMod(in FpE a, in FpE b, out FpE res)
    {
        Add(a, b, out FpE z);
        if (LessThan(qElement, z))
            res = z - qElement;
        else
            res = z;

    }

    public static void SubMod(in FpE a, in FpE b, out FpE res)
    {
        if (SubtractUnderflow(a, b, out res)) res += qElement;
    }

    public static void Divide(in FpE x, in FpE y, out FpE z)
    {
        Inverse(y, out FpE yInv);
        MulMod(x, yInv, out z);
    }

    public static void Lsh(in FpE x, int n, out FpE res)
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

        res = new FpE(z0, z1, z2, z3);
    }

    public void LeftShift(int n, out FpE res)
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


    public static void Rsh(in FpE x, int n, out FpE res)
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

        res = new FpE(z0, z1, z2, z3);
    }

    public void RightShift(int n, out FpE res) => Rsh(this, n, out res);



    internal void Lsh64(out FpE res)
    {
        res = new FpE(0, u0, u1, u2);
    }

    internal void Lsh128(out FpE res)
    {
        res = new FpE(0, 0, u0, u1);
    }

    internal void Lsh192(out FpE res)
    {
        res = new FpE(0, 0, 0, u0);
    }

    internal void Rsh64(out FpE res)
    {
        res = new FpE(u1, u2, u3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Rsh128(out FpE res)
    {
        res = new FpE(u2, u3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Rsh192(out FpE res)
    {
        res = new FpE(u3);
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
    public static void Add(in FpE a, in FpE b, out FpE res)
    {
        ulong carry = 0ul;
        AddWithCarry(a.u0, b.u0, ref carry, out ulong res1);
        AddWithCarry(a.u1, b.u1, ref carry, out ulong res2);
        AddWithCarry(a.u2, b.u2, ref carry, out ulong res3);
        AddWithCarry(a.u3, b.u3, ref carry, out ulong res4);
        res = new FpE(res1, res2, res3, res4);
    }
    public static bool SubtractUnderflow(in FpE a, in FpE b, out FpE res)
    {
        ulong borrow = 0;
        SubtractWithBorrow(a[0], b[0], ref borrow, out ulong z0);
        SubtractWithBorrow(a[1], b[1], ref borrow, out ulong z1);
        SubtractWithBorrow(a[2], b[2], ref borrow, out ulong z2);
        SubtractWithBorrow(a[3], b[3], ref borrow, out ulong z3);
        res = new FpE(z0, z1, z2, z3);
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

    public static implicit operator FpE(ulong value) => new FpE(value, 0ul, 0ul, 0ul);
    public static implicit operator FpE(ulong[] value) => new FpE(value[0], value[1], value[2], value[3]);

    public static explicit operator FpE(in BigInteger value)
    {
        byte[] bytes32 = value.ToBytes32(true);
        return new FpE(bytes32, true);
    }

    public static FpE operator +(in FpE a, in FpE b)
    {
        AddMod(in a, in b, out FpE res);
        return res;
    }

    public static FpE operator -(in FpE a, in FpE b)
    {
        SubMod(in a, in b, out FpE c);
        return c;
    }

    public static FpE operator >>(in FpE a, int n)
    {
        a.RightShift(n, out FpE res);
        return res;
    }
    public static FpE operator <<(in FpE a, int n)
    {
        a.LeftShift(n, out FpE res);
        return res;
    }

    public bool Equals(FpE other) => u0 == other.u0 && u1 == other.u1 && u2 == other.u2 && u3 == other.u3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Equals(in FpE other) =>
        u0 == other.u0 &&
        u1 == other.u1 &&
        u2 == other.u2 &&
        u3 == other.u3;

    public int CompareTo(FpE b) => this < b ? -1 : Equals(b) ? 0 : 1;

    public override int GetHashCode() => HashCode.Combine(u0, u1, u2, u3);


    public static FpE operator /(in FpE a, in FpE b)
    {
        Divide(in a, in b, out FpE c);
        return c;
    }

    public static bool operator <(in FpE a, in FpE b) => LessThan(in a, in b);
    public static bool operator <(in FpE a, int b) => LessThan(in a, b);
    public static bool operator <(int a, in FpE b) => LessThan(a, in b);
    public static bool operator <(in FpE a, uint b) => LessThan(in a, b);
    public static bool operator <(uint a, in FpE b) => LessThan(a, in b);
    public static bool operator <(in FpE a, long b) => LessThan(in a, b);
    public static bool operator <(long a, in FpE b) => LessThan(a, in b);
    public static bool operator <(in FpE a, ulong b) => LessThan(in a, b);
    public static bool operator <(ulong a, in FpE b) => LessThan(a, in b);
    public static bool operator <=(in FpE a, in FpE b) => !LessThan(in b, in a);
    public static bool operator <=(in FpE a, int b) => !LessThan(b, in a);
    public static bool operator <=(int a, in FpE b) => !LessThan(in b, a);
    public static bool operator <=(in FpE a, uint b) => !LessThan(b, in a);
    public static bool operator <=(uint a, in FpE b) => !LessThan(in b, a);
    public static bool operator <=(in FpE a, long b) => !LessThan(b, in a);
    public static bool operator <=(long a, in FpE b) => !LessThan(in b, a);
    public static bool operator <=(in FpE a, ulong b) => !LessThan(b, in a);
    public static bool operator <=(ulong a, FpE b) => !LessThan(in b, a);
    public static bool operator >(in FpE a, in FpE b) => LessThan(in b, in a);
    public static bool operator >(in FpE a, int b) => LessThan(b, in a);
    public static bool operator >(int a, in FpE b) => LessThan(in b, a);
    public static bool operator >(in FpE a, uint b) => LessThan(b, in a);
    public static bool operator >(uint a, in FpE b) => LessThan(in b, a);
    public static bool operator >(in FpE a, long b) => LessThan(b, in a);
    public static bool operator >(long a, in FpE b) => LessThan(in b, a);
    public static bool operator >(in FpE a, ulong b) => LessThan(b, in a);
    public static bool operator >(ulong a, in FpE b) => LessThan(in b, a);
    public static bool operator >=(in FpE a, in FpE b) => !LessThan(in a, in b);
    public static bool operator >=(in FpE a, int b) => !LessThan(in a, b);
    public static bool operator >=(int a, in FpE b) => !LessThan(a, in b);
    public static bool operator >=(in FpE a, uint b) => !LessThan(in a, b);
    public static bool operator >=(uint a, in FpE b) => !LessThan(a, in b);
    public static bool operator >=(in FpE a, long b) => !LessThan(in a, b);
    public static bool operator >=(long a, in FpE b) => !LessThan(a, in b);
    public static bool operator >=(in FpE a, ulong b) => !LessThan(in a, b);
    public static bool operator >=(ulong a, in FpE b) => !LessThan(a, in b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool LessThan(in FpE a, long b) => b >= 0 && a.u3 == 0 && a.u2 == 0 && a.u1 == 0 && a.u0 < (ulong)b;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool LessThan(long a, in FpE b) => a < 0 || b.u1 != 0 || b.u2 != 0 || b.u3 != 0 || (ulong)a < b.u0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool LessThan(in FpE a, ulong b) => a.u3 == 0 && a.u2 == 0 && a.u1 == 0 && a.u0 < b;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool LessThan(ulong a, in FpE b) => b.u3 != 0 || b.u2 != 0 || b.u1 != 0 || a < b.u0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool LessThan(in FpE a, in FpE b)
    {
        if (a.u3 != b.u3)
            return a.u3 < b.u3;
        if (a.u2 != b.u2)
            return a.u2 < b.u2;
        if (a.u1 != b.u1)
            return a.u1 < b.u1;
        return a.u0 < b.u0;
    }

    public static bool LessThanSubModulus(FpE x)
    {
        return LessThan(x, qElement);
    }

    public static bool operator ==(in FpE a, int b) => a.Equals(b);
    public static bool operator ==(int a, in FpE b) => b.Equals(a);
    public static bool operator ==(in FpE a, uint b) => a.Equals(b);
    public static bool operator ==(uint a, in FpE b) => b.Equals(a);
    public static bool operator ==(in FpE a, long b) => a.Equals(b);
    public static bool operator ==(long a, in FpE b) => b.Equals(a);
    public static bool operator ==(in FpE a, ulong b) => a.Equals(b);
    public static bool operator ==(ulong a, in FpE b) => b.Equals(a);
    public static bool operator !=(in FpE a, int b) => !a.Equals(b);
    public static bool operator !=(int a, in FpE b) => !b.Equals(a);
    public static bool operator !=(in FpE a, uint b) => !a.Equals(b);
    public static bool operator !=(uint a, in FpE b) => !b.Equals(a);
    public static bool operator !=(in FpE a, long b) => !a.Equals(b);
    public static bool operator !=(long a, in FpE b) => !b.Equals(a);
    public static bool operator !=(in FpE a, ulong b) => !a.Equals(b);
    public static bool operator !=(ulong a, in FpE b) => !b.Equals(a);

    public bool Equals(int other) => other >= 0 && u0 == (uint)other && u1 == 0 && u2 == 0 && u3 == 0;

    public bool Equals(uint other) => u0 == other && u1 == 0 && u2 == 0 && u3 == 0;

    public bool Equals(long other) => other >= 0 && u0 == (ulong)other && u1 == 0 && u2 == 0 && u3 == 0;

    public bool Equals(ulong other) => u0 == other && u1 == 0 && u2 == 0 && u3 == 0;


    public static FpE operator *(in FpE a, in FpE b)
    {
        MulMod(a, b, out FpE x);
        return x;
    }
}
