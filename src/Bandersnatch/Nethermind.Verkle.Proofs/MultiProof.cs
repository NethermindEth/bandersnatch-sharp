using Nethermind.Field.Montgomery.FrEElement;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Polynomial;

namespace Nethermind.Verkle.Proofs
{
    public class MultiProof
    {
        private readonly CRS _crs;
        private readonly PreComputeWeights _precomp;

        public MultiProof(CRS cRs, int domain)
        {
            _precomp = PreComputeWeights.Init(domain);
            _crs = cRs;
        }

        public VerkleProofStruct MakeMultiProof(Transcript transcript, VerkleProverQuery[] queries)
        {
            int domainSize = _precomp.Domain.Length;
            transcript.DomainSep("multiproof");

            foreach (VerkleProverQuery query in queries)
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

            foreach (VerkleProverQuery query in queries)
            {
                LagrangeBasis f = query.f;
                FrE index = query.z;
                FrE[] quotient = Quotient.ComputeQuotientInsideDomain(_precomp, f, index);
                for (int i = 0; i < domainSize; i++)
                {
                    g[i] += powerOfR * quotient[i];
                }

                powerOfR *= r;
            }

            Banderwagon D = _crs.Commit(g);
            transcript.AppendPoint(D, "D");
            FrE t = transcript.ChallengeScalar("t");

            FrE[] h = new FrE[domainSize];
            for (int i = 0; i < domainSize; i++)
            {
                h[i] = FrE.Zero;
            }

            powerOfR = FrE.One;

            foreach (VerkleProverQuery query in queries)
            {
                LagrangeBasis f = query.f;
                int index = (int)query.z.u0;
                FrE.Inverse(t - _precomp.Domain[index], out FrE denominatorInv);

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

            Banderwagon E = _crs.Commit(h);
            transcript.AppendPoint(E, "E");

            Banderwagon ipaCommitment = E - D;
            FrE[] inputPointVector = _precomp.BarycentricFormulaConstants(t);
            IpaProverQuery pQuery = new IpaProverQuery(hMinusG, ipaCommitment,
                t, inputPointVector);

            (FrE outputPoint, IpaProofStruct ipaProof) = IPA.MakeIpaProof(_crs, transcript, pQuery);

            return new VerkleProofStruct(ipaProof, D);

        }

        public bool CheckMultiProof(Transcript transcript, VerkleVerifierQuery[] queries, VerkleProofStruct proof)
        {
            transcript.DomainSep("multiproof");
            Banderwagon D = proof.D;
            IpaProofStruct ipaIpaProof = proof._ipaProof;
            foreach (VerkleVerifierQuery query in queries)
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

            foreach (VerkleVerifierQuery query in queries)
            {
                Banderwagon C = query.C;
                int z = (int)query.z.u0;
                FrE y = query.y;
                FrE eCoefficient = powerOfR / t - _precomp.Domain[z];
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
            FrE[] inputPointVector = _precomp.BarycentricFormulaConstants(t);

            IpaVerifierQuery queryX = new IpaVerifierQuery(ipaCommitment, t,
                inputPointVector, yO, ipaIpaProof);

            return IPA.CheckIpaProof(_crs, transcript, queryX);

        }

        public static Banderwagon VarBaseCommit(FrE[] values, Banderwagon[] elements)
        {
            return Banderwagon.MSM(elements, values);
        }
    }
}
