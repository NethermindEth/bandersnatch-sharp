using System;
using System.Collections.Generic;
using System.Linq;
using Nethermind.Field;
using Nethermind.Int256;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Polynomial;
using NUnit.Framework;

namespace Nethermind.Verkle.Proofs.Test;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public class IPATests
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
    public void TestBasicIpaProof()
    {
        Fr[] domain = new Fr[256];
        for (int i = 0; i < 256; i++)
        {
            domain[i] = new Fr((UInt256)i);
        }

        PreComputeWeights? weights = PreComputeWeights.Init(domain);

        List<Fr> lagrangePoly = new();

        for (int i = 0; i < 8; i++)
        {
            lagrangePoly.AddRange(_poly);
        }

        CRS? crs = CRS.Default();
        Banderwagon? commitment = crs.Commit(lagrangePoly.ToArray());

        Assert.IsTrue(Convert.ToHexString(commitment.ToBytes()).ToLower()
            .SequenceEqual("1b9dff8f5ebbac250d291dfe90e36283a227c64b113c37f1bfb9e7a743cdb128"));

        Transcript? proverTranscript = new Transcript("test");

        Fr? inputPoint = new Fr((UInt256)2101);
        Fr[]? b = weights.BarycentricFormulaConstants(inputPoint);
        ProverQuery query = new ProverQuery(lagrangePoly.ToArray(), commitment, inputPoint, b);

        byte[] hash =
        {
            59, 242, 0, 139, 181, 46, 10, 203, 105, 140, 230, 43, 108, 173, 120, 136, 17, 42, 116, 137, 73, 212, 87,
            150, 5, 145, 25, 202, 179, 251, 7, 191
        };
        List<byte> cache = new();
        foreach (Fr? i in lagrangePoly)
        {
            cache.AddRange(i.ToBytes());
        }
        cache.AddRange(commitment.ToBytes());
        cache.AddRange(inputPoint.ToBytes());
        foreach (Fr? i in b)
        {
            cache.AddRange(i.ToBytes());
        }

        (Fr? outputPoint, ProofStruct proof) = IPA.MakeIpaProof(crs, proverTranscript, query);
        Fr? pChallenge = proverTranscript.ChallengeScalar("state");

        Assert.IsTrue(Convert.ToHexString(pChallenge.ToBytes()).ToLower()
            .SequenceEqual("0a81881cbfd7d7197a54ebd67ed6a68b5867f3c783706675b34ece43e85e7306"));

        Transcript? verifierTranscript = new Transcript("test");

        VerifierQuery queryX = new VerifierQuery(commitment, inputPoint, b, outputPoint, proof);

        bool ok = IPA.CheckIpaProof(crs, verifierTranscript, queryX);

        Assert.IsTrue(ok);
    }

    [Test]
    public void TestInnerProduct()
    {
        Fr[] a =
        {
            new Fr((UInt256) 1),
            new Fr((UInt256) 2),
            new Fr((UInt256) 3),
            new Fr((UInt256) 4),
            new Fr((UInt256) 5),
        };

        Fr[] b =
        {
            new Fr((UInt256) 10),
            new Fr((UInt256) 12),
            new Fr((UInt256) 13),
            new Fr((UInt256) 14),
            new Fr((UInt256) 15),
        };

        Fr? expectedResult = new Fr((UInt256)204);

        Fr? gotResult = IPA.InnerProduct(a, b);
        Assert.IsTrue(gotResult == expectedResult);
    }
}
