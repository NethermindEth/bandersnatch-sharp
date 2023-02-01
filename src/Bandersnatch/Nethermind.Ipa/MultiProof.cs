using Nethermind.Field.Montgomery.FrEElement;
using Nethermind.Polynomial;
using Nethermind.Verkle.Curve;

namespace Nethermind.Ipa
{
    public class MultiProof
    {
        private readonly CRS crs;
        private readonly PreComputeWeights precomp;

        public MultiProof(FrE[] domain, CRS cRS)
        {
            precomp = PreComputeWeights.Init(domain);
            crs = cRS;
        }

        public MultiProofStruct MakeMultiProof(Transcript transcript, MultiProofProverQuery[] queries)
        {
            int domainSize = precomp.Domain.Length;
            transcript.DomainSep("multiproof");

            foreach (MultiProofProverQuery query in queries)
            {
                transcript.AppendPoint(query.C, "C");
                transcript.AppendScalar(query.z, "z");
                transcript.AppendScalar(query.y, "y");
            }

            FrE r = transcript.ChallengeScalar("r");
            FrE[] g = new FrE[domainSize];
            for (int i = 0; i < domainSize; i++)
            {
                g[i] = FrE.Zero;
            }

            FrE powerOfR = FrE.One;

            foreach (MultiProofProverQuery query in queries)
            {
                LagrangeBasis f = query.f;
                FrE index = query.z;
                FrE[] quotient = Quotient.ComputeQuotientInsideDomain(precomp, f, index);
                for (int i = 0; i < domainSize; i++)
                {
                    g[i] += powerOfR * quotient[i];
                }

                powerOfR *= r;
            }

            Banderwagon D = crs.Commit(g);
            transcript.AppendPoint(D, "D");
            FrE t = transcript.ChallengeScalar("t");

            FrE[] h = new FrE[domainSize];
            for (int i = 0; i < domainSize; i++)
            {
                h[i] = FrE.Zero;
            }

            powerOfR = FrE.One;

            foreach (MultiProofProverQuery query in queries)
            {
                LagrangeBasis f = query.f;
                int index = (int)query.z.u0;
                FrE.Inverse(t - precomp.Domain[index], out FrE denominatorInv);

                for (int i = 0; i < domainSize; i++)
                {
                    h[i] += powerOfR * f.Evaluations[i] * denominatorInv;
                }

                powerOfR = powerOfR * r;
            }

            FrE[] hMinusG = new FrE[domainSize];
            for (int i = 0; i < domainSize; i++)
            {
                hMinusG[i] = h[i] - g[i];
            }

            Banderwagon E = crs.Commit(h);
            transcript.AppendPoint(E, "E");

            Banderwagon ipaCommitment = E - D;
            FrE[] inputPointVector = precomp.BarycentricFormulaConstants(t);
            ProverQuery pQuery = new ProverQuery(hMinusG, ipaCommitment,
                t, inputPointVector);

            (FrE outputPoint, ProofStruct ipaProof) = IPA.MakeIpaProof(crs, transcript, pQuery);

            return new MultiProofStruct(ipaProof, D);

        }

        public bool CheckMultiProof(Transcript transcript, MultiProofVerifierQuery[] queries, MultiProofStruct proof)
        {
            transcript.DomainSep("multiproof");
            Banderwagon D = proof.D;
            ProofStruct ipaProof = proof.IpaProof;
            foreach (MultiProofVerifierQuery query in queries)
            {
                transcript.AppendPoint(query.C, "C");
                transcript.AppendScalar(query.z, "z");
                transcript.AppendScalar(query.y, "y");
            }

            FrE r = transcript.ChallengeScalar("r");

            transcript.AppendPoint(D, "D");
            FrE t = transcript.ChallengeScalar("t");

            Dictionary<byte[], FrE> eCoefficients = new Dictionary<byte[], FrE>();
            FrE g2OfT = FrE.Zero;
            FrE powerOfR = FrE.One;

            Dictionary<byte[], Banderwagon> cBySerialized = new Dictionary<byte[], Banderwagon>();

            foreach (MultiProofVerifierQuery query in queries)
            {
                Banderwagon C = query.C;
                int z = (int)query.z.u0;
                FrE y = query.y;
                FrE eCoefficient = powerOfR / t - precomp.Domain[z];
                byte[] cSerialized = C.ToBytes();
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

            Banderwagon E = VarBaseCommit(eCoefficients.Values.ToArray(), elems);
            transcript.AppendPoint(E, "E");

            FrE yO = g2OfT;
            Banderwagon ipaCommitment = E - D;
            FrE[] inputPointVector = precomp.BarycentricFormulaConstants(t);

            VerifierQuery queryX = new VerifierQuery(ipaCommitment, t,
                inputPointVector, yO, ipaProof);

            return IPA.CheckIpaProof(crs, transcript, queryX);

        }

        public static Banderwagon VarBaseCommit(FrE[] values, Banderwagon[] elements)
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
        public FrE z;
        public FrE y;

        public MultiProofProverQuery(LagrangeBasis _f, Banderwagon _C, FrE _z, FrE _y)
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
        public FrE z;
        public FrE y;

        public MultiProofVerifierQuery(Banderwagon _C, FrE _z, FrE _y)
        {
            C = _C;
            z = _z;
            y = _y;
        }
    }
}
