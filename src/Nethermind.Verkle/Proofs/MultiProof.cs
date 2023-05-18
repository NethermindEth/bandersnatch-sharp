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

        // watch.Stop();
        // Console.WriteLine($"    Generate Challenge r and powers of r: {watch.ElapsedMilliseconds}ms");
        // watch = Stopwatch.StartNew();

        FrE[] g = new FrE[domainSize];
        for (int i = 0; i < queries.Count; i++)
        {
            VerkleProverQuery query = queries[i];
            LagrangeBasis f = query.ChildHashPoly;
            byte index = query.ChildIndex;
            FrE[] quotient = Quotient.ComputeQuotientInsideDomain(PreComp, f, index);

            for (int j = 0; j < quotient.Length; j++)
            {
                g[j] += powersOfR[i] * quotient[j];
            }
        }

        Banderwagon d = Crs.Commit(g);
        transcript.AppendPoint(d, "D");

        // watch.Stop();
        // Console.WriteLine($"    Calculate t, g(x) and D: {watch.ElapsedMilliseconds}ms");
        // watch = Stopwatch.StartNew();

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

        // watch.Stop();
        // Console.WriteLine($"    Calculate h(x) and E: {watch.ElapsedMilliseconds}ms");
        // watch = Stopwatch.StartNew();

        FrE[] hMinusG = new FrE[domainSize];
        for (int i = 0; i < domainSize; i++)
        {
            hMinusG[i] = h[i] - g[i];
        }

        Banderwagon ipaCommitment = e - d;

        // watch.Stop();
        // Console.WriteLine($"    Calculate (h-g)(x) and E-D: {watch.ElapsedMilliseconds}ms");
        // watch = Stopwatch.StartNew();

        FrE[] inputPointVector = PreComp.BarycentricFormulaConstants(t);
        IpaProverQuery pQuery = new(hMinusG, ipaCommitment, t, inputPointVector);
        IpaProofStruct ipaProof = Ipa.MakeIpaProof(Crs, transcript, pQuery, out _);

        // watch.Stop();
        // Console.WriteLine($"    IPA for (h-g)(x) and E-D on t: {watch.ElapsedMilliseconds}ms");

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
