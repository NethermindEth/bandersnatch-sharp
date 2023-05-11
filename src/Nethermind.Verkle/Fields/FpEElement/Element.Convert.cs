using System.Numerics;
using System.Runtime.CompilerServices;
using Nethermind.Int256;
using FE = Nethermind.Verkle.Fields.FpEElement.FpE;

namespace Nethermind.Verkle.Fields.FpEElement
{
    public readonly partial struct FpE
    {
        public new string ToString()
        {
            return $"[{u0} {u1} {u2} {u3}]";
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
                ? 192 + Len64(u3)
                : u2 != 0
                    ? 128 + Len64(u2)
                    : u1 != 0
                        ? 64 + Len64(u1)
                        : Len64(u0);
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
            return ToLittleEndian(x.u0, x.u1, x.u2, x.u3);
        }

        public Span<byte> ToBytesBigEndian()
        {
            ToRegular(in this, out FE x);
            return ToBigEndian(x.u0, x.u1, x.u2, x.u3);
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

        private static void FromMontgomery(in FE x, out FE res)
        {
            U4 z = new() { u0 = x.u0, u1 = x.u1, u2 = x.u2, u3 = x.u3 };

            ulong m = z.u0 * QInvNeg;
            ulong c = MAdd0(m, Q0, z.u0);
            c = MAdd2(m, Q1, z.u1, c, out z.u0);
            c = MAdd2(m, Q2, z.u2, c, out z.u1);
            c = MAdd2(m, Q3, z.u3, c, out z.u2);
            z.u3 = c;

            m = z.u0 * QInvNeg;
            c = MAdd0(m, Q0, z.u0);
            c = MAdd2(m, Q1, z.u1, c, out z.u0);
            c = MAdd2(m, Q2, z.u2, c, out z.u1);
            c = MAdd2(m, Q3, z.u3, c, out z.u2);
            z.u3 = c;

            m = z.u0 * QInvNeg;
            c = MAdd0(m, Q0, z.u0);
            c = MAdd2(m, Q1, z.u1, c, out z.u0);
            c = MAdd2(m, Q2, z.u2, c, out z.u1);
            c = MAdd2(m, Q3, z.u3, c, out z.u2);
            z.u3 = c;

            m = z.u0 * QInvNeg;
            c = MAdd0(m, Q0, z.u0);
            c = MAdd2(m, Q1, z.u1, c, out z.u0);
            c = MAdd2(m, Q2, z.u2, c, out z.u1);
            c = MAdd2(m, Q3, z.u3, c, out z.u2);
            z.u3 = c;
            Unsafe.SkipInit(out res);
            Unsafe.As<FE, U4>(ref res) = z;
            if (LessThan(qElement, res))
            {
                SubtractUnderflow(res, qElement, out res);
            }
        }
    }
}
