using Nethermind.Verkle.Fields.FrEElement;

namespace Nethermind.Verkle.Polynomial
{
    public class PreComputedWeights
    {
        public static PreComputedWeights Instance { get; } = new PreComputedWeights();

        private const int VerkleNodeWidth = 256;
        private MonomialBasis A { get; }
        public FrE[] APrimeDomain { get; }
        public FrE[] APrimeDomainInv { get; }
        public FrE[] Domain { get; }
        public FrE[] DomainInv { get; }

        private PreComputedWeights()
        {
            Domain = new FrE[VerkleNodeWidth];
            Parallel.For(0, VerkleNodeWidth, i => Domain[i] = FrE.SetElement(i));
            A = MonomialBasis.VanishingPoly(Domain);
            MonomialBasis aPrime = MonomialBasis.FormalDerivative(A);

            APrimeDomain = new FrE[VerkleNodeWidth];
            APrimeDomainInv = new FrE[VerkleNodeWidth];

            for (int i = 0; i < VerkleNodeWidth; i++)
            {
                FrE aPrimeX = aPrime.Evaluate(FrE.SetElement(i));
                FrE.Inverse(in aPrimeX, out FrE aPrimeXInv);
                APrimeDomain[i] = aPrimeX;
                APrimeDomainInv[i] = aPrimeXInv;
            }

            DomainInv = new FrE[2 * VerkleNodeWidth - 1];

            int index = 0;
            for (int i = 0; i < VerkleNodeWidth; i++)
            {
                FrE.Inverse(FrE.SetElement(i), out DomainInv[index]);
                index++;
            }

            for (int i = 1 - VerkleNodeWidth; i < 0; i++)
            {
                FrE.Inverse(FrE.SetElement(i), out DomainInv[index]);
                index++;
            }

        }

        public FrE[] BarycentricFormulaConstants(FrE z)
        {
            FrE az = A.Evaluate(z);

            FrE[] elems = new FrE[VerkleNodeWidth];
            for (int i = 0; i < VerkleNodeWidth; i++)
            {
                elems[i] = z - Domain[i];
            }

            FrE[] inverses = FrE.MultiInverse(elems);

            FrE[] r = new FrE[inverses.Length];

            for (int i = 0; i < inverses.Length; i++)
            {
                r[i] = az * APrimeDomainInv[i] * inverses[i];
            }

            return r;
        }
    }
}