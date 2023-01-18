using System.Data;
using System.Runtime.CompilerServices;
using Nethermind.Int256;
using FE = Nethermind.Field.Montgomery.FpEElement.FpE;

namespace Nethermind.Field.Montgomery.FpEElement
{
    public readonly partial struct FpE
    {
        public static IEnumerable<FE> GetRandom()
        {
            byte[] data = new byte[32];
            Random rand = new Random(0);
            rand.NextBytes(data);
            yield return new FE(data);
        }

        public FE Negative()
        {
            SubtractMod(Zero, this, out FE res);
            return res;
        }

        public void LeftShift(int n, out FE res)
        {
            Lsh(this, n, out res);
        }
        public void RightShift(int n, out FE res)
        {
            Rsh(this, n, out res);
        }


        public static void AddMod(in FE a, in FE b, out FE res)
        {
            bool overflow = ElementUtils.AddOverflow(a.u0, a.u1, a.u2, a.u3, b.u0, b.u1, b.u2, b.u3, out ulong u0, out ulong u1, out ulong u2, out ulong u3);
            if (overflow)
            {
                ElementUtils.SubtractUnderflow(u0, u1, u2, u3, Q0, Q1, Q2, Q3, out u0, out u1, out u2, out u3);
                res = new FE(u0, u1, u2, u3);
                return;
            }

            if (!LessThanSubModulus(u0, u1, u2, u3))
            {
                if (ElementUtils.SubtractUnderflow(u0, u1, u2, u3, Q0, Q1, Q2, Q3, out u0, out u1, out u2, out u3))
                {
                    throw new InvalidConstraintException("this should now be possible");
                }
            }
            res = new FE(u0, u1, u2, u3);
        }
        public static void Divide(in FE x, in FE y, out FE z)
        {
            Inverse(y, out FE yInv);
            MultiplyMod(x, yInv, out z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractMod(in FE a, in FE b, out FE res)
        {
            ulong u0;
            ulong u1;
            ulong u2;
            ulong u3;
            if (ElementUtils.SubtractUnderflow(a.u0, a.u1, a.u2, a.u3, b.u0, b.u1, b.u2, b.u3, out u0, out u1, out u2, out u3))
                ElementUtils.AddOverflow(Q0, Q1, Q2, Q3, u0, u1, u2, u3, out u0, out u1, out u2, out u3);

            res = new FE(u0, u1, u2, u3);
        }

        public static void Exp(in FE b, in UInt256 e, out FE result)
        {
            result = One;
            FE bs = b;
            int len = e.BitLen;
            for (int i = 0; i < len; i++)
            {
                if (e.Bit(i))
                {
                    MultiplyMod(result, bs, out result);
                }
                MultiplyMod(bs, bs, out bs);
            }
        }

        public static void Lsh(in FE x, int n, out FE res)
        {
            if (n % 64 == 0)
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
            if (n > 128)
            {
                x.Lsh128(out res);
                n -= 128;
                goto sh128;
            }
            if (n > 64)
            {
                x.Lsh64(out res);
                n -= 64;
                goto sh64;
            }
            res = x;

            // remaining shifts
            a = ElementUtils.Rsh(res.u0, 64 - n);
            z0 = ElementUtils.Lsh(res.u0, n);

        sh64:
            b = ElementUtils.Rsh(res.u1, 64 - n);
            z1 = ElementUtils.Lsh(res.u1, n) | a;

        sh128:
            a = ElementUtils.Rsh(res.u2, 64 - n);
            z2 = ElementUtils.Lsh(res.u2, n) | b;

        sh192:
            z3 = ElementUtils.Lsh(res.u3, n) | a;

            res = new FE(z0, z1, z2, z3);
        }


        public static void Rsh(in FE x, int n, out FE res)
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
            if (n > 128)
            {
                x.Rsh128(out res);
                z0 = res.u0;
                z1 = res.u1;
                z2 = res.u2;
                z3 = res.u3;
                n -= 128;
                goto sh128;
            }
            if (n > 64)
            {
                x.Rsh64(out res);
                z0 = res.u0;
                z1 = res.u1;
                z2 = res.u2;
                z3 = res.u3;
                n -= 64;
                goto sh64;
            }
            res = x;
            z0 = res.u0;
            z1 = res.u1;
            z2 = res.u2;
            z3 = res.u3;

            // remaining shifts
            a = ElementUtils.Lsh(res.u3, 64 - n);
            z3 = ElementUtils.Rsh(res.u3, n);

        sh64:
            b = ElementUtils.Lsh(res.u2, 64 - n);
            z2 = ElementUtils.Rsh(res.u2, n) | a;

        sh128:
            a = ElementUtils.Lsh(res.u1, 64 - n);
            z1 = ElementUtils.Rsh(res.u1, n) | b;

        sh192:
            z0 = ElementUtils.Rsh(res.u0, n) | a;

            res = new FE(z0, z1, z2, z3);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Lsh64(out FE res)
        {
            res = new FE(0, u0, u1, u2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Lsh128(out FE res)
        {
            res = new FE(0, 0, u0, u1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Lsh192(out FE res)
        {
            res = new FE(0, 0, 0, u0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Rsh64(out FE res)
        {
            res = new FE(u1, u2, u3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Rsh128(out FE res)
        {
            res = new FE(u2, u3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Rsh192(out FE res)
        {
            res = new FE(u3);
        }
    }
}
