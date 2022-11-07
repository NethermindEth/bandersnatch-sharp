using Curve;
using Field;
using Polynomial;

namespace Proofs;
using Fp = FixedFiniteField<BandersnatchBaseFieldStruct>;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public class MultiProof
{
    private PreComputeWeights precomp;
    private CRS crs;

    public MultiProof(Fr[] domain, CRS cRS)
    {
        precomp = PreComputeWeights.Init(domain);
        crs = cRS;
    }

    public MultiProofStruct MakeMultiProof(Transcript transcript, MultiProofProverQuery[] queries)
    {
        var domainSize = precomp.Domain.Length;
        transcript.DomainSep("multiproof");

        foreach (var query in queries)
        {
            transcript.AppendPoint(query.C, "C");
            transcript.AppendScalar(query.z, "z");
            transcript.AppendScalar(query.y, "y");
        }

        var r = transcript.ChallengeScalar("r");
        Fr[] g = new Fr[domainSize];
        for (int i = 0; i < domainSize; i++)
        {
            g[i] = Fr.Zero;
        }

        var powerOfR = Fr.One;

        foreach (var query in queries)
        {
            var f = query.f;
            var index = query.z;
            var quotient = Quotient.ComputeQuotientInsideDomain(precomp, f, index);
            for (int i = 0; i < domainSize; i++)
            {
                g[i] += powerOfR * quotient[i];
            }

            powerOfR *= r;
        }

        var D = crs.Commit(g);
        transcript.AppendPoint(D, "D");
        var t = transcript.ChallengeScalar("t");
        
        Fr[] h = new Fr[domainSize];
        for (int i = 0; i < domainSize; i++)
        {
            h[i] = Fr.Zero;
        }

        powerOfR = Fr.One;

        foreach (var query in queries)
        {
            var f = query.f;
            var index = query.z.ToInt();
            var denominatorInv = Fr.Inverse(t - precomp.Domain[index]);

            for (int i = 0; i < domainSize; i++)
            {
                h[i] += powerOfR * f.Evaluations[i] * denominatorInv;
            }

            powerOfR = powerOfR * r;
        }

        var hMinusG = new Fr[domainSize];
        for (int i = 0; i < domainSize; i++)
        {
            hMinusG[i] = h[i] - g[i];
        }

        var E = crs.Commit(h);
        transcript.AppendPoint(E, "E");

        var ipaCommitment = E - D;
        var inputPointVector = precomp.BarycentricFormulaConstants(t);
        var pQuery = new ProverQuery(hMinusG, ipaCommitment,
            t, inputPointVector);
        
        var (outputPoint, ipaProof) = IPA.MakeIpaProof(crs, transcript, pQuery);

        return new MultiProofStruct(ipaProof, D);

    }
    
    public bool CheckMultiProof(Transcript transcript, MultiProofVerifierQuery[] queries, MultiProofStruct proof)
    {
        transcript.DomainSep("multiproof");
        var D = proof.D;
        var ipaProof = proof.IpaProof;
        foreach (var query in queries)
        {
            transcript.AppendPoint(query.C, "C");
            transcript.AppendScalar(query.z, "z");
            transcript.AppendScalar(query.y, "y");
        }

        var r = transcript.ChallengeScalar("r");

        transcript.AppendPoint(D, "D");
        var t = transcript.ChallengeScalar("t");

        Dictionary<byte[], Fr> eCoefficients = new();
        var g2OfT = Fr.Zero;
        var powerOfR = Fr.One;

        Dictionary<byte[], Banderwagon> cBySerialized = new();

        foreach (var query in queries)
        {
            var C = query.C;
            var z = query.z.ToInt();
            var y = query.y;
            var eCoefficient = (powerOfR / t) - precomp.Domain[z];
            var cSerialized = C.ToBytes();
            cBySerialized[cSerialized] = C;
            if (!eCoefficients.ContainsKey(cSerialized))
            {
                eCoefficients[cSerialized] = eCoefficient;
            }
            else
            {
                eCoefficients[cSerialized] += eCoefficient;
            }

            g2OfT += eCoefficient * y;

            powerOfR = powerOfR * r;
        }

        Banderwagon[] elems = new Banderwagon[eCoefficients.Count];
        for (int i = 0; i < eCoefficients.Count; i++)
        {
            elems[i] = cBySerialized[eCoefficients.Keys.ToArray()[i]];
        }

        var E = VarBaseCommit(eCoefficients.Values.ToArray(), elems);
        transcript.AppendPoint(E, "E");

        var yO = g2OfT;
        var ipaCommitment = E - D;
        var inputPointVector = precomp.BarycentricFormulaConstants(t);

        var queryX = new VerifierQuery(ipaCommitment, t,
            inputPointVector, yO, ipaProof);

        return IPA.CheckIpaProof(crs, transcript, queryX);

    }

    public static Banderwagon VarBaseCommit(Fr[] values, Banderwagon[] elements)
    {
        return Banderwagon.MSM(elements, values);
    }
}

public struct MultiProofStruct
{
    public ProofStruct IpaProof;
    public Banderwagon D;

    public MultiProofStruct(ProofStruct ipaProof, Banderwagon d)
    {
        IpaProof = ipaProof;
        D = d;
    }
}

public struct MultiProofProverQuery
{
    public LagrangeBasis f;
    public Banderwagon C;
    public Fr z;
    public Fr y;

    public MultiProofProverQuery(LagrangeBasis _f, Banderwagon _C, Fr _z, Fr _y)
    {
        f = _f;
        C = _C;
        z = _z;
        y = _y;
    }
}

public struct MultiProofVerifierQuery
{
    public Banderwagon C;
    public Fr z;
    public Fr y;

    public MultiProofVerifierQuery(Banderwagon _C, Fr _z, Fr _y)
    {
        C = _C;
        z = _z;
        y = _y;
    }
}