using System;
using System.Collections.Generic;
using System.Linq;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Polynomial;
using NUnit.Framework;
using Nethermind.Field.Montgomery.FrEElement;

namespace Nethermind.Verkle.Proofs.Test;


public class IPATests
{
    private readonly FrE[] _poly =
    {
        FrE.SetElement( 1),
        FrE.SetElement( 2),
        FrE.SetElement( 3),
        FrE.SetElement( 4),
        FrE.SetElement( 5),
        FrE.SetElement( 6),
        FrE.SetElement( 7),
        FrE.SetElement( 8),
        FrE.SetElement( 9),
        FrE.SetElement( 10),
        FrE.SetElement( 11),
        FrE.SetElement( 12),
        FrE.SetElement( 13),
        FrE.SetElement( 14),
        FrE.SetElement( 15),
        FrE.SetElement( 16),
        FrE.SetElement( 17),
        FrE.SetElement( 18),
        FrE.SetElement( 19),
        FrE.SetElement( 20),
        FrE.SetElement( 21),
        FrE.SetElement( 22),
        FrE.SetElement( 23),
        FrE.SetElement( 24),
        FrE.SetElement( 25),
        FrE.SetElement( 26),
        FrE.SetElement( 27),
        FrE.SetElement( 28),
        FrE.SetElement( 29),
        FrE.SetElement( 30),
        FrE.SetElement( 31),
        FrE.SetElement( 32),
    };


    [Test]
    public void TestBasicIpaProof()
    {
        FrE[] domain = new FrE[256];
        for (int i = 0; i < 256; i++)
        {
            domain[i] = FrE.SetElement(i);
        }

        PreComputeWeights weights = PreComputeWeights.Init(domain);

        List<FrE> lagrangePoly = new();

        for (int i = 0; i < 8; i++)
        {
            lagrangePoly.AddRange(_poly);
        }

        CRS crs = CRS.Default();
        Banderwagon commitment = crs.Commit(lagrangePoly.ToArray());

        Assert.IsTrue(Convert.ToHexString(commitment.ToBytes()).ToLower()
            .SequenceEqual("1b9dff8f5ebbac250d291dfe90e36283a227c64b113c37f1bfb9e7a743cdb128"));

        Transcript proverTranscript = new Transcript("test");

        FrE inputPoint = FrE.SetElement(2101);
        FrE[] b = weights.BarycentricFormulaConstants(inputPoint);
        ProverQuery query = new ProverQuery(lagrangePoly.ToArray(), commitment, inputPoint, b);

        byte[] hash =
        {
            59, 242, 0, 139, 181, 46, 10, 203, 105, 140, 230, 43, 108, 173, 120, 136, 17, 42, 116, 137, 73, 212, 87,
            150, 5, 145, 25, 202, 179, 251, 7, 191
        };
        List<byte> cache = new();
        foreach (FrE i in lagrangePoly)
        {
            cache.AddRange(i.ToBytes().ToArray());
        }
        cache.AddRange(commitment.ToBytes());
        cache.AddRange(inputPoint.ToBytes().ToArray());
        foreach (FrE i in b)
        {
            cache.AddRange(i.ToBytes().ToArray());
        }

        (FrE outputPoint, ProofStruct proof) = IPA.MakeIpaProof(crs, proverTranscript, query);
        FrE pChallenge = proverTranscript.ChallengeScalar("state");

        Assert.IsTrue(Convert.ToHexString(pChallenge.ToBytes()).ToLower()
            .SequenceEqual("0a81881cbfd7d7197a54ebd67ed6a68b5867f3c783706675b34ece43e85e7306"));

        Transcript verifierTranscript = new Transcript("test");

        VerifierQuery queryX = new VerifierQuery(commitment, inputPoint, b, outputPoint, proof);

        bool ok = IPA.CheckIpaProof(crs, verifierTranscript, queryX);

        Assert.IsTrue(ok);
    }

    [Test]
    public void TestInnerProduct()
    {
        FrE[] a =
        {
            FrE.SetElement( 1),
            FrE.SetElement( 2),
            FrE.SetElement( 3),
            FrE.SetElement( 4),
            FrE.SetElement( 5),
        };

        FrE[] b =
        {
            FrE.SetElement( 10),
            FrE.SetElement( 12),
            FrE.SetElement( 13),
            FrE.SetElement( 14),
            FrE.SetElement( 15),
        };

        FrE expectedResult = FrE.SetElement(204);

        FrE gotResult = IPA.InnerProduct(a, b);
        Assert.IsTrue(gotResult.Equals(expectedResult));
    }
}
