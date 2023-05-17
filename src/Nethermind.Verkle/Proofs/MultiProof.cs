using Nethermind.Verkle.Fields.FrEElement;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Polynomial;

// ReSharper disable InconsistentNaming

namespace Nethermind.Verkle.Proofs
{
    public class MultiProof
    {
        private readonly CRS Crs;
        private readonly PreComputedWeights PreComp;

        public MultiProof(CRS cRs, PreComputedWeights preComp)
        {
            PreComp = preComp;
            Crs = cRs;
        }

        public VerkleProofStruct MakeMultiProof(Transcript transcript, List<VerkleProverQuery> queries)
        {
            int domainSize = PreComp.Domain.Length;

            transcript.DomainSep("multiproof");
            for (int i = 0; i < queries.Count; i++)
            {
                transcript.AppendPoint(queries[i].NodeCommitPoint, "C");
                transcript.AppendScalar(queries[i].ChildIndex, "z");
                transcript.AppendScalar(queries[i].ChildHash, "y");
            }

            FrE r = transcript.ChallengeScalar("r");
            FrE[] g = new FrE[domainSize];
            FrE powerOfR = FrE.One;
            FrE[] powersOfR = new FrE[queries.Count];

            for (int i = 0; i < queries.Count; i++)
            {
                powersOfR[i] = powerOfR;
                VerkleProverQuery query = queries[i];
                LagrangeBasis f = query.ChildHashPoly;
                FrE index = query.ChildIndex;
                FrE[] quotient = Quotient.ComputeQuotientInsideDomain(PreComp, f, index);

                for (int j = 0; j < quotient.Length; j++)
                {
                    g[j] += powerOfR * quotient[j];
                }

                powerOfR *= r;
            }

            Banderwagon d = Crs.Commit(g);

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
                int index = query.ChildIndex.ToBytes()[0];
                FrE.Inverse(t - PreComp.Domain[index], out FrE denominatorInv);
                LagrangeBasis f = query.ChildHashPoly;
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

            Banderwagon e = Crs.Commit(h);
            transcript.AppendPoint(e, "E");

            Banderwagon ipaCommitment = e - d;
            FrE[] inputPointVector = PreComp.BarycentricFormulaConstants(t);
            IpaProverQuery pQuery = new(hMinusG, ipaCommitment,
                t, inputPointVector);

            IpaProofStruct ipaProof = Ipa.MakeIpaProof(Crs, transcript, pQuery, out _);

            return new VerkleProofStruct(ipaProof, d);
        }

        public bool CheckMultiProof(Transcript transcript, VerkleVerifierQuery[] queries, VerkleProofStruct proof)
        {
            transcript.DomainSep("multiproof");
            Banderwagon d = proof.D;
            IpaProofStruct ipaProof = proof.IpaProof;
            foreach (VerkleVerifierQuery query in queries)
            {
                transcript.AppendPoint(query.NodeCommitPoint, "C");
                transcript.AppendScalar(query.ChildIndex, "z");
                transcript.AppendScalar(query.ChildHash, "y");
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

            FrE[] g2Den = new FrE[queries.Length];
            for (int i = 0; i < queries.Length; i++)
            {
                g2Den[i] = t - queries[i].ChildIndex;
            }
            g2Den = FrE.MultiInverse(g2Den);

            FrE g2T = FrE.Zero;
            FrE[] helperScalars = new FrE[queries.Length];
            Banderwagon[] commitments = new Banderwagon[queries.Length];
            for (int i = 0; i < queries.Length; i++)
            {
                helperScalars[i] = g2Den[i] * powersOfR[i];
                g2T += helperScalars[i] * queries[i].ChildHash;
                commitments[i] = queries[i].NodeCommitPoint;
            }

            Banderwagon g1Comm = Banderwagon.MultiScalarMul(commitments, helperScalars);

            transcript.AppendPoint(g1Comm, "E");

            FrE[] inputPointVector = PreComp.BarycentricFormulaConstants(t);
            Banderwagon ipaCommitment = g1Comm - d;
            IpaVerifierQuery queryX = new(ipaCommitment, t, inputPointVector, g2T, ipaProof);

            return Ipa.CheckIpaProof(Crs, transcript, queryX);
        }
    }
}
