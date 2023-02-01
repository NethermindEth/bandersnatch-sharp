using Nethermind.Field.Montgomery.FrEElement;
using Nethermind.Verkle.Curve;

namespace Nethermind.Verkle.Proofs
{
    public static class IPA
    {
        public static Banderwagon VarBaseCommit(FrE[] values, Banderwagon[] elements)
        {
            return Banderwagon.MSM(elements, values);
        }

        public static FrE InnerProduct(FrE[] a, FrE[] b)
        {
            FrE result = FrE.SetElement();

            foreach ((FrE aI, FrE bI) in a.Zip(b))
            {
                FrE.MultiplyMod(aI, bI, out FrE term);
                result += term;
            }

            return result;
        }

        public static (FrE Y, IpaProofStruct Proof) MakeIpaProof(CRS crs, Transcript transcript,
            IpaProverQuery query)
        {
            transcript.DomainSep("ipa");

            int n = query.Polynomial.Length;
            int m = n / 2;
            FrE[] a = query.Polynomial;
            FrE[] b = query.PointEvaluations;
            FrE y = InnerProduct(a, b);

            IpaProofStruct ipaProof = new IpaProofStruct(new List<Banderwagon>(), FrE.Zero, new List<Banderwagon>());

            transcript.AppendPoint(query.Commitment, "C"u8.ToArray());
            transcript.AppendScalar(query.Point, "input point"u8.ToArray());
            transcript.AppendScalar(y, "output point"u8.ToArray());
            FrE w = transcript.ChallengeScalar("w"u8.ToArray());

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

                Banderwagon cL = VarBaseCommit(aR, currentBasis[..m]) + q * zL;
                Banderwagon cR = VarBaseCommit(aL, currentBasis[m..]) + q * zR;

                ipaProof.L.Add(cL);
                ipaProof.R.Add(cR);

                transcript.AppendPoint(cL, "L"u8.ToArray());
                transcript.AppendPoint(cR, "R"u8.ToArray());
                FrE x = transcript.ChallengeScalar("x"u8.ToArray());

                FrE.Inverse(x, out FrE xinv);

                a = new FrE[aL.Length];
                int i = 0;
                foreach ((FrE V, FrE W) in aL.Zip(aR))
                {
                    a[i] = V + x * W;
                    i++;
                }

                b = new FrE[aL.Length];
                i = 0;
                foreach ((FrE V, FrE W) in bL.Zip(bR))
                {
                    b[i] = V + xinv * W;
                    i++;
                }

                Banderwagon[] currentBasisN = new Banderwagon[m];
                i = 0;
                foreach ((Banderwagon V, Banderwagon W) in currentBasis[..m].Zip(currentBasis[m..]))
                {
                    currentBasisN[i] = V + W * xinv;
                    i++;
                }

                currentBasis = currentBasisN;
                n = m;
                m = n / 2;
            }

            ipaProof.A = a[0];

            return (y, ipaProof);
        }

        public static bool CheckIpaProof(CRS crs, Transcript transcript,
            IpaVerifierQuery query)
        {
            transcript.DomainSep("ipa"u8.ToArray());

            int n = query.PointEvaluations.Length;
            int m = n / 2;


            Banderwagon C = query.Commitment;
            FrE z = query.Point;
            FrE[] b = query.PointEvaluations;
            IpaProofStruct ipaProof = query._ipaProof;
            FrE y = query.OutputPoint;

            transcript.AppendPoint(C, "C"u8.ToArray());
            transcript.AppendScalar(z, "input point"u8.ToArray());
            transcript.AppendScalar(y, "output point"u8.ToArray());
            FrE w = transcript.ChallengeScalar("w"u8.ToArray());

            Banderwagon q = crs.BasisQ * w;

            Banderwagon currentCommitment = C + q * y;

            int i = 0;
            List<FrE> xs = new List<FrE>();
            List<FrE> xinvs = new List<FrE>();


            while (n > 1)
            {
                Banderwagon C_L = ipaProof.L[i];
                Banderwagon C_R = ipaProof.R[i];

                transcript.AppendPoint(C_L, "L"u8.ToArray());
                transcript.AppendPoint(C_R, "R"u8.ToArray());
                FrE x = transcript.ChallengeScalar("x"u8.ToArray());

                FrE.Inverse(in x, out FrE xinv);

                xs.Add(x);
                xinvs.Add(xinv);

                currentCommitment = currentCommitment + C_L * x + C_R * xinv;
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

            Banderwagon gotCommitment = g0 * ipaProof.A + q * (ipaProof.A * b0);

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
}
