using System.Diagnostics;
using Nethermind.Verkle.Tree.VerkleStateDb;

namespace Nethermind.Verkle.Tree;

public class VerkleStateStore : IVerkleStore
{
    private long FullStateBlock { get; set; }
    private IVerkleDb Storage { get; }
    private IVerkleDiffDb Batch { get; }
    private IVerkleDiffDb Cache { get; }
    private DiffLayer ForwardDiff { get; }
    private DiffLayer ReverseDiff { get; }

    public VerkleStateStore()
    {
        Storage = new DiskDb();
        Batch = new MemoryStateDb();
        Cache = new MemoryStateDb();
        ForwardDiff = new DiffLayer();
        ReverseDiff = new DiffLayer();
        FullStateBlock = 0;
    }

    public void InitRootHash()
    {
        Batch.SetBranch(Array.Empty<byte>(), new BranchNode());
    }

    public byte[]? GetLeaf(byte[] key)
    {
        if (Cache.GetLeaf(key, out byte[]? value)) return value;
        if (Batch.GetLeaf(key, out value)) return value;
        return Storage.GetLeaf(key, out value) ? value : null;
    }

    public SuffixTree? GetStem(byte[] key)
    {
        if (Cache.GetStem(key, out SuffixTree? value)) return value;
        if (Batch.GetStem(key, out value)) return value;
        return Storage.GetStem(key, out value) ? value : null;
    }

    public InternalNode? GetBranch(byte[] key)
    {
        if (Cache.GetBranch(key, out InternalNode? value)) return value;
        if (Batch.GetBranch(key, out value)) return value;
        return Storage.GetBranch(key, out value) ? value : null;
    }

    public void SetLeaf(byte[] leafKey, byte[] leafValue)
    {
        Cache.SetLeaf(leafKey, leafValue);
        Batch.SetLeaf(leafKey, leafValue);
    }

    public void SetStem(byte[] stemKey, SuffixTree suffixTree)
    {
        Cache.SetStem(stemKey, suffixTree);
        Batch.SetStem(stemKey, suffixTree);
    }

    public void SetBranch(byte[] branchKey, InternalNode internalNodeValue)
    {
        Cache.SetBranch(branchKey, internalNodeValue);
        Batch.SetBranch(branchKey, internalNodeValue);
    }

    public void Flush(long blockNumber)
    {
        // we should not have any null values in the Batch db - because deletion of values from verkle tree is not allowed
        // nullable values are allowed in MemoryStateDb only for reverse diffs.
        MemoryStateDb reverseDiff = new MemoryStateDb();

        foreach (KeyValuePair<byte[], byte[]?> entry in Batch.LeafNodes)
        {
            Debug.Assert(entry.Key is not null, "nullable value only for reverse diff");
            if (Storage.GetLeaf(entry.Key, out byte[]? node)) reverseDiff.LeafTable[entry.Key] = node;
            else reverseDiff.LeafTable[entry.Key] = null;

            Storage.SetLeaf(entry.Key, entry.Value);
        }

        foreach (KeyValuePair<byte[], SuffixTree?> entry in Batch.StemNodes)
        {
            Debug.Assert(entry.Key is not null, "nullable value only for reverse diff");
            if (Storage.GetStem(entry.Key, out SuffixTree? node)) reverseDiff.StemTable[entry.Key] = node;
            else reverseDiff.StemTable[entry.Key] = null;

            Storage.SetStem(entry.Key, entry.Value);
        }

        foreach (KeyValuePair<byte[], InternalNode?> entry in Batch.BranchNodes)
        {
            Debug.Assert(entry.Key is not null, "nullable value only for reverse diff");
            if (Storage.GetBranch(entry.Key, out InternalNode? node)) reverseDiff.BranchTable[entry.Key] = node;
            else reverseDiff.BranchTable[entry.Key] = null;

            Storage.SetBranch(entry.Key, entry.Value);
        }

        ForwardDiff.InsertDiff(blockNumber, Batch);
        ReverseDiff.InsertDiff(blockNumber, reverseDiff);
        FullStateBlock = blockNumber;
    }

    public void ReverseState()
    {
        byte[] reverseDiffByte = ReverseDiff.FetchDiff(FullStateBlock);
        MemoryStateDb reverseDiff = MemoryStateDb.Decode(reverseDiffByte);

        foreach (KeyValuePair<byte[], byte[]?> entry in reverseDiff.LeafTable)
        {
            reverseDiff.GetLeaf(entry.Key, out byte[]? node);
            if (node is null)
            {
                Cache.RemoveLeaf(entry.Key);
                Storage.RemoveLeaf(entry.Key);
            }
            else
            {
                Cache.SetLeaf(entry.Key, node);
                Storage.SetLeaf(entry.Key, node);
            }
        }

        foreach (KeyValuePair<byte[], SuffixTree?> entry in reverseDiff.StemTable)
        {
            reverseDiff.GetStem(entry.Key, out SuffixTree? node);
            if (node is null)
            {
                Cache.RemoveStem(entry.Key);
                Storage.RemoveStem(entry.Key);
            }
            else
            {
                Cache.SetStem(entry.Key, node);
                Storage.SetStem(entry.Key, node);
            }
        }

        foreach (KeyValuePair<byte[], InternalNode?> entry in reverseDiff.BranchTable)
        {
            reverseDiff.GetBranch(entry.Key, out InternalNode? node);
            if (node is null)
            {
                Cache.RemoveBranch(entry.Key);
                Storage.RemoveBranch(entry.Key);
            }
            else
            {
                Cache.SetBranch(entry.Key, node);
                Storage.SetBranch(entry.Key, node);
            }
        }
    }
}

public interface IVerkleStore
{
    void InitRootHash();
    byte[]? GetLeaf(byte[] key);
    SuffixTree? GetStem(byte[] key);
    InternalNode? GetBranch(byte[] key);
    void SetLeaf(byte[] leafKey, byte[] leafValue);
    void SetStem(byte[] stemKey, SuffixTree suffixTree);
    void SetBranch(byte[] branchKey, InternalNode internalNodeValue);
    void Flush(long blockNumber);
    void ReverseState();
}
