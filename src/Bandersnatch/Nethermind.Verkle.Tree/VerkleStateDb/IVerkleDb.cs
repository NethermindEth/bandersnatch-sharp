// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

namespace Nethermind.Verkle.Tree.VerkleStateDb;
using LeafStore = Dictionary<byte[], byte[]?>;
using SuffixStore = Dictionary<byte[], SuffixTree?>;
using BranchStore = Dictionary<byte[], InternalNode?>;


public interface IVerkleDb
{
    bool GetLeaf(byte[] key, out byte[]? value);
    bool GetStem(byte[] key, out SuffixTree? value);
    bool GetBranch(byte[] key, out InternalNode? value);
    void SetLeaf(byte[] leafKey, byte[] leafValue);
    void SetStem(byte[] stemKey, SuffixTree suffixTree);
    void SetBranch(byte[] branchKey, InternalNode internalNodeValue);
    void RemoveLeaf(byte[] leafKey);
    void RemoveStem(byte[] stemKey);
    void RemoveBranch(byte[] branchKey);

    void BatchLeafInsert(IEnumerable<KeyValuePair<byte[], byte[]?>> keyLeaf);
    void BatchStemInsert(IEnumerable<KeyValuePair<byte[], SuffixTree?>> suffixLeaf);
    void BatchBranchInsert(IEnumerable<KeyValuePair<byte[], InternalNode?>> branchLeaf);
}

public interface IVerkleDiffDb: IVerkleDb
{
    byte[] Encode();
    IEnumerable<KeyValuePair<byte[], byte[]?>> LeafNodes { get; }
    IEnumerable<KeyValuePair<byte[], SuffixTree?>> StemNodes { get; }
    IEnumerable<KeyValuePair<byte[], InternalNode?>> BranchNodes { get; }
}
