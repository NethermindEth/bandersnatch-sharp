using Nethermind.Verkle.Fields.FrEElement;

namespace Nethermind.Verkle.Polynomial
{
    public class PreComputeWeights
    {
        private const int VerkleNodeWidth = 256;
        public readonly MonomialBasis _a;
        public readonly MonomialBasis _aPrime;
        public readonly FrE[] _aPrimeDomain;
        public readonly FrE[] _aPrimeDomainInv;
        public readonly FrE[] _domain;
        public readonly FrE[] _domainInv;

        private PreComputeWeights()
        {
            _domain = new FrE[VerkleNodeWidth];
            for (int i = 0; i < VerkleNodeWidth; i++)
            {
                _domain[i] = FrE.SetElement(i);
            }
            _a = MonomialBasis.VanishingPoly(_domain);
            _aPrime = MonomialBasis.FormalDerivative(_a);

            _aPrimeDomain = new FrE[VerkleNodeWidth];
            _aPrimeDomainInv = new FrE[VerkleNodeWidth];

            for (int i = 0; i < VerkleNodeWidth; i++)
            {
                FrE aPrimeX = _aPrime.Evaluate(FrE.SetElement(i));
                FrE.Inverse(in aPrimeX, out FrE aPrimeXInv);
                _aPrimeDomain[i] = aPrimeX;
                _aPrimeDomainInv[i] = aPrimeXInv;
            }

            _domainInv = new FrE[2 * VerkleNodeWidth - 1];

            int index = 0;
            for (int i = 0; i < VerkleNodeWidth; i++)
            {
                FrE.Inverse(FrE.SetElement(i), out _domainInv[index]);
                index++;
            }

            for (int i = 1 - VerkleNodeWidth; i < 0; i++)
            {
                FrE.Inverse(FrE.SetElement(i), out _domainInv[index]);
                index++;
            }

        }

        public static PreComputeWeights Init()
        {
            // add cache here - write to file
            return new PreComputeWeights();
        }

        public FrE[] BarycentricFormulaConstants(FrE z)
        {
            FrE az = _a.Evaluate(z);

            FrE[] elems = new FrE[VerkleNodeWidth];
            for (int i = 0; i < VerkleNodeWidth; i++)
            {
                elems[i] = z - _domain[i];
            }

            FrE[] inverses = FrE.MultiInverse(elems);

            FrE[] r = new FrE[inverses.Length];

            for (int i = 0; i < inverses.Length; i++)
            {
                r[i] = az * _aPrimeDomainInv[i] * inverses[i];
            }

            return r;
        }
    }
}
