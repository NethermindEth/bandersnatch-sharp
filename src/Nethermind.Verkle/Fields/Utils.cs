using Nethermind.Int256;

namespace Nethermind.Verkle.Fields
{
    public static class FieldMethods
    {
        public static UInt256? ModSqrt(UInt256 a, UInt256 p)
        {
            if (LegendreSymbol(a, p) != 1)
                return null;
            if (a.IsZero)
                return UInt256.Zero;
            if (a == 2)
                return UInt256.Zero;
            UInt256.Mod(a, 4, out UInt256 res);
            if (res == 4)
            {
                UInt256.Divide(a + 1, 4, out UInt256 exp);
                UInt256.ExpMod(a, exp, p, out UInt256 ls);
                return ls;
            }

            UInt256 s = p - 1;
            UInt256 e = 0;

            UInt256.Mod(s, 2, out UInt256 loopVar);
            while (loopVar.IsZero)
            {
                UInt256.Divide(s, 2, out UInt256 ss);
                s = ss;
                e += 1;
                UInt256.Mod(s, 2, out loopVar);
            }

            UInt256 n = 2;
            while (LegendreSymbol(n, p) != -1)
                n += 1;

            UInt256.Divide(s + 1, 2, out UInt256 expX);
            UInt256.ExpMod(a, expX, p, out UInt256 x);

            UInt256.ExpMod(a, s, p, out UInt256 b);
            UInt256.ExpMod(n, s, p, out UInt256 g);

            UInt256 r = e;

            while (true)
            {
                UInt256 t = b;
                UInt256 m = UInt256.Zero;

                for (; m < r; m++)
                {
                    if (t.IsOne)
                    {
                        break;
                    }

                    UInt256.ExpMod(t, 2, p, out UInt256 tt);
                    t = tt;
                }

                if (m.IsZero)
                    return x;

                UInt256.Exp(2, r - m - 1, out UInt256 expGS);
                UInt256.ExpMod(g, expGS, p, out UInt256 gs);

                UInt256.MultiplyMod(gs, gs, p, out g);
                UInt256.MultiplyMod(x, gs, p, out x);
                UInt256.MultiplyMod(b, g, p, out b);

                r = m;
            }
        }
        private static int LegendreSymbol(UInt256 a, UInt256 p)
        {
            UInt256.Divide(p - 1, 2, out UInt256 exp);
            UInt256.ExpMod(a, exp, p, out UInt256 ls);
            return ls == p - 1 ? -1 : 1;
        }
    }
}
