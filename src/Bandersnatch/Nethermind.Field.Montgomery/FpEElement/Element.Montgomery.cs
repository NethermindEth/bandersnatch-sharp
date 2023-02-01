using System.Runtime.CompilerServices;
using FE = Nethermind.Field.Montgomery.FpEElement.FpE;

namespace Nethermind.Field.Montgomery.FpEElement
{
    public readonly partial struct FpE
    {

        public static int Legendre(in FE z)
        {
            Exp(z, _bLegendreExponentElement.Value, out FE res);

            if (res.IsZero) return 0;

            if (res.IsOne) return 1;

            return -1;
        }

        public bool LexicographicallyLargest()
        {
            FromMontgomery(in this, out FE mont);
            return !SubtractUnderflow(mont, qMinOne, out _);
        }

        public static void Inverse(in FE x, out FE z)
        {
            if (x.IsZero)
            {
                z = Zero;
                return;
            }

            // initialize u = q
            FE u = qElement;
            // initialize s = r^2
            FE s = rSquare;
            FE r = new FE(0);
            FE v = x;


            while (true)
            {
                while ((v[0] & 1) == 0)
                {
                    v.RightShiftByOne(out v);
                    if ((s[0] & 1) == 1)
                    {
                        AddOverflow(s, qElement, out s);
                    }
                    s.RightShiftByOne(out s);
                }

                while ((u[0] & 1) == 0)
                {
                    u.RightShiftByOne(out u);
                    if ((r[0] & 1) == 1)
                    {
                        AddOverflow(r, qElement, out r);
                    }
                    r.RightShiftByOne(out r);
                }

                if (!LessThan(v, u))
                {
                    SubtractUnderflow(v, u, out v);
                    SubtractMod(s, r, out s);
                }
                else
                {
                    SubtractUnderflow(u, v, out u);
                    SubtractMod(r, s, out r);
                }


                if (u[0] == 1 && (u[3] | u[2] | u[1]) == 0)
                {
                    z = r;
                    return;
                }
                if (v[0] == 1 && (v[3] | v[2] | v[1]) == 0)
                {
                    z = s;
                    return;
                }
            }
        }

        public static void MultiplyMod(in FE x, in FE y, out FE res)
        {
            ref ulong rx = ref Unsafe.As<FE, ulong>(ref Unsafe.AsRef(in x));
            ref ulong ry = ref Unsafe.As<FE, ulong>(ref Unsafe.AsRef(in y));

            ulong[] t = new ulong[4];
            ulong[] c = new ulong[3];
            ulong[] z = new ulong[4];

            {
                // round 0
                c[1] = Math.BigMul(rx, ry, out c[0]);
                ulong m = c[0] * QInvNeg;
                c[2] = MAdd0(m, Q0, c[0]);
                c[1] = MAdd1(rx, Unsafe.Add(ref ry, 1), c[1], out c[0]);
                c[2] = MAdd2(m, Q1, c[2], c[0], out t[0]);
                c[1] = MAdd1(rx, Unsafe.Add(ref ry, 2), c[1], out c[0]);
                c[2] = MAdd2(m, Q2, c[2], c[0], out t[1]);
                c[1] = MAdd1(rx, Unsafe.Add(ref ry, 3), c[1], out c[0]);
                t[3] = MAdd3(m, Q3, c[0], c[2], c[1], out t[2]);
            }
            {
                // round 1
                c[1] = MAdd1(Unsafe.Add(ref rx, 1), ry, t[0], out c[0]);
                ulong m = c[0] * QInvNeg;
                c[2] = MAdd0(m, Q0, c[0]);
                c[1] = MAdd2(Unsafe.Add(ref rx, 1), Unsafe.Add(ref ry, 1), c[1], t[1], out c[0]);
                c[2] = MAdd2(m, Q1, c[2], c[0], out t[0]);
                c[1] = MAdd2(Unsafe.Add(ref rx, 1), Unsafe.Add(ref ry, 2), c[1], t[2], out c[0]);
                c[2] = MAdd2(m, Q2, c[2], c[0], out t[1]);
                c[1] = MAdd2(Unsafe.Add(ref rx, 1), Unsafe.Add(ref ry, 3), c[1], t[3], out c[0]);
                t[3] = MAdd3(m, Q3, c[0], c[2], c[1], out t[2]);
            }
            {
                // round 2
                c[1] = MAdd1(Unsafe.Add(ref rx, 2), ry, t[0], out c[0]);
                ulong m = c[0] * QInvNeg;
                c[2] = MAdd0(m, Q0, c[0]);
                c[1] = MAdd2(Unsafe.Add(ref rx, 2), Unsafe.Add(ref ry, 1), c[1], t[1], out c[0]);
                c[2] = MAdd2(m, Q1, c[2], c[0], out t[0]);
                c[1] = MAdd2(Unsafe.Add(ref rx, 2), Unsafe.Add(ref ry, 2), c[1], t[2], out c[0]);
                c[2] = MAdd2(m, Q2, c[2], c[0], out t[1]);
                c[1] = MAdd2(Unsafe.Add(ref rx, 2), Unsafe.Add(ref ry, 3), c[1], t[3], out c[0]);
                t[3] = MAdd3(m, Q3, c[0], c[2], c[1], out t[2]);
            }
            {
                // round 3
                c[1] = MAdd1(Unsafe.Add(ref rx, 3), ry, t[0], out c[0]);
                ulong m = c[0] * QInvNeg;
                c[2] = MAdd0(m, Q0, c[0]);
                c[1] = MAdd2(Unsafe.Add(ref rx, 3), Unsafe.Add(ref ry, 1), c[1], t[1], out c[0]);
                c[2] = MAdd2(m, Q1, c[2], c[0], out z[0]);
                c[1] = MAdd2(Unsafe.Add(ref rx, 3), Unsafe.Add(ref ry, 2), c[1], t[2], out c[0]);
                c[2] = MAdd2(m, Q2, c[2], c[0], out z[1]);
                c[1] = MAdd2(Unsafe.Add(ref rx, 3), Unsafe.Add(ref ry, 3), c[1], t[3], out c[0]);
                z[3] = MAdd3(m, Q3, c[0], c[2], c[1], out z[2]);
            }
            res = z;
            if (LessThan(qElement, res))
            {
                SubtractUnderflow(res, qElement, out res);
            }
        }

        public static FE[] MultiInverse(FE[] values)
        {
            if (values.Length == 0) return Array.Empty<FE>();

            FE[] results = new FE[values.Length];
            bool[] zeros = new bool[values.Length];

            FE accumulator = One;

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].IsZero)
                {
                    zeros[i] = true;
                    continue;
                }
                results[i] = accumulator;
                MultiplyMod(in accumulator, in values[i], out accumulator);
            }

            Inverse(in accumulator, out accumulator);

            for (int i = values.Length - 1; i >= 0; i--)
            {
                if (zeros[i]) continue;
                MultiplyMod(in results[i], in accumulator, out results[i]);
                MultiplyMod(in accumulator, in values[i], out accumulator);
            }

            return results;
        }

        public static bool Sqrt(in FE x, out FE z)
        {
            Exp(in x, _bSqrtExponentElement.Value, out FE w);
            MultiplyMod(x, w, out FE y);
            MultiplyMod(w, y, out FE b);

            ulong r = SqrtR;
            FE t = b;

            for (ulong i = 0; i < r - 1; i++)
            {
                MultiplyMod(in t, in t, out t);
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

            FE g = gResidue;
            while (true)
            {
                ulong m = 0;
                t = b;

                while (!t.IsOne)
                {
                    MultiplyMod(in t, in t, out t);
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
                    MultiplyMod(in t, in t, out t);
                    ge--;
                }

                MultiplyMod(in t, in t, out g);
                MultiplyMod(in y, in t, out y);
                MultiplyMod(in b, in g, out b);
                r = m;
            }
        }
    }
}
