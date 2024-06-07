// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using Nethermind.RustVerkle;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Fields.FrEElement;
using Nethermind.Verkle.Polynomial;
using Nethermind.Verkle.Proofs;
using Nethermind.Verkle.Tests.Proofs;

namespace Nethermind.Verkle.Bench.MultiProofs;

public class BenchmarkMultiProofBase
{
    protected readonly VerkleProofStruct _proof;
    protected readonly byte[] _proofUncompressed;
    protected readonly List<VerkleProverQuery> _proverQueries;
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

        _proverQueryInput = SerializeProverQueriesForRust(_proverQueries);

        _context = RustVerkleLib.VerkleContextNew();

        _proof = GenerateProof(_proverQueries.ToArray());
        _proofUncompressed = GenerateProofSerialized(_proverQueries.ToArray());

        _verifierQueries = GetVerifierQueries(_proverQueries);

        _verifierQueryInput = SerializeVerifierQueriesForRust(_verifierQueries, _proofUncompressed);
    }

    protected static MultiProof Prover => new(CRS.Instance, PreComputedWeights.Instance);

    private static VerkleProofStruct GenerateProof(VerkleProverQuery[] queries)
    {
        Transcript proverTranscript = new("verkle");
        return Prover.MakeMultiProof(proverTranscript, [.. queries]);
    }

    private static byte[] GenerateProofSerialized(VerkleProverQuery[] queries)
    {
        VerkleProverQuerySerialized[] queriesSerialized = queries
            .Select(VerkleProverQuerySerialized.CreateProverQuerySerialized)
            .ToArray();
        VerkleProofStructSerialized proof = Prover.MakeMultiProofSerialized(queriesSerialized);
        return proof.Encode();
    }


    private static VerkleVerifierQuery[] GetVerifierQueries(List<VerkleProverQuery> queries)
    {
        return queries
            .Select(x => new VerkleVerifierQuery(x.NodeCommitPoint, x.ChildIndex, x.ChildHash)).ToArray();
    }

    private static VerkleVerifierQuerySerialized[] GetVerifierQueriesSerialized(List<VerkleProverQuery> queries)
    {

        VerkleVerifierQuerySerialized[] verifierQueries = queries
            .Select(
                x => new VerkleVerifierQuerySerialized(
                    x.NodeCommitPoint.ToBytesUncompressedLittleEndian(),
                    x.ChildIndex,
                    x.ChildHash.ToBytes()
                )
            ).ToArray();

        return verifierQueries;
    }

    private static byte[] SerializeProverQueriesForRust(List<VerkleProverQuery> queries)
    {
        List<byte> input = [];
        foreach (VerkleProverQuery query in queries)
        {
            input.AddRange(query.NodeCommitPoint.ToBytesUncompressedLittleEndian());
            foreach (FrE eval in query.ChildHashPoly.Evaluations)
            {
                input.AddRange(eval.ToBytes());
            }
            input.Add(query.ChildIndex);
            input.AddRange(query.ChildHash.ToBytes());
        }
        return input.ToArray();
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
