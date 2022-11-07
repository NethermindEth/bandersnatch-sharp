using System.Text;
using Curve;
using Field;
using Nethermind.Int256;

namespace Proofs;
using Fp = FixedFiniteField<BandersnatchBaseFieldStruct>;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public static class IPA
{
    public static Banderwagon VarBaseCommit(Fr[] values, Banderwagon[] elements)
    {
        return Banderwagon.MSM(elements, values);
    }

    public static Fr InnerProduct(Fr[] a, Fr[] b)
    {
        var result = new Fr((UInt256)0);

        foreach (var (aI, bI) in Enumerable.Zip(a,b))
        {
            var term = Fr.Mul(aI, bI);
            result += term;
        }

        return result;
    }

    public static (Fr Y, ProofStruct Proof) MakeIpaProof(CRS crs, Transcript transcript,
        ProverQuery query)
    {
        transcript.DomainSep("ipa");

        var n = query.Polynomial.Length;
        var m = n / 2;
        var a = query.Polynomial;
        var b = query.PointEvaluations;
        var y = InnerProduct(a, b);

        var proof = new ProofStruct(new (), Fr.Zero, new());

        transcript.AppendPoint(query.Commitment, Encoding.ASCII.GetBytes("C"));
        transcript.AppendScalar(query.Point, Encoding.ASCII.GetBytes("input point"));
        transcript.AppendScalar(y, Encoding.ASCII.GetBytes("output point"));
        var w = transcript.ChallengeScalar(Encoding.ASCII.GetBytes("w"));

        var q = crs.BasisQ * w;

        var currentBasis = crs.BasisG;

        while (n > 1)
        {
            var aL = a[..m];
            var aR = a[m..];
            var bL = b[..m];
            var bR = b[m..];
            var zL = InnerProduct(aR, bL);
            var zR = InnerProduct(aL, bR);

            var cL = VarBaseCommit(aR, currentBasis[..m]) + (q * zL);
            var cR = VarBaseCommit(aL, currentBasis[m..]) + (q * zR);

            proof.L.Add(cL);
            proof.R.Add(cR);

            transcript.AppendPoint(cL, Encoding.ASCII.GetBytes("L"));
            transcript.AppendPoint(cR, Encoding.ASCII.GetBytes("R"));
            var x = transcript.ChallengeScalar(Encoding.ASCII.GetBytes("x"));

            var xinv = Fr.Inverse(x);

            a = new Fr[aL.Length];
            int i = 0;
            foreach (var (V, W) in Enumerable.Zip(aL, aR))
            {
                a[i] = V + x * W;
                i++;
            }

            b = new Fr[aL.Length];
            i = 0;
            foreach (var (V, W) in Enumerable.Zip(bL, bR))
            {
                b[i] = V + xinv * W;
                i++;
            }
            
            Banderwagon[] currentBasisN = new Banderwagon[m];
            i = 0;
            foreach (var (V, W) in Enumerable.Zip(currentBasis[..m], currentBasis[m..]))
            {
                currentBasisN[i] = V + W * xinv;
                i++;
            }

            currentBasis = currentBasisN;
            n = m;
            m = n / 2;
        }

        proof.A = a[0];

        return (y, proof);
    }
    
    public static bool CheckIpaProof(CRS crs, Transcript transcript,
        VerifierQuery query)
    {
        transcript.DomainSep(Encoding.ASCII.GetBytes("ipa"));

        var n = query.PointEvaluations.Length;
        var m = n / 2;


        var C = query.Commitment;
        var z = query.Point;
        var b = query.PointEvaluations;
        var proof = query.Proof;
        var y = query.OutputPoint;

        transcript.AppendPoint(C, Encoding.ASCII.GetBytes("C"));
        transcript.AppendScalar(z, Encoding.ASCII.GetBytes("input point"));
        transcript.AppendScalar(y, Encoding.ASCII.GetBytes("output point"));
        var w = transcript.ChallengeScalar(Encoding.ASCII.GetBytes("w"));

        var q = crs.BasisQ * w;

        var currentCommitment = C + (q * y);

        var i = 0;
        var xs = new List<Fr>();
        var xinvs = new List<Fr>();
        

        while (n > 1)
        {
            var C_L = proof.L[i];
            var C_R = proof.R[i];

            transcript.AppendPoint(C_L, Encoding.ASCII.GetBytes("L"));
            transcript.AppendPoint(C_R, Encoding.ASCII.GetBytes("R"));
            var x = transcript.ChallengeScalar(Encoding.ASCII.GetBytes("x"));

            var xinv = Fr.Inverse(x);

            xs.Add(x);
            xinvs.Add(xinv);

            currentCommitment = currentCommitment + (C_L * x) + (C_R * xinv);
            n = m;
            m = n / 2;
            i = i + 1;
        }

        var currentBasis = crs.BasisG;

        for (int j = 0; j < xs.Count; j++)
        {
            var (G_L, G_R) = SplitPoints(currentBasis);
            var (b_L, b_R) = SplitScalars(b);

            var x_inv = xinvs[j];

            b = FoldScalars(b_L, b_R, x_inv);
            currentBasis = FoldPoints(G_L, G_R, x_inv);

        }

        if (b.Length != currentBasis.Length)
            throw new Exception();

        if (b.Length != 1)
            throw new Exception();
        var b0 = b[0];
        var g0 = currentBasis[0];

        var gotCommitment = g0 * proof.A + q * (proof.A * b0);

        return currentCommitment == gotCommitment;
    }

    public static (T[] firstHalf, T[] secondHalf) SplitListInHalf<T>(T[] x)
    {
        if (x.Length % 2 != 0)
            throw new Exception();

        int mid = x.Length / 2;
        return (x[..mid], x[mid..]);
    }

    public static (Banderwagon[] firstHalf, Banderwagon[] secondHalf) SplitPoints(Banderwagon[] x)
    {
        return SplitListInHalf(x);
    }
    
    public static (Fr[] firstHalf, Fr[] secondHalf) SplitScalars(Fr[] x)
    {
        return SplitListInHalf(x);
    }

    public static Fr[] FoldScalars(Fr[] a, Fr[] b, Fr foldingChallenge)
    {
        if (a.Length != b.Length)
            throw new Exception();

        Fr[] result = new Fr[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            result[i] = a[i] + b[i] * foldingChallenge;
        }

        return result;
    }
    
    public static Banderwagon[] FoldPoints(Banderwagon[] a, Banderwagon[] b, Fr foldingChallenge)
    {
        if (a.Length != b.Length)
            throw new Exception();

        Banderwagon[] result = new Banderwagon[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            result[i] = a[i] + b[i] * foldingChallenge;
        }

        return result;
    }
    
}

public struct ProverQuery
{
    public Fr[] Polynomial;
    public Banderwagon Commitment;
    public Fr Point;
    public Fr[] PointEvaluations;

    public ProverQuery(Fr[] polynomial, Banderwagon commitment, Fr point,
        Fr[] pointEvaluations)
    {
        Polynomial = polynomial;
        Commitment = commitment;
        Point = point;
        PointEvaluations = pointEvaluations;
    }
}

public struct ProofStruct
{
    public List<Banderwagon> L;
    public Fr A;
    public List<Banderwagon> R;

    public ProofStruct(List<Banderwagon> l, Fr a, List<Banderwagon> r)
    {
        L = l;
        A = a;
        R = r;
    }
}

public struct VerifierQuery
{
    public Banderwagon Commitment;
    public Fr Point;
    public Fr[] PointEvaluations;
    public Fr OutputPoint;
    public ProofStruct Proof;

    public VerifierQuery(Banderwagon commitment, Fr point, Fr[] pointEvaluations, Fr outputPoint, ProofStruct proof)
    {
        Commitment = commitment;
        Point = point;
        PointEvaluations = pointEvaluations;
        OutputPoint = outputPoint;
        Proof = proof;
    }
}
