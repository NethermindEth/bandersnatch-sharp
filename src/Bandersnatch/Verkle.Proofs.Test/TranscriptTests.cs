using System;
using System.Linq;
using Curve;
using Field;
using Nethermind.Int256;
using NUnit.Framework;

namespace Proofs.Test;
using Fp = FixedFiniteField<BandersnatchBaseFieldStruct>;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public class TranscriptTests
{
    [Test]
    public void test_prover_verifier_consistency()
    {
        Banderwagon? point = Banderwagon.Generator();
        Random random = new Random();
        byte[] data = new byte[32];
        random.NextBytes(data);
        Fr? scalar = new Fr(new UInt256(data));

        Transcript? proverTranscript = new Transcript("protocol_name");

        proverTranscript.AppendPoint(point, "D");
        proverTranscript.DomainSep("sub_protocol_name");
        proverTranscript.AppendScalar(scalar, "r");

        Fr? proverQ = proverTranscript.ChallengeScalar("q");

        Transcript? verifierTranscript = new Transcript("protocol_name");

        verifierTranscript.AppendPoint(point, "D");
        verifierTranscript.DomainSep("sub_protocol_name");
        verifierTranscript.AppendScalar(scalar, "r");

        Fr? verifierQ = verifierTranscript.ChallengeScalar("q");

        Assert.IsTrue(proverQ == verifierQ);
    }

    [Test]
    public void test_vector_0()
    {
        Transcript? transcript = new Transcript("foo");
        Fr? firstChallenge = transcript.ChallengeScalar("f");
        Fr? secondChallenge = transcript.ChallengeScalar("f");
        Assert.IsTrue(firstChallenge != secondChallenge);
    }

    [Test]
    public void test_vector_1()
    {
        Transcript? transcript = new Transcript("simple_protocol");
        Fr? challenge = transcript.ChallengeScalar("simple_challenge");
        Assert.IsTrue(Convert.ToHexString(challenge.ToBytes()).ToLower()
            .SequenceEqual("c2aa02607cbdf5595f00ee0dd94a2bbff0bed6a2bf8452ada9011eadb538d003"));
    }

    [Test]
    public void test_vector_2()
    {
        Transcript? transcript = new Transcript("simple_protocol");
        Fr? scalar = new Fr((UInt256)5);

        transcript.AppendScalar(scalar, "five");
        transcript.AppendScalar(scalar, "five again");

        Fr? challenge = transcript.ChallengeScalar("simple_challenge");
        Assert.IsTrue(Convert.ToHexString(challenge.ToBytes()).ToLower()
            .SequenceEqual("498732b694a8ae1622d4a9347535be589e4aee6999ffc0181d13fe9e4d037b0b"));
    }

    [Test]
    public void test_vector_3()
    {
        Transcript? transcript = new Transcript("simple_protocol");
        Fr? minusOne = new Fr(-1);
        Fr? one = new Fr((UInt256)1);
        transcript.AppendScalar(minusOne, "-1");
        transcript.DomainSep("separate me");
        transcript.AppendScalar(minusOne, "-1 again");
        transcript.DomainSep("separate me again");
        transcript.AppendScalar(one, "now 1");

        Fr? challenge = transcript.ChallengeScalar("simple_challenge");
        Assert.IsTrue(Convert.ToHexString(challenge.ToBytes()).ToLower()
            .SequenceEqual("14f59938e9e9b1389e74311a464f45d3d88d8ac96adf1c1129ac466de088d618"));
    }

    [Test]
    public void test_vector_4()
    {
        Transcript? transcript = new Transcript("simple_protocol");

        Banderwagon? generator = Banderwagon.Generator();

        transcript.AppendPoint(generator, "generator");
        Fr? challenge = transcript.ChallengeScalar("simple_challenge");

        Assert.IsTrue(Convert.ToHexString(challenge.ToBytes()).ToLower()
            .SequenceEqual("8c2dafe7c0aabfa9ed542bb2cbf0568399ae794fc44fdfd7dff6cc0e6144921c"));
    }
}
