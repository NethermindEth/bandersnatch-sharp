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
[NoIntrinsicsJob(RuntimeMoniker.Net70)]
[MemoryDiagnoser]
public class BenchmarkIpaProve
{
    private readonly FrE[] _poly =
    {
        FrE.SetElement(1), FrE.SetElement(2), FrE.SetElement(3), FrE.SetElement(4), FrE.SetElement(5), FrE.SetElement(6), FrE.SetElement(7), FrE.SetElement(8), FrE.SetElement(9), FrE.SetElement(10), FrE.SetElement(11), FrE.SetElement(12), FrE.SetElement(13),
        FrE.SetElement(14), FrE.SetElement(15), FrE.SetElement(16), FrE.SetElement(17), FrE.SetElement(18), FrE.SetElement(19), FrE.SetElement(20), FrE.SetElement(21), FrE.SetElement(22), FrE.SetElement(23), FrE.SetElement(24), FrE.SetElement(25), FrE.SetElement(26),
        FrE.SetElement(27), FrE.SetElement(28), FrE.SetElement(29), FrE.SetElement(30), FrE.SetElement(31), FrE.SetElement(32)
    };

    private readonly FrE[] _lagPoly;
    private readonly PreComputedWeights _weights;
    private readonly CRS _crs;

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

        _lagPoly = lagrangePoly.ToArray();
        _weights = PreComputedWeights.Instance;
        _crs = CRS.Instance;
    }

    [Benchmark]
    public void TestBasicIpaProve()
    {
        Banderwagon commitment = _crs.Commit(_lagPoly);
        FrE inputPoint = FrE.SetElement(2101);
        FrE[] b = _weights.BarycentricFormulaConstants(inputPoint);
        IpaProverQuery query = new (_lagPoly, commitment, inputPoint, b);
        Transcript proverTranscript = new Transcript("test");
        Ipa.MakeIpaProof(_crs, proverTranscript, query, out FrE outputPoint);
    }
}

[SimpleJob(RuntimeMoniker.Net70)]
[NoIntrinsicsJob(RuntimeMoniker.Net70)]
[MemoryDiagnoser]
public class BenchmarkIpaVerify
{
    private readonly FrE[] _poly =
    {
        FrE.SetElement(1), FrE.SetElement(2), FrE.SetElement(3), FrE.SetElement(4), FrE.SetElement(5), FrE.SetElement(6), FrE.SetElement(7), FrE.SetElement(8), FrE.SetElement(9), FrE.SetElement(10), FrE.SetElement(11), FrE.SetElement(12), FrE.SetElement(13),
        FrE.SetElement(14), FrE.SetElement(15), FrE.SetElement(16), FrE.SetElement(17), FrE.SetElement(18), FrE.SetElement(19), FrE.SetElement(20), FrE.SetElement(21), FrE.SetElement(22), FrE.SetElement(23), FrE.SetElement(24), FrE.SetElement(25), FrE.SetElement(26),
        FrE.SetElement(27), FrE.SetElement(28), FrE.SetElement(29), FrE.SetElement(30), FrE.SetElement(31), FrE.SetElement(32)
    };

    private readonly IpaProofStruct _proof;
    private readonly Banderwagon _commitment;
    private readonly FrE _inputPoint;
    private readonly FrE _outputPoint;
    private readonly CRS _crs;
    private readonly PreComputedWeights _weights;

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

        FrE[] lagPoly = lagrangePoly.ToArray();

        _weights = PreComputedWeights.Instance;

        _crs = CRS.Instance;
        _commitment = _crs.Commit(lagPoly);

        Transcript proverTranscript = new Transcript("test");

        _inputPoint = FrE.SetElement(2101);
        FrE[] b = _weights.BarycentricFormulaConstants(_inputPoint);
        IpaProverQuery query = new (lagPoly, _commitment, _inputPoint, b);

        _proof = Ipa.MakeIpaProof(_crs, proverTranscript, query, out _outputPoint);
    }

    [Benchmark]
    public void TestBasicIpaVerify()
    {
        Transcript verifierTranscript = new("test");
        FrE[] b = _weights.BarycentricFormulaConstants(_inputPoint);
        IpaVerifierQuery queryX = new(_commitment, _inputPoint, b, _outputPoint, _proof);
        Ipa.CheckIpaProof(_crs, verifierTranscript, queryX);
    }
}
