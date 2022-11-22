using System.Text;
using Nethermind.Field.Montgomery;
using Nethermind.Int256;
using Nethermind.Verkle.Curve;
using Nethermind.Field.Montgomery;

namespace Nethermind.Verkle.Proofs;


public static class IPA
{
    public static Banderwagon VarBaseCommit(FrE[] values, Banderwagon[] elements)
    {
        return Banderwagon.MSM(elements, values);
    }

    public static FrE InnerProduct(FrE[] a, FrE[] b)
    {
        FrE result = FrE.SetElement(0);

        foreach ((FrE aI, FrE bI) in Enumerable.Zip(a, b))
        {
            FrE.MulMod(aI, bI, out FrE term);
            result += term;
        }

        return result;
    }

    public static (FrE Y, ProofStruct Proof) MakeIpaProof(CRS crs, Transcript transcript,
        ProverQuery query)
    {
        transcript.DomainSep("ipa");

        int n = query.Polynomial.Length;
        int m = n / 2;
        FrE[] a = query.Polynomial;
        FrE[] b = query.PointEvaluations;
        FrE y = InnerProduct(a, b);

        ProofStruct proof = new ProofStruct(new(), FrE.Zero, new());

        transcript.AppendPoint(query.Commitment, Encoding.ASCII.GetBytes("C"));
        transcript.AppendScalar(query.Point, Encoding.ASCII.GetBytes("input point"));
        transcript.AppendScalar(y, Encoding.ASCII.GetBytes("output point"));
        FrE w = transcript.ChallengeScalar(Encoding.ASCII.GetBytes("w"));

        Banderwagon q = crs.BasisQ * w;

        Banderwagon[] currentBasis = crs.BasisG;

        while (n > 1)
        {
            FrE[] aL = a[..m];
            FrE[] aR = a[m..];
            FrE[] bL = b[..m];
            FrE[] bR = b[m..];
            FrE zL = InnerProduct(aR, bL);
            FrE zR = InnerProduct(aL, bR);

            Banderwagon cL = VarBaseCommit(aR, currentBasis[..m]) + (q * zL);
            Banderwagon cR = VarBaseCommit(aL, currentBasis[m..]) + (q * zR);

            proof.L.Add(cL);
            proof.R.Add(cR);

            transcript.AppendPoint(cL, Encoding.ASCII.GetBytes("L"));
            transcript.AppendPoint(cR, Encoding.ASCII.GetBytes("R"));
            FrE x = transcript.ChallengeScalar(Encoding.ASCII.GetBytes("x"));

            FrE.Inverse(x, out FrE xinv);

            a = new FrE[aL.Length];
            int i = 0;
            foreach ((FrE V, FrE W) in Enumerable.Zip(aL, aR))
            {
                a[i] = V + x * W;
                i++;
            }

            b = new FrE[aL.Length];
            i = 0;
            foreach ((FrE V, FrE W) in Enumerable.Zip(bL, bR))
            {
                b[i] = V + xinv * W;
                i++;
            }

            Banderwagon[] currentBasisN = new Banderwagon[m];
            i = 0;
            foreach ((Banderwagon V, Banderwagon W) in Enumerable.Zip(currentBasis[..m], currentBasis[m..]))
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


        Banderwagon C = query.Commitment;
        FrE z = query.Point;
        FrE[] b = query.PointEvaluations;
        ProofStruct proof = query.Proof;
        FrE y = query.OutputPoint;

        transcript.AppendPoint(C, Encoding.ASCII.GetBytes("C"));
        transcript.AppendScalar(z, Encoding.ASCII.GetBytes("input point"));
        transcript.AppendScalar(y, Encoding.ASCII.GetBytes("output point"));
        FrE w = transcript.ChallengeScalar(Encoding.ASCII.GetBytes("w"));

        Banderwagon q = crs.BasisQ * w;

        Banderwagon currentCommitment = C + (q * y);

        int i = 0;
        List<FrE> xs = new List<FrE>();
        List<FrE> xinvs = new List<FrE>();


        while (n > 1)
        {
            Banderwagon C_L = proof.L[i];
            Banderwagon C_R = proof.R[i];

            transcript.AppendPoint(C_L, Encoding.ASCII.GetBytes("L"));
            transcript.AppendPoint(C_R, Encoding.ASCII.GetBytes("R"));
            FrE x = transcript.ChallengeScalar(Encoding.ASCII.GetBytes("x"));

            FrE.Inverse(in x, out FrE xinv);

            xs.Add(x);
            xinvs.Add(xinv);

            currentCommitment = currentCommitment + (C_L * x) + (C_R * xinv);
            n = m;
            m = n / 2;
            i = i + 1;
        }

        Banderwagon[] currentBasis = crs.BasisG;

        for (int j = 0; j < xs.Count; j++)
        {
            (Banderwagon[] G_L, Banderwagon[] G_R) = SplitPoints(currentBasis);
            (FrE[] b_L, FrE[] b_R) = SplitScalars(b);

            FrE x_inv = xinvs[j];

            b = FoldScalars(b_L, b_R, x_inv);
            currentBasis = FoldPoints(G_L, G_R, x_inv);

        }

        if (b.Length != currentBasis.Length)
            throw new Exception();

        if (b.Length != 1)
            throw new Exception();
        FrE b0 = b[0];
        Banderwagon g0 = currentBasis[0];

        Banderwagon gotCommitment = g0 * proof.A + q * (proof.A * b0);

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

    public static (FrE[] firstHalf, FrE[] secondHalf) SplitScalars(FrE[] x)
    {
        return SplitListInHalf(x);
    }

    public static FrE[] FoldScalars(FrE[] a, FrE[] b, FrE foldingChallenge)
    {
        if (a.Length != b.Length)
            throw new Exception();

        FrE[] result = new FrE[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            result[i] = a[i] + b[i] * foldingChallenge;
        }

        return result;
    }

    public static Banderwagon[] FoldPoints(Banderwagon[] a, Banderwagon[] b, FrE foldingChallenge)
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
    public FrE[] Polynomial;
    public Banderwagon Commitment;
    public FrE Point;
    public FrE[] PointEvaluations;

    public ProverQuery(FrE[] polynomial, Banderwagon commitment, FrE point,
        FrE[] pointEvaluations)
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
    public FrE A;
    public List<Banderwagon> R;

    public ProofStruct(List<Banderwagon> l, FrE a, List<Banderwagon> r)
    {
        L = l;
        A = a;
        R = r;
    }
}

public struct VerifierQuery
{
    public Banderwagon Commitment;
    public FrE Point;
    public FrE[] PointEvaluations;
    public FrE OutputPoint;
    public ProofStruct Proof;

    public VerifierQuery(Banderwagon commitment, FrE point, FrE[] pointEvaluations, FrE outputPoint, ProofStruct proof)
    {
        Commitment = commitment;
        Point = point;
        PointEvaluations = pointEvaluations;
        OutputPoint = outputPoint;
        Proof = proof;
    }
}
