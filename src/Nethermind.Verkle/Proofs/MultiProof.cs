using System.Diagnostics;
using Nethermind.Verkle.Fields.FrEElement;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Polynomial;

// ReSharper disable InconsistentNaming

namespace Nethermind.Verkle.Proofs;

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

        // Stopwatch watch = new();
        // watch.Start();

        transcript.DomainSep("multiproof");
        for (int i = 0; i < queries.Count; i++)
        {
            transcript.AppendPoint(queries[i].NodeCommitPoint, "C");
            transcript.AppendScalar(queries[i].ChildIndex, "z");
            transcript.AppendScalar(queries[i].ChildHash, "y");
        }

        FrE r = transcript.ChallengeScalar("r");
        FrE[] powersOfR = new FrE[queries.Count];
        powersOfR[0] = FrE.One;
        for (int i = 1; i < queries.Count; i++)
        {
            FrE.MultiplyMod(in powersOfR[i - 1], in r, out powersOfR[i]);
        }

        // We aggregate all the polynomials in evaluation form per domain point
        // to avoid work downstream.
        LagrangeBasis[] aggregatedPolys = new LagrangeBasis[domainSize];
        for (int i = 0; i < queries.Count; i++)
        {
            LagrangeBasis f = queries[i].ChildHashPoly;
            // TODO: GetEvaluationPoint() should return _domain. 
            FrE evaluationpoint = f.GetEvaluationPoint();
            if (aggregatedPolys[evaluationPoint] == null)
            {
                aggregatedPolys[i] = new LagrangeBasis(f); // TODO: ~copy constructor?
                continue;
            }
            FrE queryR = powersOfR[i];
            LagrangeBasis scaledF = f * queryR;

            aggregatedPolys[evaluationPoint] += scaledF;
        }


        // REMOVABLE COMMENT: now we work on aggregatedPolys. Remember that we already multiplied by `r` so it was removed here.
        FrE[] g = new FrE[domainSize];
        for (int i = 0; i < domainSize; i++)
        {
            if (aggregatedPolys[i] == null)
            {
                continue;
            }

            FrE[] quotient = Quotient.ComputeQuotientInsideDomain(PreComp, aggregatedPolys[i], i);
            g += quotient;
        }

        Banderwagon d = Crs.Commit(g);
        transcript.AppendPoint(d, "D");

        FrE t = transcript.ChallengeScalar("t");
        // We only will calculate inverses for domain points that are actually queried.
        FrE[] denomInvs = new FrE[domainSize];
        for (int i = 0; i < domainSize; i++)
        {
            if (aggregatedPolys[i] == null)
            {
                // REMOVABLE COMMENT: note how domainInvs[i] will be zero. We'll skip it in the next loop.
                continue;
            }
            denomInvs[i] = t - PreComp.Domain[i];
        }
        denomInvs = FrE.MultiInverse(denomInvs);

        FrE[] h = new FrE[domainSize];
        for (int i = 0; i < domainSize; i++)
        {
            if (aggregatedPolys[i] == null)
            {
                // REMOVABLE COMMENT: Note how we'll skip accessing denomInvs[i] here, since it's zero.
                continue;
            }
            LagrangeBasis f = aggregatedPolys[i];
            for (int j = 0; j < f.Evaluations.Length; j++)
            {
                // REMOVABLE COMMENT: note that 'r' multiplication is removed, since we already did
                // that multiplication when we created aggregatedPolys[i]
                h[j] += f.Evaluations[j] * denomInvs[i];
            }
        }

        Banderwagon e = Crs.Commit(h);
        transcript.AppendPoint(e, "E");

        FrE[] hMinusG = new FrE[domainSize];
        for (int i = 0; i < domainSize; i++)
        {
            hMinusG[i] = h[i] - g[i];
        }

        Banderwagon ipaCommitment = e - d;

        FrE[] inputPointVector = PreComp.BarycentricFormulaConstants(t);
        IpaProverQuery pQuery = new(hMinusG, ipaCommitment, t, inputPointVector);
        IpaProofStruct ipaProof = Ipa.MakeIpaProof(Crs, transcript, pQuery, out _);

        return new VerkleProofStruct(ipaProof, d);
    }

    public bool CheckMultiProof(Transcript transcript, VerkleVerifierQuery[] queries, VerkleProofStruct proof)
    {
        transcript.DomainSep("multiproof");
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

        Banderwagon d = proof.D;
        IpaProofStruct ipaProof = proof.IpaProof;
        transcript.AppendPoint(d, "D");
        FrE t = transcript.ChallengeScalar("t");

        FrE[] g2Den = new FrE[queries.Length];
        for (int i = 0; i < queries.Length; i++)
        {
            g2Den[i] = t - FrE.SetElement(u0: queries[i].ChildIndex);
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
