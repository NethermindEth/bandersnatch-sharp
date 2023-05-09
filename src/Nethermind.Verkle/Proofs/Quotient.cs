using Nethermind.Verkle.Polynomial;
using Nethermind.Verkle.Fields.FrEElement;

namespace Nethermind.Verkle.Proofs
{
    public static class Quotient
    {
        public static FrE[] ComputeQuotientInsideDomain(PreComputedWeights preComp, LagrangeBasis f,
            FrE index)
        {
            int domainSize = f.Evaluations.Length;

            FrE[] inverses = preComp.DomainInv;
            FrE[] aPrimeDomain = preComp.APrimeDomain;
            FrE[] aPrimeDomainInv = preComp.APrimeDomainInv;

            int indexI = index.ToBytes()[0];

            FrE[] q = new FrE[domainSize];
            for (int i = 0; i < domainSize; i++)
            {
                q[i] = FrE.Zero;
            }
            FrE y = f.Evaluations[indexI];


            for (int i = 0; i < domainSize; i++)
            {
                if (i == indexI) continue;
                q[i] = (f.Evaluations[i] - y) * inverses[(i - indexI) < 0 ? (inverses.Length + (i - indexI)): (i - indexI)];
                q[indexI] += (f.Evaluations[i] - y) * inverses[(indexI - i) < 0 ? (inverses.Length + indexI - i) : (indexI - i)] * aPrimeDomain[indexI] *
                             aPrimeDomainInv[i];
            }

            return q;
        }

        public static FrE[] ComputeQuotientOutsideDomain(PreComputedWeights preComp, LagrangeBasis f, FrE z,
            FrE y)
        {
            FrE[] domain = preComp.Domain;
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
