using Nethermind.Field.Montgomery.FrEElement;
using Nethermind.Polynomial;

namespace Nethermind.Ipa
{
    public static class Quotient
    {
        public static FrE[] ComputeQuotientInsideDomain(PreComputeWeights precomp, LagrangeBasis f,
            FrE index)
        {
            int domainSize = precomp.Domain.Length;
            FrE[] inverses = precomp.DomainInv;
            FrE[] aPrimeDomain = precomp.APrimeDomain;
            FrE[] aPrimeDomainInv = precomp.APrimeDomainInv;

            int indexI = (int)index.u0;

            FrE[] q = new FrE[domainSize];
            for (int i = 0; i < domainSize; i++)
            {
                q[i] = FrE.Zero;
            }
            FrE y = f.Evaluations[indexI];

            for (int i = 0; i < domainSize; i++)
            {
                if (i != indexI)
                {
                    q[i] = (f.Evaluations[i] - y) * inverses[i - indexI];
                    q[indexI] += (f.Evaluations[i] - y) * inverses[indexI - i < 0 ? inverses.Length + indexI - i : indexI - i] * aPrimeDomain[indexI] *
                                 aPrimeDomainInv[i];
                }
            }

            return q;
        }

        public static FrE[] ComputeQuotientOutsideDomain(PreComputeWeights precom, LagrangeBasis f, FrE z,
            FrE y)
        {
            FrE[] domain = precom.Domain;
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
