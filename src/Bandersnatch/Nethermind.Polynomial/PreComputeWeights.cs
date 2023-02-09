
namespace Nethermind.Polynomial
{
    public class PreComputeWeights
    {
        public MonomialBasis A;
        public MonomialBasis APrime;
        public FrE[] APrimeDomain;
        public FrE[] APrimeDomainInv;
        public FrE[] Domain;
        public FrE[] DomainInv;

        private PreComputeWeights()
        {

        }

        public static PreComputeWeights Init(FrE[] domain)
        {
            PreComputeWeights res = new PreComputeWeights();
            res.Domain = domain;
            int domainSize = domain.Length;

            res.A = MonomialBasis.VanishingPoly(domain);
            res.APrime = MonomialBasis.FormalDerivative(res.A);

            FrE[] aPrimeDom = new FrE[domain.Length];
            FrE[] aPrimeDomInv = new FrE[domain.Length];

            for (int i = 0; i < domain.Length; i++)
            {
                FrE aPrimeX = res.APrime.Evaluate(FrE.SetElement(i));
                FrE.Inverse(in aPrimeX, out FrE aPrimeXInv);
                aPrimeDom[i] = aPrimeX;
                aPrimeDomInv[i] = aPrimeXInv;
            }

            res.APrimeDomain = aPrimeDom;
            res.APrimeDomainInv = aPrimeDomInv;

            res.DomainInv = new FrE[2 * domainSize - 1];

            int index = 0;
            for (int i = 0; i < domainSize; i++)
            {
                FrE.Inverse(FrE.SetElement(i), out res.DomainInv[index]);
                index++;
            }

            for (int i = 1 - domainSize; i < 0; i++)
            {
                FrE.Inverse(FrE.SetElement(i), out res.DomainInv[index]);
                index++;
            }

            return res;
        }

        public FrE[] BarycentricFormulaConstants(FrE z)
        {
            FrE Az = A.Evaluate(z);

            FrE[] elems = new FrE[Domain.Length];
            for (int i = 0; i < Domain.Length; i++)
            {
                elems[i] = z - Domain[i];
            }

            FrE[] inverses = FrE.MultiInverse(elems);

            FrE[] r = new FrE[inverses.Length];

            for (int i = 0; i < inverses.Length; i++)
            {
                r[i] = Az * APrimeDomainInv[i] * inverses[i];
            }

            return r;
        }
    }
}
