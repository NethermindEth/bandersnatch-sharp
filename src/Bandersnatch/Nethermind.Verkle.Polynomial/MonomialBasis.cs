using Nethermind.Field.Montgomery.FrEElement;

namespace Nethermind.Verkle.Polynomial
{
    public class MonomialBasis : IEqualityComparer<MonomialBasis>
    {
        public readonly FrE[] Coeffs;

        public MonomialBasis(FrE[] coeffs)
        {
            Coeffs = coeffs;
        }

        public bool Equals(MonomialBasis x, MonomialBasis y)
        {
            return x!.Coeffs.SequenceEqual(y!.Coeffs);
        }

        public int GetHashCode(MonomialBasis obj)
        {
            return obj.Coeffs.GetHashCode();
        }

        public static MonomialBasis Empty()
        {
            return new MonomialBasis(new FrE[]
            {
            });
        }

        private static MonomialBasis Mul(MonomialBasis a, MonomialBasis b)
        {
            FrE[] output = new FrE[a.Length() + b.Length() - 1];
            for (int i = 0; i < a.Length(); i++)
            {
                for (int j = 0; j < b.Length(); j++)
                {
                    output[i + j] += a.Coeffs[i]! * b.Coeffs[j]!;
                }
            }
            return new MonomialBasis(output);
        }

        public static MonomialBasis Div(MonomialBasis a, MonomialBasis b)
        {
            if (a.Length() < b.Length())
            {
                throw new Exception();
            }

            FrE[] x = a.Coeffs.ToArray();
            List<FrE> output = new List<FrE>();

            int aPos = a.Length() - 1;
            int bPos = b.Length() - 1;

            int diff = aPos - bPos;
            while (diff >= 0)
            {
                FrE quot = x[aPos]! / b.Coeffs[bPos]!;
                output.Insert(0, quot!);
                for (int i = bPos; i > -1; i--)
                {
                    x[diff + i] -= b.Coeffs[i] * quot;
                }

                aPos -= 1;
                diff -= 1;
            }

            return new MonomialBasis(output.ToArray());
        }

        public FrE Evaluate(FrE x)
        {
            FrE y = FrE.Zero;
            FrE powerOfX = FrE.One;
            foreach (FrE pCoeff in Coeffs)
            {
                y += powerOfX * pCoeff!;
                powerOfX *= x;
            }

            return y;
        }

        public static MonomialBasis FormalDerivative(MonomialBasis f)
        {
            FrE[] derivative = new FrE[f.Length() - 1];
            for (int i = 1; i < f.Length(); i++)
            {
                FrE x = FrE.SetElement(i) * f.Coeffs[i]!;
                derivative[i - 1] = x;
            }
            return new MonomialBasis(derivative.ToArray());
        }

        public static MonomialBasis VanishingPoly(IEnumerable<FrE> xs)
        {
            List<FrE> root = new List<FrE>
            {
                FrE.One
            };
            foreach (FrE x in xs)
            {
                root.Insert(0, FrE.Zero);
                for (int i = 0; i < root.Count - 1; i++)
                {
                    root[i] -= root[i + 1] * x;
                }
            }

            return new MonomialBasis(root.ToArray());
        }

        public int Length()
        {
            return Coeffs.Length;
        }

        public static MonomialBasis operator /(in MonomialBasis a, in MonomialBasis b)
        {
            return Div(a, b);
        }

        public static MonomialBasis operator *(in MonomialBasis a, in MonomialBasis b)
        {
            return Mul(a, b);
        }

        public static bool operator ==(in MonomialBasis a, in MonomialBasis b)
        {
            return a.Coeffs == b.Coeffs;
        }

        public static bool operator !=(in MonomialBasis a, in MonomialBasis b)
        {
            return !(a == b);
        }

        private bool Equals(MonomialBasis other)
        {
            return Coeffs.Equals(other.Coeffs);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((MonomialBasis)obj);
        }

        public override int GetHashCode()
        {
            return Coeffs.GetHashCode();
        }
    }
}
