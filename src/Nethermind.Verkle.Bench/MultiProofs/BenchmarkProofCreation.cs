// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nethermind.Verkle.Proofs;

namespace Nethermind.Verkle.Bench.MultiProofs;

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkProofCreation1Opening() : BenchmarkMultiProofBase(1)
{
    [Benchmark]
    public VerkleProofStruct ProofCreation1OpeningsCSharp()
    {
        Transcript proverTranscript = new("verkle");
        return Prover.MakeMultiProof(proverTranscript, _proverQueries);
    }

    [Benchmark]
    public void ProofCreation1OpeningRust()
    {
        byte[] output = new byte[576];
        RustVerkleLib.VerkleProve(_context, _proverQueryInput, (UIntPtr)_proverQueryInput.Length, output);

    }

    [Benchmark]
    public void ProofCreation1OpeningRustUncompressed()
    {
        byte[] output = new byte[576];
        RustVerkleLib.VerkleProveUncompressed(
            _context,
            _proverQueryInput,
            (UIntPtr)_proverQueryInput.Length,
            output
        );
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkProofCreation1000Openings() : BenchmarkMultiProofBase(1000)
{
    [Benchmark]
    public VerkleProofStruct ProofCreation1000OpeningsCSharp()
    {
        Transcript proverTranscript = new("verkle");
        return Prover.MakeMultiProof(proverTranscript, _proverQueries);
    }

    [Benchmark]
    public void ProofCreation1000OpeningsRust()
    {
        byte[] output = new byte[576];
        RustVerkleLib.VerkleProve(_context, _proverQueryInput, (UIntPtr)_proverQueryInput.Length, output);
    }

    [Benchmark]
    public void ProofCreation1000OpeningsRustUncompressed()
    {
        byte[] output = new byte[576];
        RustVerkleLib.VerkleProveUncompressed(
            _context,
            _proverQueryInput,
            (UIntPtr)_proverQueryInput.Length,
            output
        );
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkProofCreation2000Openings() : BenchmarkMultiProofBase(2000)
{
    [Benchmark]
    public VerkleProofStruct ProofCreation2000OpeningsCSharp()
    {
        Transcript proverTranscript = new("verkle");
        return Prover.MakeMultiProof(proverTranscript, _proverQueries);
    }

    [Benchmark]
    public void ProofCreation2000peningsRust()
    {
        byte[] output = new byte[1120];
        RustVerkleLib.VerkleProveUncompressed(
            _context,
            _proverQueryInput,
            (UIntPtr)_proverQueryInput.Length,
            output
        );
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkProofCreation4000Openings() : BenchmarkMultiProofBase(4000)
{
    [Benchmark]
    public VerkleProofStruct ProofCreation4000OpeningsCSharp()
    {
        Transcript proverTranscript = new("verkle");
        return Prover.MakeMultiProof(proverTranscript, _proverQueries);
    }

    [Benchmark]
    public void ProofCreation4000peningsRust()
    {
        byte[] output = new byte[1120];
        RustVerkleLib.VerkleProveUncompressed(
            _context,
            _proverQueryInput,
            (UIntPtr)_proverQueryInput.Length,
            output
        );
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkProofCreation8000Openings() : BenchmarkMultiProofBase(8000)
{
    [Benchmark]
    public VerkleProofStruct ProofCreation8000OpeningsCSharp()
    {
        Transcript proverTranscript = new("verkle");
        return Prover.MakeMultiProof(proverTranscript, _proverQueries);
    }

    [Benchmark]
    public void ProofCreation8000peningsRust()
    {
        byte[] output = new byte[1120];
        RustVerkleLib.VerkleProveUncompressed(
            _context,
            _proverQueryInput,
            (UIntPtr)_proverQueryInput.Length,
            output
        );
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkProofCreation16000Openings() : BenchmarkMultiProofBase(16000)
{
    [Benchmark]
    public VerkleProofStruct ProofCreation16000OpeningsCSharp()
    {
        Transcript proverTranscript = new("verkle");
        return Prover.MakeMultiProof(proverTranscript, _proverQueries);
    }

    [Benchmark]
    public void ProofCreation16000peningsRust()
    {
        byte[] output = new byte[1120];
        RustVerkleLib.VerkleProveUncompressed(
            _context,
            _proverQueryInput,
            (UIntPtr)_proverQueryInput.Length,
            output
        );
    }
}

