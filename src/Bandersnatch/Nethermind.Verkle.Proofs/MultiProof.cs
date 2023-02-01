using Nethermind.Field.Montgomery.FrEElement;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Polynomial;

namespace Nethermind.Verkle.Proofs
{
    public class MultiProof
    {
        private readonly CRS _crs;
        private readonly PreComputeWeights _preComp;

        public MultiProof(CRS cRs, PreComputeWeights preComp)
        {
            _preComp = preComp;
            _crs = cRs;
        }

        public VerkleProofStruct MakeMultiProof(Transcript transcript, List<VerkleProverQuery> queries)
        {
            int domainSize = _preComp._domain.Length;
            transcript.DomainSep("multiproof");

            foreach (VerkleProverQuery query in queries)
            {
                transcript.AppendPoint(query._nodeCommitPoint, "C");
                transcript.AppendScalar(query._childIndex, "z");
                transcript.AppendScalar(query._childHash, "y");
            }
            FrE r = transcript.ChallengeScalar("r");

            FrE[] g = new FrE[domainSize];
            for (int i = 0; i < domainSize; i++)
            {
                g[i] = FrE.Zero;
            }

            FrE powerOfR = FrE.One;

            foreach (FrE[] quotient in from query in queries let f = query._childHashPoly let index = query._childIndex select Quotient.ComputeQuotientInsideDomain(_preComp, f, index))
            {
                for (int i = 0; i < domainSize; i++)
                {
                    g[i] += powerOfR * quotient[i];
                }
                powerOfR *= r;
            }

            Banderwagon d = _crs.Commit(g);
            transcript.AppendPoint(d, "D");
            FrE t = transcript.ChallengeScalar("t");

            FrE[] h = new FrE[domainSize];
            for (int i = 0; i < domainSize; i++)
            {
                h[i] = FrE.Zero;
            }

            powerOfR = FrE.One;

            foreach (VerkleProverQuery query in queries)
            {
                LagrangeBasis f = query._childHashPoly;
                int index = (int)query._childIndex.u0;
                FrE.Inverse(t - _preComp._domain[index], out FrE denominatorInv);
                for (int i = 0; i < domainSize; i++)
                {
                    h[i] += powerOfR * f.Evaluations[i] * denominatorInv;
                }
                powerOfR *= r;
            }

            FrE[] hMinusG = new FrE[domainSize];
            for (int i = 0; i < domainSize; i++)
            {
                hMinusG[i] = h[i] - g[i];
            }

            Banderwagon e = _crs.Commit(h);
            transcript.AppendPoint(e, "E");

            Banderwagon ipaCommitment = e - d;
            FrE[] inputPointVector = _preComp.BarycentricFormulaConstants(t);
            IpaProverQuery pQuery = new IpaProverQuery(hMinusG, ipaCommitment,
                t, inputPointVector);

            (FrE _, IpaProofStruct ipaProof) = Ipa.MakeIpaProof(_crs, transcript, pQuery);

            return new VerkleProofStruct(ipaProof, d);

        }

        public bool CheckMultiProof(Transcript transcript, VerkleVerifierQuery[] queries, VerkleProofStruct proof)
        {
            transcript.DomainSep("multiproof");
            Banderwagon d = proof._d;
            IpaProofStruct ipaIpaProof = proof._ipaProof;
            foreach (VerkleVerifierQuery query in queries)
            {
                transcript.AppendPoint(query._nodeCommitPoint, "C");
                transcript.AppendScalar(query._childIndex, "z");
                transcript.AppendScalar(query._childHash, "y");
            }

            FrE r = transcript.ChallengeScalar("r");

            transcript.AppendPoint(d, "D");
            FrE t = transcript.ChallengeScalar("t");

            Dictionary<byte[], FrE> eCoefficients = new Dictionary<byte[], FrE>();
            FrE g2OfT = FrE.Zero;
            FrE powerOfR = FrE.One;

            Dictionary<byte[], Banderwagon> cBySerialized = new Dictionary<byte[], Banderwagon>();

            foreach (VerkleVerifierQuery query in queries)
            {
                Banderwagon c = query._nodeCommitPoint;
                int z = (int)query._childIndex.u0;
                FrE y = query._childHash;
                FrE eCoefficient = powerOfR / t - _preComp._domain[z];
                byte[] cSerialized = c.ToBytes();
                cBySerialized[cSerialized] = c;
                if (!eCoefficients.ContainsKey(cSerialized))
                {
                    eCoefficients[cSerialized] = eCoefficient;
                }
                else
                {
                    eCoefficients[cSerialized] += eCoefficient;
                }

                g2OfT += eCoefficient * y;

                powerOfR *= r;
            }

            Banderwagon[] elems = new Banderwagon[eCoefficients.Count];
            for (int i = 0; i < eCoefficients.Count; i++)
            {
                elems[i] = cBySerialized[eCoefficients.Keys.ToArray()[i]];
            }

            Banderwagon e = VarBaseCommit(eCoefficients.Values.ToArray(), elems);
            transcript.AppendPoint(e, "E");

            FrE yO = g2OfT;
            Banderwagon ipaCommitment = e - d;
            FrE[] inputPointVector = _preComp.BarycentricFormulaConstants(t);

            IpaVerifierQuery queryX = new IpaVerifierQuery(ipaCommitment, t,
                inputPointVector, yO, ipaIpaProof);

            return Ipa.CheckIpaProof(_crs, transcript, queryX);
        }

        private static Banderwagon VarBaseCommit(FrE[] values, Banderwagon[] elements)
        {
            return Banderwagon.MSM(elements, values);
        }
    }
}
