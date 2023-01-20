using System;
using System.Collections.Generic;
using System.Linq;
using Nethermind.Field.Montgomery.FrEElement;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Polynomial;
using NUnit.Framework;

namespace Nethermind.Verkle.Proofs.Test
{
    public class MultiProofTests
    {
        private readonly FrE[] _poly =
        {
            FrE.SetElement(1), FrE.SetElement(2), FrE.SetElement(3), FrE.SetElement(4), FrE.SetElement(5), FrE.SetElement(6), FrE.SetElement(7), FrE.SetElement(8), FrE.SetElement(9), FrE.SetElement(10), FrE.SetElement(11), FrE.SetElement(12), FrE.SetElement(13),
            FrE.SetElement(14), FrE.SetElement(15), FrE.SetElement(16), FrE.SetElement(17), FrE.SetElement(18), FrE.SetElement(19), FrE.SetElement(20), FrE.SetElement(21), FrE.SetElement(22), FrE.SetElement(23), FrE.SetElement(24), FrE.SetElement(25), FrE.SetElement(26),
            FrE.SetElement(27), FrE.SetElement(28), FrE.SetElement(29), FrE.SetElement(30), FrE.SetElement(31), FrE.SetElement(32)
        };

        [Test]
        public void TestBasicMultiProof()
        {
            List<FrE> polyEvalA = new List<FrE>();
            List<FrE> polyEvalB = new List<FrE>();

            for (int i = 0; i < 8; i++)
            {
                polyEvalA.AddRange(_poly);
                polyEvalB.AddRange(_poly.Reverse());
            }
            CRS crs = CRS.Instance;
            Banderwagon cA = crs.Commit(polyEvalA.ToArray());
            Banderwagon cB = crs.Commit(polyEvalB.ToArray());

            FrE[] zs =
            {
                FrE.Zero, FrE.Zero
            };
            FrE[] ys =
            {
                FrE.SetElement(1), FrE.SetElement(32)
            };
            FrE[][] fs =
            {
                polyEvalA.ToArray(), polyEvalB.ToArray()
            };
            ;
            Banderwagon[] cs =
            {
                cA, cB
            };

            FrE[] domain = new FrE[256];
            for (int i = 0; i < 256; i++)
            {
                domain[i] = FrE.SetElement(i);
            }

            MultiProofProverQuery queryA = new MultiProofProverQuery(new LagrangeBasis(fs[0], domain), cs[0], zs[0], ys[0]);
            MultiProofProverQuery queryB = new MultiProofProverQuery(new LagrangeBasis(fs[1], domain), cs[1], zs[1], ys[1]);

            MultiProof multiproof = new MultiProof(domain, crs);

            Transcript proverTranscript = new Transcript("test");
            MultiProofProverQuery[] queries =
            {
                queryA, queryB
            };
            MultiProofStruct proof = multiproof.MakeMultiProof(proverTranscript, queries);
            FrE pChallenge = proverTranscript.ChallengeScalar("state");

            Assert.IsTrue(Convert.ToHexString(pChallenge.ToBytes()).ToLower()
                .SequenceEqual("eee8a80357ff74b766eba39db90797d022e8d6dee426ded71234241be504d519"));

            Transcript verifierTranscript = new Transcript("test");
            MultiProofVerifierQuery queryAx = new MultiProofVerifierQuery(cs[0], zs[0], ys[0]);
            MultiProofVerifierQuery queryBx = new MultiProofVerifierQuery(cs[1], zs[1], ys[1]);

            MultiProofVerifierQuery[] queriesX =
            {
                queryAx, queryBx
            };
            bool ok = multiproof.CheckMultiProof(verifierTranscript, queriesX, proof);
            Assert.IsTrue(ok);

            FrE vChallenge = verifierTranscript.ChallengeScalar("state");
            Assert.IsTrue(vChallenge.Equals(pChallenge));
        }
    }
}
