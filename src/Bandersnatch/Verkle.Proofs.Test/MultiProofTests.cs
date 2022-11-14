using System;
using System.Collections.Generic;
using System.Linq;
using Curve;
using Field;
using Nethermind.Int256;
using NUnit.Framework;
using Polynomial;

namespace Proofs.Test;
using Fp = FixedFiniteField<BandersnatchBaseFieldStruct>;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public class MultiProofTests
{
    private readonly Fr[] _poly =
    {
        new Fr((UInt256) 1),
        new Fr((UInt256) 2),
        new Fr((UInt256) 3),
        new Fr((UInt256) 4),
        new Fr((UInt256) 5),
        new Fr((UInt256) 6),
        new Fr((UInt256) 7),
        new Fr((UInt256) 8),
        new Fr((UInt256) 9),
        new Fr((UInt256) 10),
        new Fr((UInt256) 11),
        new Fr((UInt256) 12),
        new Fr((UInt256) 13),
        new Fr((UInt256) 14),
        new Fr((UInt256) 15),
        new Fr((UInt256) 16),
        new Fr((UInt256) 17),
        new Fr((UInt256) 18),
        new Fr((UInt256) 19),
        new Fr((UInt256) 20),
        new Fr((UInt256) 21),
        new Fr((UInt256) 22),
        new Fr((UInt256) 23),
        new Fr((UInt256) 24),
        new Fr((UInt256) 25),
        new Fr((UInt256) 26),
        new Fr((UInt256) 27),
        new Fr((UInt256) 28),
        new Fr((UInt256) 29),
        new Fr((UInt256) 30),
        new Fr((UInt256) 31),
        new Fr((UInt256) 32),
    };

    [Test]
    public void TestBasicMultiProof()
    {
        List<Fr> polyEvalA = new();
        List<Fr> polyEvalB = new();

        for (int i = 0; i < 8; i++)
        {
            polyEvalA.AddRange(_poly);
            polyEvalB.AddRange(_poly.Reverse());
        }
        CRS? crs = CRS.Default();
        Banderwagon? cA = crs.Commit(polyEvalA.ToArray());
        Banderwagon? cB = crs.Commit(polyEvalB.ToArray());

        Fr[] zs =
        {
            Fr.Zero,
            Fr.Zero
        };
        Fr[] ys = { new Fr((UInt256)1), new Fr((UInt256)32) };
        Fr[][] fs =
        {
            polyEvalA.ToArray(), polyEvalB.ToArray()
        };
        ;
        Banderwagon[] cs =
        {
            cA, cB
        };

        Fr[] domain = new Fr[256];
        for (int i = 0; i < 256; i++)
        {
            domain[i] = new Fr((UInt256)i);
        }

        MultiProofProverQuery queryA = new MultiProofProverQuery(new LagrangeBasis(fs[0], domain), cs[0], zs[0], ys[0]);
        MultiProofProverQuery queryB = new MultiProofProverQuery(new LagrangeBasis(fs[1], domain), cs[1], zs[1], ys[1]);

        MultiProof? multiproof = new MultiProof(domain, crs);

        Transcript? proverTranscript = new Transcript("test");
        MultiProofProverQuery[] queries =
        {
            queryA, queryB
        };
        MultiProofStruct proof = multiproof.MakeMultiProof(proverTranscript, queries);
        Fr? pChallenge = proverTranscript.ChallengeScalar("state");

        Assert.IsTrue(Convert.ToHexString(pChallenge.ToBytes()).ToLower()
            .SequenceEqual("eee8a80357ff74b766eba39db90797d022e8d6dee426ded71234241be504d519"));

        Transcript? verifierTranscript = new Transcript("test");
        MultiProofVerifierQuery queryAx = new MultiProofVerifierQuery(cs[0], zs[0], ys[0]);
        MultiProofVerifierQuery queryBx = new MultiProofVerifierQuery(cs[1], zs[1], ys[1]);

        MultiProofVerifierQuery[] queriesX =
        {
            queryAx, queryBx
        };
        bool ok = multiproof.CheckMultiProof(verifierTranscript, queriesX, proof);
        Assert.IsTrue(ok);

        Fr? vChallenge = verifierTranscript.ChallengeScalar("state");
        Assert.IsTrue(vChallenge == pChallenge);
    }
}
