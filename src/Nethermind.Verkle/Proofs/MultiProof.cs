
using Nethermind.Verkle.Fields.FrEElement;
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

            // create transcript for multiproof
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

            foreach (VerkleProverQuery query in queries)
            {
                LagrangeBasis f = query._childHashPoly;
                FrE index = query._childIndex;
                FrE[] quotient = Quotient.ComputeQuotientInsideDomain(_preComp, f, index);

                for (int i = 0; i < quotient.Length; i++)
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
                int index = query._childIndex.ToBytes()[0];
                FrE.Inverse(t - _preComp._domain[index], out FrE denominatorInv);
                LagrangeBasis f = query._childHashPoly;
                for (int i = 0; i < f.Evaluations.Length; i++)
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
            FrE[] powersOfR = new FrE[queries.Length];
            powersOfR[0] = FrE.One;

            for (int i = 1; i < queries.Length; i++)
            {
                powersOfR[i] = powersOfR[i - 1] * r;
            }

            transcript.AppendPoint(d, "D");
            FrE t = transcript.ChallengeScalar("t");

            FrE[] g2Den = queries.Select(query => t - query._childIndex).ToArray();
            g2Den = FrE.MultiInverse(g2Den);

            FrE[] helperScalars = powersOfR.Zip(g2Den).Select((elem, i) => elem.First * elem.Second).ToArray();
            FrE g2T = helperScalars.Zip(queries).Select((elem, i) => elem.First * elem.Second._childHash).Aggregate(FrE.Zero, (current, elem) => current + elem);
            IEnumerable<Banderwagon> comms = queries.Select(query => query._nodeCommitPoint);

            Banderwagon g1Comm = VarBaseCommit(helperScalars.ToArray(), comms.ToArray());

            transcript.AppendPoint(g1Comm, "E");

            Banderwagon ipaCommitment = g1Comm - d;
            FrE[] inputPointVector = _preComp.BarycentricFormulaConstants(t);

            IpaVerifierQuery queryX = new IpaVerifierQuery(ipaCommitment, t,
                inputPointVector, g2T, ipaIpaProof);

            return Ipa.CheckIpaProof(_crs, transcript, queryX);
        }

        private static Banderwagon VarBaseCommit(FrE[] values, Banderwagon[] elements)
        {
            return Banderwagon.MSM(elements, values);
        }
    }
}
