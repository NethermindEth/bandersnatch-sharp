using System;
using System.Linq;
using Nethermind.Field.Montgomery;
using Nethermind.Verkle.Curve;
using NUnit.Framework;

namespace Nethermind.Verkle.Proofs.Test;


public class TranscriptTests
{
    [Test]
    public void test_prover_verifier_consistency()
    {
        Banderwagon point = Banderwagon.Generator();
        Random random = new Random();
        byte[] data = new byte[32];
        random.NextBytes(data);
        FrE scalar = new FrE(data);

        Transcript proverTranscript = new Transcript("protocol_name");

        proverTranscript.AppendPoint(point, "D");
        proverTranscript.DomainSep("sub_protocol_name");
        proverTranscript.AppendScalar(scalar, "r");

        FrE proverQ = proverTranscript.ChallengeScalar("q");

        Transcript verifierTranscript = new Transcript("protocol_name");

        verifierTranscript.AppendPoint(point, "D");
        verifierTranscript.DomainSep("sub_protocol_name");
        verifierTranscript.AppendScalar(scalar, "r");

        FrE verifierQ = verifierTranscript.ChallengeScalar("q");

        Assert.IsTrue(proverQ.Equals(verifierQ));
    }

    [Test]
    public void test_vector_0()
    {
        Transcript transcript = new Transcript("foo");
        FrE firstChallenge = transcript.ChallengeScalar("f");
        FrE secondChallenge = transcript.ChallengeScalar("f");
        Assert.IsTrue(!firstChallenge.Equals(secondChallenge));
    }

    [Test]
    public void test_vector_1()
    {
        Transcript transcript = new Transcript("simple_protocol");
        FrE challenge = transcript.ChallengeScalar("simple_challenge");
        Assert.IsTrue(Convert.ToHexString(challenge.ToBytes()).ToLower()
            .SequenceEqual("c2aa02607cbdf5595f00ee0dd94a2bbff0bed6a2bf8452ada9011eadb538d003"));
    }

    [Test]
    public void test_vector_2()
    {
        Transcript transcript = new Transcript("simple_protocol");
        FrE scalar = FrE.SetElement(5);

        transcript.AppendScalar(scalar, "five");
        transcript.AppendScalar(scalar, "five again");

        FrE challenge = transcript.ChallengeScalar("simple_challenge");
        Assert.IsTrue(Convert.ToHexString(challenge.ToBytes()).ToLower()
            .SequenceEqual("498732b694a8ae1622d4a9347535be589e4aee6999ffc0181d13fe9e4d037b0b"));
    }

    [Test]
    public void test_vector_3()
    {
        Transcript transcript = new Transcript("simple_protocol");
        FrE minusOne = FrE.SetElement(-1);
        FrE one = FrE.SetElement(1);
        transcript.AppendScalar(minusOne, "-1");
        transcript.DomainSep("separate me");
        transcript.AppendScalar(minusOne, "-1 again");
        transcript.DomainSep("separate me again");
        transcript.AppendScalar(one, "now 1");

        FrE challenge = transcript.ChallengeScalar("simple_challenge");
        Assert.IsTrue(Convert.ToHexString(challenge.ToBytes()).ToLower()
            .SequenceEqual("14f59938e9e9b1389e74311a464f45d3d88d8ac96adf1c1129ac466de088d618"));
    }

    [Test]
    public void test_vector_4()
    {
        Transcript transcript = new Transcript("simple_protocol");

        Banderwagon generator = Banderwagon.Generator();

        transcript.AppendPoint(generator, "generator");
        FrE challenge = transcript.ChallengeScalar("simple_challenge");

        Assert.IsTrue(Convert.ToHexString(challenge.ToBytes()).ToLower()
            .SequenceEqual("8c2dafe7c0aabfa9ed542bb2cbf0568399ae794fc44fdfd7dff6cc0e6144921c"));
    }
}
