using System.Numerics;
using System.Runtime.CompilerServices;
using Nethermind.Int256;
using FE = Nethermind.Field.Montgomery.ElementFactory.Element;

namespace Nethermind.Field.Montgomery.ElementFactory
{
    public readonly partial struct Element
    {
        public FE Dup()
        {
            return new FE(u0, u1, u2, u3);
        }

        public bool Bit(int n)
        {
            int bucket = n / 64 % 4;
            int position = n % 64;
            return (this[bucket] & (ulong)1 << position) != 0;
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

        public static FE SetElement(ulong u0 = 0, ulong u1 = 0, ulong u2 = 0, ulong u3 = 0)
        {
            FE newElem = new FE(u0, u1, u2, u3);
            ToMontgomery(in newElem, out FE res);
            return res;
        }

        public static FE SetElement(BigInteger value)
        {
            FE newElem = new FE(value);
            ToMontgomery(in newElem, out FE res);
            return res;
        }

        public Span<byte> ToBytes()
        {
            ToRegular(in this, out FE x);
            return ElementUtils.ToLittleEndian(x.u0, x.u1, x.u2, x.u3);
        }

        public Span<byte> ToBytesBigEndian()
        {
            ToRegular(in this, out FE x);
            return ElementUtils.ToBigEndian(x.u0, x.u1, x.u2, x.u3);
        }

        public static FE FromBytes(byte[] byteEncoded, bool isBigEndian = false)
        {
            UInt256 val = new UInt256(byteEncoded, isBigEndian);
            if (val > _modulus.Value) throw new ArgumentException("FromBytes: byteEncoded should be less than modulus - use FromBytesReduced instead.");
            FE inp = new FE(val.u0, val.u1, val.u2, val.u3);
            ToMontgomery(inp, out FE resF);
            return resF;
        }

        public static FE FromBytesReduced(byte[] byteEncoded, bool isBigEndian = false)
        {
            UInt256 val = new UInt256(byteEncoded, isBigEndian);
            val.Mod(_modulus.Value, out UInt256 res);
            FE inp = new FE(res.u0, res.u1, res.u2, res.u3);
            ToMontgomery(inp, out FE resF);
            return resF;
        }
        public static void ToMontgomery(in FE x, out FE z)
        {
            MultiplyMod(x, rSquare, out z);
        }

        public static void ToRegular(in FE x, out FE z)
        {
            FromMontgomery(in x, out z);
        }

        public static void FromMontgomery(in FE x, out FE res)
        {
            ulong[] z = new ulong[4];
            z[0] = x[0];
            z[1] = x[1];
            z[2] = x[2];
            z[3] = x[3];

            ulong m = z[0] * QInvNeg;
            ulong c = ElementUtils.MAdd0(m, Q0, z[0]);
            c = ElementUtils.MAdd2(m, Q1, z[1], c, out z[0]);
            c = ElementUtils.MAdd2(m, Q2, z[2], c, out z[1]);
            c = ElementUtils.MAdd2(m, Q3, z[3], c, out z[2]);
            z[3] = c;

            m = z[0] * QInvNeg;
            c = ElementUtils.MAdd0(m, Q0, z[0]);
            c = ElementUtils.MAdd2(m, Q1, z[1], c, out z[0]);
            c = ElementUtils.MAdd2(m, Q2, z[2], c, out z[1]);
            c = ElementUtils.MAdd2(m, Q3, z[3], c, out z[2]);
            z[3] = c;

            m = z[0] * QInvNeg;
            c = ElementUtils.MAdd0(m, Q0, z[0]);
            c = ElementUtils.MAdd2(m, Q1, z[1], c, out z[0]);
            c = ElementUtils.MAdd2(m, Q2, z[2], c, out z[1]);
            c = ElementUtils.MAdd2(m, Q3, z[3], c, out z[2]);
            z[3] = c;

            m = z[0] * QInvNeg;
            c = ElementUtils.MAdd0(m, Q0, z[0]);
            c = ElementUtils.MAdd2(m, Q1, z[1], c, out z[0]);
            c = ElementUtils.MAdd2(m, Q2, z[2], c, out z[1]);
            c = ElementUtils.MAdd2(m, Q3, z[3], c, out z[2]);
            z[3] = c;

            if (ElementUtils.LessThan(ref Unsafe.As<FE, ulong>(ref Unsafe.AsRef(in qElement)), ref Unsafe.As<ulong[], ulong>(ref Unsafe.AsRef(in z))))
            {
                ElementUtils.SubtractUnderflow(z[0], z[1], z[2], z[3], Q0, Q1, Q2, Q3, out z[0], out z[1], out z[2], out z[3]);
            }
            res = z;
        }
    }
}
