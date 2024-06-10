// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using Nethermind.RustVerkle;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Polynomial;
using Nethermind.Verkle.Proofs;
using Nethermind.Verkle.Tests.Proofs;

namespace Nethermind.Verkle.Bench.MultiProofs;

public class BenchmarkMultiProofBase
{
    protected readonly VerkleProofStruct _proof;
    protected readonly byte[] _proofUncompressed;
    protected readonly List<VerkleProverQuery> _proverQueries;
    protected readonly VerkleProverQuerySerialized[] _proverQueriesSerialized;
    protected readonly byte[] _proverQueryInput;
    protected readonly VerkleVerifierQuery[] _verifierQueries;
    protected readonly byte[] _verifierQueryInput;
    protected readonly IntPtr _context;

    public static List<VerkleProverQuery> GenerateProverQueries(int numberOfOpenings)
    {
        return MultiProofTests.GenerateRandomQueries(numberOfOpenings);
    }

    protected BenchmarkMultiProofBase(int numPoly)
    {
        _proverQueries = MultiProofTests.GenerateRandomQueries(numPoly);
        _proverQueriesSerialized = MultiProofTests.GenerateRandomQueriesSerialized(numPoly);

        _proverQueryInput = SerializeProverQueriesForRust(_proverQueriesSerialized);

        _context = RustVerkleLib.VerkleContextNew();

        _proof = GenerateProof(_proverQueries.ToArray());
        _proofUncompressed = GenerateProofSerialized(_proverQueriesSerialized);

        _verifierQueries = GetVerifierQueries(_proverQueries);

        _verifierQueryInput = SerializeVerifierQueriesForRust(_verifierQueries, _proofUncompressed);
    }

    protected static MultiProof Prover => new(CRS.Instance, PreComputedWeights.Instance);

    private static VerkleProofStruct GenerateProof(VerkleProverQuery[] queries)
    {
        Transcript proverTranscript = new("verkle");
        return Prover.MakeMultiProof(proverTranscript, [.. queries]);
    }

    private static byte[] GenerateProofSerialized(VerkleProverQuerySerialized[] queries)
    {
        VerkleProofStructSerialized proof = Prover.MakeMultiProofSerialized(queries);
        return proof.Encode();
    }


    private static VerkleVerifierQuery[] GetVerifierQueries(List<VerkleProverQuery> queries)
    {
        return queries
            .Select(x => new VerkleVerifierQuery(x.NodeCommitPoint, x.ChildIndex, x.ChildHash)).ToArray();
    }

    private static byte[] SerializeProverQueriesForRust(VerkleProverQuerySerialized[] queries)
    {
        return queries.SelectMany(query => query.Encode()).ToArray();
    }

    private static byte[] SerializeVerifierQueriesForRust(VerkleVerifierQuery[] queries, byte[] proof)
    {
        List<byte> input = new(proof);
        foreach (VerkleVerifierQuery query in queries)
        {
            input.AddRange(query.NodeCommitPoint.ToBytesUncompressedLittleEndian());
            input.Add(query.ChildIndex);
            input.AddRange(query.ChildHash.ToBytes());
        }
        return input.ToArray();
    }
}
