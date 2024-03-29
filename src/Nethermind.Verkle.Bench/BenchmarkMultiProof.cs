// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Polynomial;
using Nethermind.Verkle.Proofs;
using Nethermind.Verkle.Tests.Proofs;

namespace Nethermind.Verkle.Bench;

public class BenchmarkMultiProofBase
{
    protected readonly VerkleProofStruct _proof;
    protected readonly List<VerkleProverQuery> _proverQueries;
    protected readonly VerkleVerifierQuery[] _verifierQueries;

    protected BenchmarkMultiProofBase(int numPoly)
    {
        _proverQueries = MultiProofTests.GenerateRandomQueries(numPoly);
        _proof = GenerateProof(_proverQueries.ToArray());
        _verifierQueries = _proverQueries
            .Select(x => new VerkleVerifierQuery(x.NodeCommitPoint, x.ChildIndex, x.ChildHash)).ToArray();
    }

    protected static MultiProof Prover => new(CRS.Instance, PreComputedWeights.Instance);

    private VerkleProofStruct GenerateProof(VerkleProverQuery[] queries)
    {
        Transcript proverTranscript = new("test");
        return Prover.MakeMultiProof(proverTranscript, [..queries]);
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkMultiProof1() : BenchmarkMultiProofBase(1)
{
    [Benchmark]
    public VerkleProofStruct BenchmarkBasicMultiProof1()
    {
        Transcript proverTranscript = new("test");
        return Prover.MakeMultiProof(proverTranscript, _proverQueries);
    }

    [Benchmark]
    public bool BenchmarkVerification1()
    {
        Transcript proverTranscript = new("test");
        return Prover.CheckMultiProof(proverTranscript, _verifierQueries, _proof);
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkMultiProof1000() : BenchmarkMultiProofBase(1000)
{
    [Benchmark]
    public VerkleProofStruct BenchmarkBasicMultiProof1000()
    {
        Transcript proverTranscript = new("test");
        return Prover.MakeMultiProof(proverTranscript, _proverQueries);
    }

    [Benchmark]
    public bool BenchmarkVerification1000()
    {
        Transcript proverTranscript = new("test");
        return Prover.CheckMultiProof(proverTranscript, _verifierQueries, _proof);
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkMultiProof2000() : BenchmarkMultiProofBase(2000)
{
    [Benchmark]
    public VerkleProofStruct BenchmarkBasicMultiProof2000()
    {
        Transcript proverTranscript = new("test");
        return Prover.MakeMultiProof(proverTranscript, _proverQueries);
    }

    [Benchmark]
    public bool BenchmarkVerification2000()
    {
        Transcript proverTranscript = new("test");
        return Prover.CheckMultiProof(proverTranscript, _verifierQueries, _proof);
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkMultiProof4000() : BenchmarkMultiProofBase(4000)
{
    [Benchmark]
    public VerkleProofStruct BenchmarkBasicMultiProof4000()
    {
        Transcript proverTranscript = new("test");
        return Prover.MakeMultiProof(proverTranscript, _proverQueries);
    }

    [Benchmark]
    public bool BenchmarkVerification4000()
    {
        Transcript proverTranscript = new("test");
        return Prover.CheckMultiProof(proverTranscript, _verifierQueries, _proof);
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkMultiProof8000() : BenchmarkMultiProofBase(8000)
{
    [Benchmark]
    public VerkleProofStruct BenchmarkBasicMultiProof8000()
    {
        Transcript proverTranscript = new("test");
        return Prover.MakeMultiProof(proverTranscript, _proverQueries);
    }

    [Benchmark]
    public bool BenchmarkVerification8000()
    {
        Transcript proverTranscript = new("test");
        return Prover.CheckMultiProof(proverTranscript, _verifierQueries, _proof);
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkMultiProof16000() : BenchmarkMultiProofBase(16000)
{
    [Benchmark]
    public VerkleProofStruct BenchmarkBasicMultiProof16000()
    {
        Transcript proverTranscript = new("test");
        return Prover.MakeMultiProof(proverTranscript, _proverQueries);
    }

    [Benchmark]
    public bool BenchmarkVerification16000()
    {
        Transcript proverTranscript = new("test");
        return Prover.CheckMultiProof(proverTranscript, _verifierQueries, _proof);
    }
}
