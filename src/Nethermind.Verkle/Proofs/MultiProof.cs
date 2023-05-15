
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

            foreach (VerkleProverQuery query in queries)
            {
                transcript.AppendPoint(query.NodeCommitPoint, "C");
                transcript.AppendScalar(query.ChildIndex, "z");
                transcript.AppendScalar(query.ChildHash, "y");
            }
            FrE r = transcript.ChallengeScalar("r");

            FrE[] g = new FrE[domainSize];

            FrE powerOfR = FrE.One;

            foreach (VerkleProverQuery query in queries)
            {
                LagrangeBasis f = query.ChildHashPoly;
                FrE index = query.ChildIndex;
                FrE[] quotient = Quotient.ComputeQuotientInsideDomain(PreComp, f, index);

                for (int i = 0; i < quotient.Length; i++)
                {
                    g[i] += powerOfR * quotient[i];
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
            IpaProverQuery pQuery = new IpaProverQuery(hMinusG, ipaCommitment,
                t, inputPointVector);

            IpaProofStruct ipaProof = Ipa.MakeIpaProof(Crs, transcript, pQuery, out _);

            return new VerkleProofStruct(ipaProof, d);

        }

        public bool CheckMultiProof(Transcript transcript, VerkleVerifierQuery[] queries, VerkleProofStruct proof)
        {
            transcript.DomainSep("multiproof");
            Banderwagon d = proof.D;
            IpaProofStruct ipaIpaProof = proof.IpaProof;
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

            FrE[] g2Den = queries.Select(query => t - query.ChildIndex).ToArray();
            g2Den = FrE.MultiInverse(g2Den);

            FrE[] helperScalars = powersOfR.Zip(g2Den).Select((elem, i) => elem.First * elem.Second).ToArray();
            FrE g2T = helperScalars.Zip(queries).Select((elem, i) => elem.First * elem.Second.ChildHash).Aggregate(FrE.Zero, (current, elem) => current + elem);
            IEnumerable<Banderwagon> commitments = queries.Select(query => query.NodeCommitPoint);

            Banderwagon g1Comm = Banderwagon.MultiScalarMul(commitments.ToArray(), helperScalars.ToArray());

            transcript.AppendPoint(g1Comm, "E");

            Banderwagon ipaCommitment = g1Comm - d;
            FrE[] inputPointVector = PreComp.BarycentricFormulaConstants(t);

            IpaVerifierQuery queryX = new IpaVerifierQuery(ipaCommitment, t,
                inputPointVector, g2T, ipaIpaProof);

            return Ipa.CheckIpaProof(Crs, transcript, queryX);
        }
    }
}
