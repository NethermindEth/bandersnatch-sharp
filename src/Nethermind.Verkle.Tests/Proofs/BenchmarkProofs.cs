// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Fields.FrEElement;
using Nethermind.Verkle.Polynomial;
using Nethermind.Verkle.Proofs;

namespace Nethermind.Verkle.Tests.Proofs;

public class BenchmarkProofs
{
    private static Banderwagon point = Banderwagon.Generator;
    private static FrE scalar = FrE.One;
    private (Banderwagon[], FrE[][], byte[]) GenerateNRandomPolysEvals(int n)
    {
        Banderwagon[] retCs = new Banderwagon[n];
        FrE[][] retEvals = new FrE[n][];
        byte[] retZs = new byte[n];

        var crs = CRS.Instance;

        var generator = new Random();
        var a = generator.Next();
        var b = generator.Next();

        using IEnumerator<FrE> randFrE = FrE.GetRandom().GetEnumerator();
        randFrE.MoveNext();
        randFrE.MoveNext();

        for (int i = 0; i < n; i++)
        {
            FrE[] frs = new FrE[256];
            for (int j = 0; j < 256; j++)
            {
                frs[j] = randFrE.Current;
                randFrE.MoveNext();
            }

            Banderwagon c = crs.Commit(frs);

            retEvals[i] = frs;
            retCs[i] = c;
            retZs[i] = (byte)generator.Next();
        }

        return (retCs, retEvals, retZs);
    }

    [Test]
    public void BenchProvingAndVerification()
    {
        MultiProof multiproof = new(CRS.Instance, PreComputedWeights.Instance);
        int[] nums = new[] { 1, 1000, 2000, 4000, 8000, 16000 };
        foreach (int num in nums)
        {
            List<VerkleProverQuery> queries = new();
            (Banderwagon[], FrE[][], byte[]) data = GenerateNRandomPolysEvals(num);

            for (int i = 0; i < num; i++)
            {
                VerkleProverQuery query = new(new LagrangeBasis(data.Item2[i]), data.Item1[i],
                    data.Item3[i], data.Item2[i][data.Item3[i]]);
                queries.Add(query);
            }

            Transcript proverTranscript = new Transcript("bench");
            Console.WriteLine($"Proving {num} polynomials:");
            VerkleProofStruct proof = multiproof.MakeMultiProof(proverTranscript, queries);
        }
    }

    [Test]
    public void BenchTranscript()
    {
        Transcript transcript = new("bench");
        transcript.DomainSep("multiproof");

        transcript.AppendPoint(point, "C");
        transcript.AppendScalar(scalar, "z");
        transcript.AppendScalar(scalar, "y");

        FrE r = transcript.ChallengeScalar("r");
    }

}
