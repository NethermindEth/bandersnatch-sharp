using Nethermind.Field;
using Nethermind.Verkle.Curve;

namespace Nethermind.Verkle.Polynomial;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public class PreComputeWeights
{
    public MonomialBasis A;
    public MonomialBasis APrime;
    public Fr?[] APrimeDomain;
    public Fr?[] APrimeDomainInv;
    public Fr?[] Domain;
    public Fr?[] DomainInv;

    private PreComputeWeights()
    {

    }

    public static PreComputeWeights Init(Fr[] domain)
    {
        PreComputeWeights? res = new PreComputeWeights();
        res.Domain = domain;
        int domainSize = domain.Length;

        res.A = MonomialBasis.VanishingPoly(domain);
        res.APrime = MonomialBasis.FormalDerivative(res.A);

        Fr?[]? aPrimeDom = new Fr?[domain.Length];
        Fr?[]? aPrimeDomInv = new Fr?[domain.Length];

        for (int i = 0; i < domain.Length; i++)
        {
            Fr? aPrimeX = res.APrime.Evaluate(new Fr(i));
            Fr? aPrimeXInv = Fr.Inverse(aPrimeX);
            aPrimeDom[i] = aPrimeX;
            aPrimeDomInv[i] = aPrimeXInv;
        }

        res.APrimeDomain = aPrimeDom;
        res.APrimeDomainInv = aPrimeDomInv;

        res.DomainInv = new Fr?[2 * domainSize - 1];

        int index = 0;
        for (int i = 0; i < domainSize; i++)
        {
            res.DomainInv[index] = Fr.Inverse(new Fr(i));
            index++;
        }

        for (int i = 1 - domainSize; i < 0; i++)
        {
            res.DomainInv[index] = Fr.Inverse(new Fr(i));
            index++;
        }

        return res;
    }

    public Fr[] BarycentricFormulaConstants(Fr z)
    {
        Fr? Az = A.Evaluate(z);

        Fr[] elems = new Fr[Domain.Length];
        for (int i = 0; i < Domain.Length; i++)
        {
            elems[i] = z - Domain[i];
        }

        Fr[]? inverses = Fr.MultiInverse(elems);

        Fr[] r = new Fr[inverses.Length];

        for (int i = 0; i < inverses.Length; i++)
        {
            r[i] = Az * APrimeDomainInv[i] * inverses[i];
        }

        return r;
    }
}
