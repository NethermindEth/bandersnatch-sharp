using Nethermind.Field.Montgomery.FrEElement;
using Nethermind.Verkle.Curve;

namespace Nethermind.Verkle.Proofs
{
    public static class Ipa
    {
        private static Banderwagon VarBaseCommit(FrE[] values, Banderwagon[] elements)
        {
            return Banderwagon.MSM(elements, values);
        }

        public static FrE InnerProduct(IEnumerable<FrE> a, IEnumerable<FrE> b)
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

            int n = query._polynomial.Length;
            int m = n / 2;
            FrE[] a = query._polynomial;
            FrE[] b = query._pointEvaluations;
            FrE y = InnerProduct(a, b);

            IpaProofStruct ipaProof = new IpaProofStruct(new List<Banderwagon>(), FrE.Zero, new List<Banderwagon>());

            transcript.AppendPoint(query._commitment, "C"u8.ToArray());
            transcript.AppendScalar(query._point, "input point"u8.ToArray());
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

                ipaProof._l.Add(cL);
                ipaProof._r.Add(cR);

                transcript.AppendPoint(cL, "L"u8.ToArray());
                transcript.AppendPoint(cR, "R"u8.ToArray());
                FrE x = transcript.ChallengeScalar("x"u8.ToArray());

                FrE.Inverse(x, out FrE xInv);

                a = new FrE[aL.Length];
                int i = 0;
                foreach ((FrE v1, FrE v2) in aL.Zip(aR))
                {
                    a[i] = v1 + x * v2;
                    i++;
                }

                b = new FrE[aL.Length];
                i = 0;
                foreach ((FrE v1, FrE v2) in bL.Zip(bR))
                {
                    b[i] = v1 + xInv * v2;
                    i++;
                }

                Banderwagon[] currentBasisN = new Banderwagon[m];
                i = 0;
                foreach ((Banderwagon v1, Banderwagon v2) in currentBasis[..m].Zip(currentBasis[m..]))
                {
                    currentBasisN[i] = v1 + v2 * xInv;
                    i++;
                }

                currentBasis = currentBasisN;
                n = m;
                m = n / 2;
            }

            ipaProof._a = a[0];

            return (y, ipaProof);
        }

        public static bool CheckIpaProof(CRS crs, Transcript transcript,
            IpaVerifierQuery query)
        {
            transcript.DomainSep("ipa"u8.ToArray());

            int n = query._pointEvaluations.Length;
            int m = n / 2;


            Banderwagon c = query._commitment;
            FrE z = query._point;
            FrE[] b = query._pointEvaluations;
            IpaProofStruct ipaProof = query._ipaProof;
            FrE y = query._outputPoint;

            transcript.AppendPoint(c, "C"u8.ToArray());
            transcript.AppendScalar(z, "input point"u8.ToArray());
            transcript.AppendScalar(y, "output point"u8.ToArray());
            FrE w = transcript.ChallengeScalar("w"u8.ToArray());

            Banderwagon q = crs.BasisQ * w;

            Banderwagon currentCommitment = c + q * y;

            int i = 0;
            List<FrE> xs = new List<FrE>();
            List<FrE> xInvList = new List<FrE>();


            while (n > 1)
            {
                Banderwagon cL = ipaProof._l[i];
                Banderwagon cR = ipaProof._r[i];

                transcript.AppendPoint(cL, "L"u8.ToArray());
                transcript.AppendPoint(cR, "R"u8.ToArray());
                FrE x = transcript.ChallengeScalar("x"u8.ToArray());

                FrE.Inverse(in x, out FrE xInv);

                xs.Add(x);
                xInvList.Add(xInv);

                currentCommitment = currentCommitment + cL * x + cR * xInv;
                n = m;
                m = n / 2;
                i += 1;
            }

            Banderwagon[] currentBasis = crs.BasisG;

            for (int j = 0; j < xs.Count; j++)
            {
                (Banderwagon[] gL, Banderwagon[] gR) = SplitPoints(currentBasis);
                (FrE[] bL, FrE[] bR) = SplitScalars(b);

                FrE xInv = xInvList[j];

                b = FoldScalars(bL, bR, xInv);
                currentBasis = FoldPoints(gL, gR, xInv);

            }

            if (b.Length != currentBasis.Length)
                throw new Exception();

            if (b.Length != 1)
                throw new Exception();
            FrE b0 = b[0];
            Banderwagon g0 = currentBasis[0];

            Banderwagon gotCommitment = g0 * ipaProof._a + q * (ipaProof._a * b0);

            return currentCommitment == gotCommitment;
        }

        private static (T[] firstHalf, T[] secondHalf) SplitListInHalf<T>(T[] x)
        {
            if (x.Length % 2 != 0)
                throw new Exception();

            int mid = x.Length / 2;
            return (x[..mid], x[mid..]);
        }

        private static (Banderwagon[] firstHalf, Banderwagon[] secondHalf) SplitPoints(Banderwagon[] x)
        {
            return SplitListInHalf(x);
        }

        private static (FrE[] firstHalf, FrE[] secondHalf) SplitScalars(FrE[] x)
        {
            return SplitListInHalf(x);
        }

        private static FrE[] FoldScalars(IReadOnlyList<FrE> a, IReadOnlyList<FrE> b, FrE foldingChallenge)
        {
            if (a.Count != b.Count)
                throw new Exception();

            FrE[] result = new FrE[a.Count];
            for (int i = 0; i < a.Count; i++)
            {
                result[i] = a[i] + b[i] * foldingChallenge;
            }

            return result;
        }

        private static Banderwagon[] FoldPoints(IReadOnlyList<Banderwagon> a, IReadOnlyList<Banderwagon> b, FrE foldingChallenge)
        {
            if (a.Count != b.Count)
                throw new Exception();

            Banderwagon[] result = new Banderwagon[a.Count];
            for (int i = 0; i < a.Count; i++)
            {
                result[i] = a[i] + b[i] * foldingChallenge;
            }

            return result;
        }
    }
}
