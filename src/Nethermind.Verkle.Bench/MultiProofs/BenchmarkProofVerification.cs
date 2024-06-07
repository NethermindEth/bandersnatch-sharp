// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nethermind.RustVerkle;
using Nethermind.Verkle.Proofs;

namespace Nethermind.Verkle.Bench.MultiProofs;

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkProofVerification1Opening() : BenchmarkMultiProofBase(1)
{

    [Benchmark]
    public bool ProofVerification1Openings()
    {
        Transcript proverTranscript = new("verkle");
        return Prover.CheckMultiProof(proverTranscript, _verifierQueries, _proof);
    }

    [Benchmark]
    public bool BenchmarkVerification1OpeningRust()
    {
        return RustVerkleLib.VerkleVerifyUncompressed(
            _context,
            _verifierQueryInput,
            (UIntPtr)_verifierQueryInput.Length
        );
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkProofVerification1000Openings() : BenchmarkMultiProofBase(1000)
{
    [Benchmark]
    public bool ProofVerification1000Openings()
    {
        Transcript proverTranscript = new("verkle");
        return Prover.CheckMultiProof(proverTranscript, _verifierQueries, _proof);
    }

    [Benchmark]
    public bool BenchmarkVerification1000OpeningRust()
    {
        return RustVerkleLib.VerkleVerifyUncompressed(
            _context,
            _verifierQueryInput,
            (UIntPtr)_verifierQueryInput.Length
        );
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkProofVerification2000Openings() : BenchmarkMultiProofBase(2000)
{

    [Benchmark]
    public bool ProofVerification2000Openings()
    {
        Transcript proverTranscript = new("verkle");
        return Prover.CheckMultiProof(proverTranscript, _verifierQueries, _proof);
    }

    [Benchmark]
    public bool BenchmarkVerification2000OpeningRust()
    {
        return RustVerkleLib.VerkleVerifyUncompressed(
            _context,
            _verifierQueryInput,
            (UIntPtr)_verifierQueryInput.Length
        );
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkProofVerification4000Openings() : BenchmarkMultiProofBase(4000)
{

    [Benchmark]
    public bool ProofVerification4000Openings()
    {
        Transcript proverTranscript = new("verkle");
        return Prover.CheckMultiProof(proverTranscript, _verifierQueries, _proof);
    }

    [Benchmark]
    public bool BenchmarkVerification4000OpeningRust()
    {
        return RustVerkleLib.VerkleVerifyUncompressed(
            _context,
            _verifierQueryInput,
            (UIntPtr)_verifierQueryInput.Length
        );
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkProofVerification8000Openings() : BenchmarkMultiProofBase(8000)
{

    [Benchmark]
    public bool ProofVerification8000Openings()
    {
        Transcript proverTranscript = new("verkle");
        return Prover.CheckMultiProof(proverTranscript, _verifierQueries, _proof);
    }

    [Benchmark]
    public bool BenchmarkVerification8000OpeningRust()
    {
        return RustVerkleLib.VerkleVerifyUncompressed(
            _context,
            _verifierQueryInput,
            (UIntPtr)_verifierQueryInput.Length
        );
    }
}

[SimpleJob(RuntimeMoniker.Net80)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchmarkProofVerification16000Openings() : BenchmarkMultiProofBase(16000)
{
    [Benchmark]
    public bool ProofVerification16000Openings()
    {
        Transcript proverTranscript = new("verkle");
        return Prover.CheckMultiProof(proverTranscript, _verifierQueries, _proof);
    }

    [Benchmark]
    public bool BenchmarkVerification16000OpeningRustUncompressed()
    {
        return RustVerkleLib.VerkleVerifyUncompressed(
            _context,
            _verifierQueryInput,
            (UIntPtr)_verifierQueryInput.Length
        );
    }
}

