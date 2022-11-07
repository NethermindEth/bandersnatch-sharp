using Curve;
using Field;
using Polynomial;

namespace Proofs;
using Fp = FixedFiniteField<BandersnatchBaseFieldStruct>;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public static class Quotient
{
    public static Fr[] ComputeQuotientInsideDomain(PreComputeWeights precomp, LagrangeBasis f,
        Fr index)
    {
        var domainSize = precomp.Domain.Length;
        var inverses = precomp.DomainInv;
        var aPrimeDomain = precomp.APrimeDomain;
        var aPrimeDomainInv = precomp.APrimeDomainInv;

        var indexI = index.ToInt();
        
        Fr[] q = new Fr[domainSize];
        for (int i = 0; i < domainSize; i++)
        {
            q[i] = Fr.Zero;
        }
        var y = f.Evaluations[indexI];

        for (int i = 0; i < domainSize; i++)
        {
            if (i != indexI)
            {
                q[i] = (f.Evaluations[i] - y) * inverses[i - indexI];
                q[indexI] += (f.Evaluations[i] - y) * inverses[indexI - i < 0? inverses.Length + indexI - i: indexI - i] * aPrimeDomain[indexI] *
                             aPrimeDomainInv[i];
            }
        }

        return q;
    }

    public static Fr[] ComputeQuotientOutsideDomain(PreComputeWeights precom, LagrangeBasis f, Fr z,
        Fr y)
    {
        var domain = precom.Domain;
        var domainSize = domain.Length;

        var q = new Fr[domainSize];
        for (int i = 0; i < domainSize; i++)
        {
            q[i] = (f.Evaluations[i] - y) / (domain[i] - z);
        }

        return q;
    }
}