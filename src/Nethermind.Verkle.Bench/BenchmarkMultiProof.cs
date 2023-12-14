// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Polynomial;
using Nethermind.Verkle.Proofs;
using Nethermind.Verkle.Tests.Proofs;

namespace Nethermind.Verkle.Bench;

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkMultiProof1 : BenchmarkMultiProofBase
{
    public BenchmarkMultiProof1() : base(1) { }

    [Benchmark]
    public void BenchmarkBasicMultiProof1()
    {
        Transcript proverTranscript = new("test");
        VerkleProofStruct proof = _prover.MakeMultiProof(proverTranscript, new List<VerkleProverQuery>(_proverQueries));
    }

    [Benchmark]
    public void BenchmarkVerification1()
    {
        Transcript proverTranscript = new("test");
        bool verification = _prover.CheckMultiProof(proverTranscript, _verifierQueries, _proof);
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkMultiProof1000 : BenchmarkMultiProofBase
{
    public BenchmarkMultiProof1000() : base(1000) { }

    [Benchmark]
    public void BenchmarkBasicMultiProof1000()
    {
        Transcript proverTranscript = new("test");
        VerkleProofStruct proof = _prover.MakeMultiProof(proverTranscript, new List<VerkleProverQuery>(_proverQueries));
    }

    [Benchmark]
    public void BenchmarkVerification1000()
    {
        Transcript proverTranscript = new("test");
        bool verification = _prover.CheckMultiProof(proverTranscript, _verifierQueries, _proof);
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkMultiProof2000 : BenchmarkMultiProofBase
{
    public BenchmarkMultiProof2000() : base(2000) { }

    [Benchmark]
    public void BenchmarkBasicMultiProof2000()
    {
        Transcript proverTranscript = new("test");
        VerkleProofStruct proof = _prover.MakeMultiProof(proverTranscript, new List<VerkleProverQuery>(_proverQueries));
    }

    [Benchmark]
    public void BenchmarkVerification2000()
    {
        Transcript proverTranscript = new("test");
        bool verification = _prover.CheckMultiProof(proverTranscript, _verifierQueries, _proof);
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkMultiProof4000 : BenchmarkMultiProofBase
{
    public BenchmarkMultiProof4000() : base(4000) { }

    [Benchmark]
    public void BenchmarkBasicMultiProof4000()
    {
        Transcript proverTranscript = new("test");
        VerkleProofStruct proof = _prover.MakeMultiProof(proverTranscript, new List<VerkleProverQuery>(_proverQueries));
    }

    [Benchmark]
    public void BenchmarkVerification4000()
    {
        Transcript proverTranscript = new("test");
        bool verification = _prover.CheckMultiProof(proverTranscript, _verifierQueries, _proof);
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkMultiProof8000 : BenchmarkMultiProofBase
{
    public BenchmarkMultiProof8000() : base(8000) { }

    [Benchmark]
    public void BenchmarkBasicMultiProof8000()
    {
        Transcript proverTranscript = new("test");
        VerkleProofStruct proof = _prover.MakeMultiProof(proverTranscript, new List<VerkleProverQuery>(_proverQueries));
    }

    [Benchmark]
    public void BenchmarkVerification8000()
    {
        Transcript proverTranscript = new("test");
        bool verification = _prover.CheckMultiProof(proverTranscript, _verifierQueries, _proof);
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkMultiProof16000 : BenchmarkMultiProofBase
{
    public BenchmarkMultiProof16000() : base(16000) { }

    [Benchmark]
    public void BenchmarkBasicMultiProof16000()
    {
        Transcript proverTranscript = new("test");
        VerkleProofStruct proof = _prover.MakeMultiProof(proverTranscript, new List<VerkleProverQuery>(_proverQueries));
    }

    [Benchmark]
    public void BenchmarkVerification16000()
    {
        Transcript proverTranscript = new("test");
        bool verification = _prover.CheckMultiProof(proverTranscript, _verifierQueries, _proof);
    }
}

public class BenchmarkMultiProofBase
{
    protected readonly VerkleProofStruct _proof;
    protected readonly MultiProof _prover;
    protected readonly VerkleProverQuery[] _proverQueries;
    protected readonly VerkleVerifierQuery[] _verifierQueries;

    protected BenchmarkMultiProofBase(int numPoly)
    {
        _prover = new MultiProof(CRS.Instance, PreComputedWeights.Instance);
        _proverQueries = MultiProofTests.GenerateRandomQueries(numPoly).ToArray();
        _proof = GenerateProof(_proverQueries);
        _verifierQueries = _proverQueries
            .Select(x => new VerkleVerifierQuery(x.NodeCommitPoint, x.ChildIndex, x.ChildHash)).ToArray();
    }

    private VerkleProofStruct GenerateProof(VerkleProverQuery[] queries)
    {
        Transcript proverTranscript = new("test");
        return _prover.MakeMultiProof(proverTranscript, new List<VerkleProverQuery>(queries));
    }
}
