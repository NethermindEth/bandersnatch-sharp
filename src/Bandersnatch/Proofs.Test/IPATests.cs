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
            domain[i] = new Fr((UInt256) i);
        }

        var weights = PreComputeWeights.Init(domain);

        List<Fr> lagrangePoly = new();

        for (int i = 0; i < 8; i++)
        {
            lagrangePoly.AddRange(_poly);
        }

        var crs = CRS.Default();
        var commitment = crs.Commit(lagrangePoly.ToArray());

        Assert.IsTrue(Convert.ToHexString(commitment.ToBytes()).ToLower()
            .SequenceEqual("1b9dff8f5ebbac250d291dfe90e36283a227c64b113c37f1bfb9e7a743cdb128"));

        var proverTranscript = new Transcript("test");

        var inputPoint = new Fr((UInt256) 2101);
        var b = weights.BarycentricFormulaConstants(inputPoint);
        var query = new ProverQuery(lagrangePoly.ToArray(), commitment, inputPoint, b);

        byte[] hash =
        {
            59, 242, 0, 139, 181, 46, 10, 203, 105, 140, 230, 43, 108, 173, 120, 136, 17, 42, 116, 137, 73, 212, 87,
            150, 5, 145, 25, 202, 179, 251, 7, 191
        };
        List<byte> cache = new();
        foreach (var i in lagrangePoly)
        {
            cache.AddRange(i.ToBytes());
        }
        cache.AddRange(commitment.ToBytes());
        cache.AddRange(inputPoint.ToBytes());
        foreach (var i in b)
        {
            cache.AddRange(i.ToBytes());
        }

        var (outputPoint, proof) = IPA.MakeIpaProof(crs, proverTranscript, query);
        var pChallenge = proverTranscript.ChallengeScalar("state");

        Assert.IsTrue(Convert.ToHexString(pChallenge.ToBytes()).ToLower()
            .SequenceEqual("0a81881cbfd7d7197a54ebd67ed6a68b5867f3c783706675b34ece43e85e7306"));

        var verifierTranscript = new Transcript("test");

        var queryX = new VerifierQuery(commitment, inputPoint, b, outputPoint, proof);

        var ok = IPA.CheckIpaProof(crs, verifierTranscript, queryX);
        
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

        var expectedResult = new Fr((UInt256)204);

        var gotResult = IPA.InnerProduct(a, b);
        Assert.IsTrue(gotResult == expectedResult);
    }
}