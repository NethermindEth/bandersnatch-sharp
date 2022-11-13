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
        Fr? result = new Fr((UInt256)0);

        foreach ((Fr? aI, Fr? bI) in Enumerable.Zip(a, b))
        {
            Fr? term = Fr.Mul(aI, bI);
            result += term;
        }

        return result;
    }

    public static (Fr Y, ProofStruct Proof) MakeIpaProof(CRS crs, Transcript transcript,
        ProverQuery query)
    {
        transcript.DomainSep("ipa");

        int n = query.Polynomial.Length;
        int m = n / 2;
        Fr[]? a = query.Polynomial;
        Fr[]? b = query.PointEvaluations;
        Fr? y = InnerProduct(a, b);

        ProofStruct proof = new ProofStruct(new(), Fr.Zero, new());

        transcript.AppendPoint(query.Commitment, Encoding.ASCII.GetBytes("C"));
        transcript.AppendScalar(query.Point, Encoding.ASCII.GetBytes("input point"));
        transcript.AppendScalar(y, Encoding.ASCII.GetBytes("output point"));
        Fr? w = transcript.ChallengeScalar(Encoding.ASCII.GetBytes("w"));

        Banderwagon? q = crs.BasisQ * w;

        Banderwagon[]? currentBasis = crs.BasisG;

        while (n > 1)
        {
            Fr[]? aL = a[..m];
            Fr[]? aR = a[m..];
            Fr[]? bL = b[..m];
            Fr[]? bR = b[m..];
            Fr? zL = InnerProduct(aR, bL);
            Fr? zR = InnerProduct(aL, bR);

            Banderwagon? cL = VarBaseCommit(aR, currentBasis[..m]) + (q * zL);
            Banderwagon? cR = VarBaseCommit(aL, currentBasis[m..]) + (q * zR);

            proof.L.Add(cL);
            proof.R.Add(cR);

            transcript.AppendPoint(cL, Encoding.ASCII.GetBytes("L"));
            transcript.AppendPoint(cR, Encoding.ASCII.GetBytes("R"));
            Fr? x = transcript.ChallengeScalar(Encoding.ASCII.GetBytes("x"));

            Fr? xinv = Fr.Inverse(x);

            a = new Fr[aL.Length];
            int i = 0;
            foreach ((Fr? V, Fr? W) in Enumerable.Zip(aL, aR))
            {
                a[i] = V + x * W;
                i++;
            }

            b = new Fr[aL.Length];
            i = 0;
            foreach ((Fr? V, Fr? W) in Enumerable.Zip(bL, bR))
            {
                b[i] = V + xinv * W;
                i++;
            }

            Banderwagon[] currentBasisN = new Banderwagon[m];
            i = 0;
            foreach ((Banderwagon? V, Banderwagon? W) in Enumerable.Zip(currentBasis[..m], currentBasis[m..]))
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

        int n = query.PointEvaluations.Length;
        int m = n / 2;


        Banderwagon? C = query.Commitment;
        Fr? z = query.Point;
        Fr[]? b = query.PointEvaluations;
        ProofStruct proof = query.Proof;
        Fr? y = query.OutputPoint;

        transcript.AppendPoint(C, Encoding.ASCII.GetBytes("C"));
        transcript.AppendScalar(z, Encoding.ASCII.GetBytes("input point"));
        transcript.AppendScalar(y, Encoding.ASCII.GetBytes("output point"));
        Fr? w = transcript.ChallengeScalar(Encoding.ASCII.GetBytes("w"));

        Banderwagon? q = crs.BasisQ * w;

        Banderwagon? currentCommitment = C + (q * y);

        int i = 0;
        List<Fr>? xs = new List<Fr>();
        List<Fr>? xinvs = new List<Fr>();


        while (n > 1)
        {
            Banderwagon? C_L = proof.L[i];
            Banderwagon? C_R = proof.R[i];

            transcript.AppendPoint(C_L, Encoding.ASCII.GetBytes("L"));
            transcript.AppendPoint(C_R, Encoding.ASCII.GetBytes("R"));
            Fr? x = transcript.ChallengeScalar(Encoding.ASCII.GetBytes("x"));

            Fr? xinv = Fr.Inverse(x);

            xs.Add(x);
            xinvs.Add(xinv);

            currentCommitment = currentCommitment + (C_L * x) + (C_R * xinv);
            n = m;
            m = n / 2;
            i = i + 1;
        }

        Banderwagon[]? currentBasis = crs.BasisG;

        for (int j = 0; j < xs.Count; j++)
        {
            (Banderwagon[]? G_L, Banderwagon[]? G_R) = SplitPoints(currentBasis);
            (Fr[]? b_L, Fr[]? b_R) = SplitScalars(b);

            Fr? x_inv = xinvs[j];

            b = FoldScalars(b_L, b_R, x_inv);
            currentBasis = FoldPoints(G_L, G_R, x_inv);

        }

        if (b.Length != currentBasis.Length)
            throw new Exception();

        if (b.Length != 1)
            throw new Exception();
        Fr? b0 = b[0];
        Banderwagon? g0 = currentBasis[0];

        Banderwagon? gotCommitment = g0 * proof.A + q * (proof.A * b0);

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
