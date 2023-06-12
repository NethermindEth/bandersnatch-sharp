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

        FrE[] g = new FrE[domainSize];
        for (int i = 0; i < queries.Count; i++)
        {
            VerkleProverQuery query = queries[i];
            LagrangeBasis f = query.ChildHashPoly;
            byte index = query.ChildIndex;
            FrE[] quotient = Quotient.ComputeQuotientInsideDomain(PreComp, f, index);

            for (int j = 0; j < quotient.Length; j++)
            {
                FrE.MultiplyMod(in powersOfR[i], in quotient[j], out FrE mul);
                g[j] += mul;
            }
        }

        Banderwagon d = Crs.Commit(g);
        transcript.AppendPoint(d, "D");

        FrE t = transcript.ChallengeScalar("t");
        FrE[] denomInvs = new FrE[queries.Count];
        for (int i = 0; i < queries.Count; i++)
        {
            denomInvs[i] = t - PreComp.Domain[queries[i].ChildIndex];
        }
        denomInvs = FrE.MultiInverse(denomInvs);

        FrE[] h = new FrE[domainSize];
        for (int i = 0; i < queries.Count; i++)
        {
            LagrangeBasis f = queries[i].ChildHashPoly;
            for (int j = 0; j < f.Evaluations.Length; j++)
            {
                h[j] += powersOfR[i] * f.Evaluations[j] * denomInvs[i];
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

        FrE[] groupedEvals = new FrE[domainSize];
        // Calculate groupedEvals = r * y_i.
        for (int i = 0; i < queries.Length; i++)
        {
            groupedEvals[queries[i].ChildIndex] += powersOfR[i] * queries[i].ChildHash;
        }


        // REMOVABLE COMMENT: The idea here is to avoid doing a lot of repeated inverses. If we have thousands of 
        // queries to check, there can only be up to 256 evaluations points.
        FrE[] helperScalarDens = new FrE[domainSize];
        // Compute helperScalarsDen = 1 / (t - z_i).
        for (int i = 0; i < domainSize; i++)
        {
            helperScalarDens[i] = t - FrE.SetElement(u0: queries[i].ChildIndex);
        }
        helperScalarDens = FrE.MultiInverse(helperScalarDens);

        FrE g2T = FrE.Zero;
        for (int i = 0; i < domainSize; i++)
        {
            // NIT: could the == operation be defined for FrEs? :)
            if (groupedEvals[i].Equals(FrE.Zero)) continue;
            // g2T = SUM [r^i * y_i] * [1 / (t - z_i)]
            g2T += groupedEvals[i] * helperScalarDens[i];
        }

        FrE[] helperScalars = new FrE[queries.Length];
        Banderwagon[] commitments = new Banderwagon[queries.Length];
        for (int i = 0; i < queries.Length; i++)
        {
            helperScalars[i] = helperScalarDens[queries[i].ChildIndex] * powersOfR[i];
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
