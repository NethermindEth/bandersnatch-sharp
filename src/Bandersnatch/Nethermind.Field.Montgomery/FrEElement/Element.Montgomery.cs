using FE=Nethermind.Field.Montgomery.FrEElement.FrE;

namespace Nethermind.Field.Montgomery.FrEElement
{
    public readonly partial struct FrE
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
            return !ElementUtils.SubtractUnderflow(mont.u0, mont.u1, mont.u2, mont.u3,
                qMinOne.u0, qMinOne.u1, qMinOne.u2, qMinOne.u3, out ulong _, out ulong _, out ulong _, out ulong _);
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
                        ElementUtils.AddOverflow(in s.u0, in s.u1, in s.u2, in s.u3, Q0, Q1, Q2, Q3, out ulong u0, out ulong u1, out ulong u2, out ulong u3);
                        s = new FE(u0, u1, u2, u3);
                    }
                    s.RightShiftByOne(out s);
                }

                while ((u[0] & 1) == 0)
                {
                    u.RightShiftByOne(out u);
                    if ((r[0] & 1) == 1)
                    {
                        ElementUtils.AddOverflow(in r.u0, in r.u1, in r.u2, in r.u3, Q0, Q1, Q2, Q3, out ulong u0, out ulong u1, out ulong u2, out ulong u3);
                        r = new FE(u0, u1, u2, u3);
                    }
                    r.RightShiftByOne(out r);
                }

                if (!LessThan(v, u))
                {
                    ElementUtils.SubtractUnderflow(in v.u0, in v.u1, in v.u2, in v.u3, in u.u0, in u.u1, in u.u2, in u.u3, out ulong u0, out ulong u1, out ulong u2, out ulong u3);
                    v = new FE(u0, u1, u2, u3);
                    SubtractMod(s, r, out s);
                }
                else
                {
                    ElementUtils.SubtractUnderflow(in u.u0, u.u1, u.u2, u.u3, in v.u0, v.u1, v.u2, v.u3, out ulong u0, out ulong u1, out ulong u2, out ulong u3);
                    u = new FE(u0, u1, u2, u3);
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
            ulong[] t = new ulong[4];
            ulong[] c = new ulong[3];
            ulong[] z = new ulong[4];

            {
                // round 0

                ulong v = x[0];
                c[1] = Math.BigMul(v, y[0], out c[0]);
                ulong m = c[0] * QInvNeg;
                c[2] = ElementUtils.MAdd0(m, Q0, c[0]);
                c[1] = ElementUtils.MAdd1(v, y[1], c[1], out c[0]);
                c[2] = ElementUtils.MAdd2(m, Q1, c[2], c[0], out t[0]);
                c[1] = ElementUtils.MAdd1(v, y[2], c[1], out c[0]);
                c[2] = ElementUtils.MAdd2(m, Q2, c[2], c[0], out t[1]);
                c[1] = ElementUtils.MAdd1(v, y[3], c[1], out c[0]);
                t[3] = ElementUtils.MAdd3(m, Q3, c[0], c[2], c[1], out t[2]);
            }
            {
                // round 1
                ulong v = x[1];
                c[1] = ElementUtils.MAdd1(v, y[0], t[0], out c[0]);
                ulong m = c[0] * QInvNeg;
                c[2] = ElementUtils.MAdd0(m, Q0, c[0]);
                c[1] = ElementUtils.MAdd2(v, y[1], c[1], t[1], out c[0]);
                c[2] = ElementUtils.MAdd2(m, Q1, c[2], c[0], out t[0]);
                c[1] = ElementUtils.MAdd2(v, y[2], c[1], t[2], out c[0]);
                c[2] = ElementUtils.MAdd2(m, Q2, c[2], c[0], out t[1]);
                c[1] = ElementUtils.MAdd2(v, y[3], c[1], t[3], out c[0]);
                t[3] = ElementUtils.MAdd3(m, Q3, c[0], c[2], c[1], out t[2]);
            }
            {
                // round 2

                ulong v = x[2];
                c[1] = ElementUtils.MAdd1(v, y[0], t[0], out c[0]);
                ulong m = c[0] * QInvNeg;
                c[2] = ElementUtils.MAdd0(m, Q0, c[0]);
                c[1] = ElementUtils.MAdd2(v, y[1], c[1], t[1], out c[0]);
                c[2] = ElementUtils.MAdd2(m, Q1, c[2], c[0], out t[0]);
                c[1] = ElementUtils.MAdd2(v, y[2], c[1], t[2], out c[0]);
                c[2] = ElementUtils.MAdd2(m, Q2, c[2], c[0], out t[1]);
                c[1] = ElementUtils.MAdd2(v, y[3], c[1], t[3], out c[0]);
                t[3] = ElementUtils.MAdd3(m, Q3, c[0], c[2], c[1], out t[2]);
            }
            {
                // round 3

                ulong v = x[3];
                c[1] = ElementUtils.MAdd1(v, y[0], t[0], out c[0]);
                ulong m = c[0] * QInvNeg;
                c[2] = ElementUtils.MAdd0(m, Q0, c[0]);
                c[1] = ElementUtils.MAdd2(v, y[1], c[1], t[1], out c[0]);
                c[2] = ElementUtils.MAdd2(m, Q1, c[2], c[0], out z[0]);
                c[1] = ElementUtils.MAdd2(v, y[2], c[1], t[2], out c[0]);
                c[2] = ElementUtils.MAdd2(m, Q2, c[2], c[0], out z[1]);
                c[1] = ElementUtils.MAdd2(v, y[3], c[1], t[3], out c[0]);
                z[3] = ElementUtils.MAdd3(m, Q3, c[0], c[2], c[1], out z[2]);
            }
            if (LessThan(qElement, z))
            {
                ElementUtils.SubtractUnderflow(z[0], z[1], z[2], z[3], Q0, Q1, Q2, Q3, out z[0], out z[1], out z[2], out z[3]);
            }
            res = z;
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
