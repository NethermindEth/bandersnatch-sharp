// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nethermind.RustVerkle;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Fields.FrEElement;
using Nethermind.Verkle.Polynomial;
using Nethermind.Verkle.Proofs;
using Nethermind.Verkle.Tests.Proofs;

namespace Nethermind.Verkle.Bench;

public class BenchmarkMultiProofBase
{
    protected readonly VerkleProofStruct _proof;
    protected readonly List<VerkleProverQuery> _proverQueries;
    protected readonly VerkleVerifierQuery[] _verifierQueries;
    protected readonly byte[] _proverQueryInput;
    protected readonly IntPtr Context;

    protected BenchmarkMultiProofBase(int numPoly)
    {
        _proverQueries = MultiProofTests.GenerateRandomQueries(numPoly);
        List<byte> input = new();
        foreach (var query in _proverQueries)
        {
            input.AddRange(query.NodeCommitPoint.ToBytes());
            foreach (FrE eval in query.ChildHashPoly.Evaluations)
            {
                input.AddRange(eval.ToBytes());
            }
            input.Add(query.ChildIndex);
            input.AddRange(query.ChildHash.ToBytes());
        }

        _proverQueryInput = input.ToArray();
        Context = RustVerkleLib.VerkleContextNew();

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
        Transcript proverTranscript = new("verkle");
        return Prover.MakeMultiProof(proverTranscript, _proverQueries);
    }

    [Benchmark]
    public bool BenchmarkBasicMultiProof8000Rust()
    {
        byte[] output = new byte[576];
        RustVerkleLib.VerkleProve(Context, _proverQueryInput, (UIntPtr)_proverQueryInput.Length, output);
        return true;
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
    public bool BenchmarkBasicMultiProof8000Rust()
    {
        byte[] output = new byte[576];
        RustVerkleLib.VerkleProve(Context, _proverQueryInput, (UIntPtr)_proverQueryInput.Length, output);
        return true;
    }
}
