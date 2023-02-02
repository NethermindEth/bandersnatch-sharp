using Nethermind.Field.Montgomery.FrEElement;
using Nethermind.Verkle.Polynomial;

namespace Nethermind.Verkle.Proofs
{
    public static class Quotient
    {
        public static FrE[] ComputeQuotientInsideDomain(PreComputeWeights preComp, LagrangeBasis f,
            FrE index)
        {
            int domainSize = preComp._domain.Length;
            FrE[] inverses = preComp._domainInv;
            FrE[] aPrimeDomain = preComp._aPrimeDomain;
            FrE[] aPrimeDomainInv = preComp._aPrimeDomainInv;

            int indexI = (int)index.u0;

            FrE[] q = new FrE[domainSize];
            for (int i = 0; i < domainSize; i++)
            {
                q[i] = FrE.Zero;
            }
            FrE y = f.Evaluations[indexI];

            for (int i = 0; i < domainSize; i++)
            {
                if (i == indexI) continue;
                q[i] = (f.Evaluations[i] - y) * inverses[(i - indexI) < 0 ? inverses.Length + (i - indexI): (i - indexI)];
                q[indexI] += (f.Evaluations[i] - y) * inverses[indexI - i < 0 ? inverses.Length + indexI - i : indexI - i] * aPrimeDomain[indexI] *
                             aPrimeDomainInv[i];
            }

            return q;
        }

        public static FrE[] ComputeQuotientOutsideDomain(PreComputeWeights preComp, LagrangeBasis f, FrE z,
            FrE y)
        {
            FrE[] domain = preComp._domain;
            int domainSize = domain.Length;

            FrE[] q = new FrE[domainSize];
            for (int i = 0; i < domainSize; i++)
            {
                FrE x = f.Evaluations[i] - y;
                FrE zz = domain[i] - z;
                q[i] = x / zz;
            }

            return q;
        }
    }
}
