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
    protected readonly List<VerkleProverQuery> _proverQueries;
    protected readonly VerkleVerifierQuery[] _verifierQueries;
    protected readonly byte[] _proverQueryInput;
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
        _verifierQueries = GetVerifierQueriesFromProverQueries(_proverQueries);
        List<byte> input = new(_proof.Encode());
        _verifierQueryInput = SerializeVerifierQueriesForRust(_verifierQueries, input);
    }

    protected static MultiProof Prover => new(CRS.Instance, PreComputedWeights.Instance);

    private static VerkleProofStruct GenerateProof(VerkleProverQuery[] queries)
    {
        Transcript proverTranscript = new("verkle");
        return Prover.MakeMultiProof(proverTranscript, [.. queries]);
    }

    private static VerkleVerifierQuery[] GetVerifierQueriesFromProverQueries(List<VerkleProverQuery> queries)
    {
        return queries
            .Select(x => new VerkleVerifierQuery(x.NodeCommitPoint, x.ChildIndex, x.ChildHash)).ToArray();
    }

    private static byte[] SerializeProverQueriesForRust(List<VerkleProverQuery> queries)
    {
        List<byte> input = [];
        foreach (VerkleProverQuery query in queries)
        {
            input.AddRange(query.NodeCommitPoint.ToBytes());
            foreach (FrE eval in query.ChildHashPoly.Evaluations)
            {
                input.AddRange(eval.ToBytes());
            }
            input.Add(query.ChildIndex);
            input.AddRange(query.ChildHash.ToBytes());
        }
        return input.ToArray();
    }

    private static byte[] SerializeVerifierQueriesForRust(VerkleVerifierQuery[] queries, List<byte> input)
    {
        foreach (VerkleVerifierQuery query in queries)
        {
            input.AddRange(query.NodeCommitPoint.ToBytes());
            input.Add(query.ChildIndex);
            input.AddRange(query.ChildHash.ToBytes());
        }
        return input.ToArray();
    }
}
