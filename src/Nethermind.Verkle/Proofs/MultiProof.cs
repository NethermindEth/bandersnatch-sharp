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
        Dictionary<byte, LagrangeBasis> aggregatedPolyMap = new();
        for (int i = 0; i < queries.Count; i++)
        {
            LagrangeBasis f = queries[i].ChildHashPoly;
            byte evaluationPoint = queries[i].ChildIndex;

            LagrangeBasis scaledF = f * powersOfR[i];

            if (!aggregatedPolyMap.TryGetValue(evaluationPoint, out LagrangeBasis? poly))
            {
                aggregatedPolyMap[evaluationPoint] = scaledF;
                continue;
            }
            aggregatedPolyMap[evaluationPoint] = poly + scaledF;
        }


        FrE[] g = new FrE[domainSize];
        Span<FrE> quotient = new FrE[domainSize];
        foreach (KeyValuePair<byte, LagrangeBasis> pointAndPoly in aggregatedPolyMap)
        {
            Quotient.ComputeQuotientInsideDomain(PreComp, pointAndPoly.Value, pointAndPoly.Key, quotient);
            for (int j = 0; j < g.Length; j++)
            {
                g[j] += quotient[j];
            }
        }

        Banderwagon d = Crs.Commit(g);
        transcript.AppendPoint(d, "D");

        FrE t = transcript.ChallengeScalar("t");
        // We only will calculate inverses for domain points that are actually queried.
        FrE[] denomInvs = new FrE[domainSize];
        foreach (KeyValuePair<byte, LagrangeBasis> pointAndPoly in aggregatedPolyMap)
        {
            denomInvs[pointAndPoly.Key] = t - PreComp.Domain[pointAndPoly.Key];
        }
        denomInvs = FrE.MultiInverse(denomInvs);

        FrE[] h = new FrE[domainSize];
        foreach (KeyValuePair<byte, LagrangeBasis> pointAndPoly in aggregatedPolyMap)
        {
            LagrangeBasis f = pointAndPoly.Value;
            for (int j = 0; j < f.Evaluations.Length; j++)
            {
                h[j] += f.Evaluations[j] * denomInvs[pointAndPoly.Key];
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
