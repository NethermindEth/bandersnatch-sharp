using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Fields.FrEElement;

namespace Nethermind.Verkle.Proofs
{
    public static class Ipa
    {
        public static FrE InnerProduct(Span<FrE> a, Span<FrE> b)
        {
            FrE res = FrE.Zero;
            for (int i = 0; i < a.Length; i++)
            {
                res += a[i] * b[i];
            }
            return res;
        }

        public static IpaProofStruct MakeIpaProof(CRS crs, Transcript transcript, IpaProverQuery query, out FrE y)
        {
            transcript.DomainSep("ipa");

            int n = query.Polynomial.Length;
            int m = n / 2;
            Span<FrE> a = query.Polynomial;
            Span<FrE> b = query.PointEvaluations;
            y = InnerProduct(a, b);

            int numRounds = (int)Math.Log2(n);
            Banderwagon[] l = new Banderwagon[numRounds];
            Banderwagon[] r = new Banderwagon[numRounds];

            transcript.AppendPoint(query.Commitment, "C"u8.ToArray());
            transcript.AppendScalar(query.Point, "input point"u8.ToArray());
            transcript.AppendScalar(y, "output point"u8.ToArray());
            FrE w = transcript.ChallengeScalar("w"u8.ToArray());

            Banderwagon q = crs.BasisQ * w;

            Span<Banderwagon> currentBasis = crs.BasisG;

            for (int round = 0; round < numRounds; round++)
            {
                Span<FrE> aL = a[..m];
                Span<FrE> aR = a[m..];
                Span<FrE> bL = b[..m];
                Span<FrE> bR = b[m..];
                Span<Banderwagon> gL = currentBasis[..m];
                Span<Banderwagon> gR = currentBasis[m..];

                FrE zL = InnerProduct(aR, bL);
                FrE zR = InnerProduct(aL, bR);

                Banderwagon cL = Banderwagon.MultiScalarMul(gL, aR) + q * zL;
                Banderwagon cR = Banderwagon.MultiScalarMul(gR, aL) + q * zR;

                l[round] = cL;
                r[round] = cR;

                transcript.AppendPoint(cL, "L"u8.ToArray());
                transcript.AppendPoint(cR, "R"u8.ToArray());
                FrE x = transcript.ChallengeScalar("x"u8.ToArray());

                FrE.Inverse(x, out FrE xInv);

                a = new FrE[m];
                for (int i = 0; i < m; i++)
                {
                    a[i] = aL[i] + x * aR[i];
                }

                b = new FrE[m];
                for (int i = 0; i < m; i++)
                {
                    b[i] = bL[i] + xInv * bR[i];
                }


                currentBasis = new Banderwagon[m];
                for (int i = 0; i < m; i++)
                {
                    currentBasis[i] = gL[i] + gR[i] * xInv;
                }

                n = m;
                m = n / 2;
            }

            return new IpaProofStruct(l, a[0], r);
        }

        public static bool CheckIpaProof(CRS crs, Transcript transcript,
            IpaVerifierQuery query)
        {
            transcript.DomainSep("ipa"u8.ToArray());

            int n = query.PointEvaluations.Length;
            int m = n / 2;

            int numRounds = query.IpaProof.L.Length;


            Banderwagon c = query.Commitment;
            FrE z = query.Point;
            Span<FrE> b = query.PointEvaluations;
            IpaProofStruct ipaProof = query.IpaProof;
            FrE y = query.OutputPoint;

            transcript.AppendPoint(c, "C"u8.ToArray());
            transcript.AppendScalar(z, "input point"u8.ToArray());
            transcript.AppendScalar(y, "output point"u8.ToArray());
            FrE w = transcript.ChallengeScalar("w"u8.ToArray());

            Banderwagon q = crs.BasisQ * w;

            Banderwagon currentCommitment = c + q * y;

            FrE[] xs = new FrE[numRounds];
            FrE[] xInvList = new FrE[numRounds];


            for (int i = 0; i < numRounds; i++)
            {
                Banderwagon cL = ipaProof.L[i];
                Banderwagon cR = ipaProof.R[i];

                transcript.AppendPoint(cL, "L"u8.ToArray());
                transcript.AppendPoint(cR, "R"u8.ToArray());
                FrE x = transcript.ChallengeScalar("x"u8.ToArray());

                FrE.Inverse(in x, out FrE xInv);

                xs[i] = x;
                xInvList[i] = xInv;

                currentCommitment = currentCommitment + cL * x + cR * xInv;
                n = m;
                m = n / 2;
            }

            Span<Banderwagon> currentBasis = crs.BasisG;
            n = crs.BasisG.Length;
            m = n / 2;

            for (int j = 0; j < numRounds; j++)
            {
                Span<Banderwagon> gL = currentBasis[..m];
                Span<Banderwagon> gR = currentBasis[m..];

                Span<FrE> bL = b[..m];
                Span<FrE> bR = b[m..];

                FrE xInv = xInvList[j];

                b = FoldScalars(bL, bR, xInv);
                currentBasis = FoldPoints(gL, gR, xInv);
                n = m;
                m = n / 2;
            }

            if (b.Length != currentBasis.Length) throw new Exception();
            if (b.Length != 1)  throw new Exception();

            FrE b0 = b[0];
            Banderwagon g0 = currentBasis[0];
            Banderwagon gotCommitment = g0 * ipaProof.A + q * (ipaProof.A * b0);

            return currentCommitment == gotCommitment;
        }

        private static FrE[] FoldScalars(Span<FrE> a, Span<FrE> b, in FrE foldingChallenge)
        {
            if (a.Length != b.Length) throw new Exception();

            FrE[] result = new FrE[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                result[i] = a[i] + b[i] * foldingChallenge;
            }

            return result;
        }

        private static Banderwagon[] FoldPoints(Span<Banderwagon> a, Span<Banderwagon> b, in FrE foldingChallenge)
        {
            if (a.Length != b.Length) throw new Exception();

            Banderwagon[] result = new Banderwagon[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                result[i] = a[i] + b[i] * foldingChallenge;
            }

            return result;
        }
    }
}
