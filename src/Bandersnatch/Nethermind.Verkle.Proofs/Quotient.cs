using Nethermind.Field;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Polynomial;

namespace Nethermind.Verkle.Proofs;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public static class Quotient
{
    public static Fr[] ComputeQuotientInsideDomain(PreComputeWeights precomp, LagrangeBasis f,
        Fr index)
    {
        int domainSize = precomp.Domain.Length;
        Fr?[]? inverses = precomp.DomainInv;
        Fr?[]? aPrimeDomain = precomp.APrimeDomain;
        Fr?[]? aPrimeDomainInv = precomp.APrimeDomainInv;

        int indexI = index.ToInt();

        Fr[] q = new Fr[domainSize];
        for (int i = 0; i < domainSize; i++)
        {
            q[i] = Fr.Zero;
        }
        Fr? y = f.Evaluations[indexI];

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

    public static Fr[] ComputeQuotientOutsideDomain(PreComputeWeights precom, LagrangeBasis f, Fr z,
        Fr y)
    {
        Fr?[]? domain = precom.Domain;
        int domainSize = domain.Length;

        Fr[]? q = new Fr[domainSize];
        for (int i = 0; i < domainSize; i++)
        {
            q[i] = (f.Evaluations[i] - y) / (domain[i] - z);
        }

        return q;
    }
}
