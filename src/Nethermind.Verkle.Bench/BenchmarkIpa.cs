// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Fields.FrEElement;
using Nethermind.Verkle.Polynomial;
using Nethermind.Verkle.Proofs;

namespace Nethermind.Verkle.Bench;

[SimpleJob(RuntimeMoniker.Net70)]
[MemoryDiagnoser]
public class BenchmarkIpaProve
{
    private readonly FrE[] _poly =
    {
        FrE.SetElement(1), FrE.SetElement(2), FrE.SetElement(3), FrE.SetElement(4), FrE.SetElement(5), FrE.SetElement(6), FrE.SetElement(7), FrE.SetElement(8), FrE.SetElement(9), FrE.SetElement(10), FrE.SetElement(11), FrE.SetElement(12), FrE.SetElement(13),
        FrE.SetElement(14), FrE.SetElement(15), FrE.SetElement(16), FrE.SetElement(17), FrE.SetElement(18), FrE.SetElement(19), FrE.SetElement(20), FrE.SetElement(21), FrE.SetElement(22), FrE.SetElement(23), FrE.SetElement(24), FrE.SetElement(25), FrE.SetElement(26),
        FrE.SetElement(27), FrE.SetElement(28), FrE.SetElement(29), FrE.SetElement(30), FrE.SetElement(31), FrE.SetElement(32)
    };

    private FrE[] lagPoly;
    private PreComputedWeights weights;
    private CRS crs;

    public BenchmarkIpaProve()
    {
        FrE[] domain = new FrE[256];
        for (int i = 0; i < 256; i++)
        {
            domain[i] = FrE.SetElement(i);
        }

        List<FrE> lagrangePoly = new List<FrE>();

        for (int i = 0; i < 8; i++)
        {
            lagrangePoly.AddRange(_poly);
        }

        lagPoly = lagrangePoly.ToArray();
        weights = PreComputedWeights.Instance;
        crs = CRS.Instance;
    }

    [Benchmark]
    public void TestBasicIpaProof()
    {
        Banderwagon commitment = crs.Commit(lagPoly);

        Transcript proverTranscript = new Transcript("test");

        FrE inputPoint = FrE.SetElement(2101);
        FrE[] b = weights.BarycentricFormulaConstants(inputPoint);
        IpaProverQuery query = new (lagPoly, commitment, inputPoint, b);

        Ipa.MakeIpaProof(crs, proverTranscript, query, out FrE outputPoint);
    }
}

[SimpleJob(RuntimeMoniker.Net70)]
[MemoryDiagnoser]
public class BenchmarkIpaVerify
{
    private readonly FrE[] _poly =
    {
        FrE.SetElement(1), FrE.SetElement(2), FrE.SetElement(3), FrE.SetElement(4), FrE.SetElement(5), FrE.SetElement(6), FrE.SetElement(7), FrE.SetElement(8), FrE.SetElement(9), FrE.SetElement(10), FrE.SetElement(11), FrE.SetElement(12), FrE.SetElement(13),
        FrE.SetElement(14), FrE.SetElement(15), FrE.SetElement(16), FrE.SetElement(17), FrE.SetElement(18), FrE.SetElement(19), FrE.SetElement(20), FrE.SetElement(21), FrE.SetElement(22), FrE.SetElement(23), FrE.SetElement(24), FrE.SetElement(25), FrE.SetElement(26),
        FrE.SetElement(27), FrE.SetElement(28), FrE.SetElement(29), FrE.SetElement(30), FrE.SetElement(31), FrE.SetElement(32)
    };

    private FrE[] lagPoly;

    private IpaProofStruct proof;
    private Banderwagon commitment;
    private FrE inputPoint;
    private FrE outputPoint;

    public BenchmarkIpaVerify()
    {
        FrE[] domain = new FrE[256];
        for (int i = 0; i < 256; i++)
        {
            domain[i] = FrE.SetElement(i);
        }

        List<FrE> lagrangePoly = new List<FrE>();

        for (int i = 0; i < 8; i++)
        {
            lagrangePoly.AddRange(_poly);
        }

        lagPoly = lagrangePoly.ToArray();

        PreComputedWeights weights = PreComputedWeights.Instance;

        CRS crs = CRS.Instance;
        commitment = crs.Commit(lagPoly);

        Transcript proverTranscript = new Transcript("test");

        inputPoint = FrE.SetElement(2101);
        FrE[] b = weights.BarycentricFormulaConstants(inputPoint);
        IpaProverQuery query = new (lagPoly, commitment, inputPoint, b);

        proof = Ipa.MakeIpaProof(crs, proverTranscript, query, out outputPoint);
    }

    [Benchmark]
    public void TestBasicIpaProof()
    {
        CRS crs = CRS.Instance;
        PreComputedWeights weights = PreComputedWeights.Instance;
        Transcript verifierTranscript = new Transcript("test");
        FrE[] b = weights.BarycentricFormulaConstants(inputPoint);
        IpaVerifierQuery queryX = new IpaVerifierQuery(commitment, inputPoint, b, outputPoint, proof);

        bool ok = Ipa.CheckIpaProof(crs, verifierTranscript, queryX);
    }
}
