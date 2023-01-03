using System.Diagnostics;
using Nethermind.Verkle.Db;
using Nethermind.Verkle.Tree.VerkleNodes;
using Nethermind.Verkle.Tree.VerkleStateDb;

namespace Nethermind.Verkle.Tree;

public class VerkleStateStore : IVerkleStore
{
    // the blockNumber for with the fullState exists.
    private long FullStateBlock { get; set; }

    // The underlying key value database
    // We try to avoid fetching from this, and we only store at the end of a batch insert
    private IVerkleDb Storage { get; }

    // This stores the key-value pairs that we need to insert into the storage. This is generally
    // used to batch insert changes for each block. This is also used to generate the forwardDiff.
    // This is flushed after every batch insert and cleared.
    private IVerkleDiffDb Batch { get; }

    // This should store the top 3 layers of the trie, since these are the most accessed in
    // the trie on average, thus speeding up some operations. But right now every things is
    // stored in the cache - bad design.
    // TODO: modify the cache to only store the top 3 layers
    private IVerkleDiffDb Cache { get; }

    // These database stores the forwardDiff and reverseDiff for each block. Diffs are generated when
    // the Flush(long blockNumber) method is called.
    // TODO: add capability to update the diffs instead of overwriting if Flush(long blockNumber) is called multiple times for the same block number
    private DiffLayer ForwardDiff { get; }
    private DiffLayer ReverseDiff { get; }

    public VerkleStateStore(DbMode dbMode, string? dbPath)
    {
        Storage = new DiskDb(dbMode, dbPath);
        Batch = new MemoryStateDb();
        Cache = new MemoryStateDb();
        ForwardDiff = new DiffLayer(DiffType.Forward);
        ReverseDiff = new DiffLayer(DiffType.Reverse);
        FullStateBlock = 0;
    }

    // This generates and returns a batchForwardDiff, that can be used to move the full state from fromBlock to toBlock.
    // for this fromBlock < toBlock - move forward in time
    public IVerkleDiffDb GetForwardMergedDiff(long fromBlock, long toBlock)
    {
        return ForwardDiff.MergeDiffs(fromBlock, toBlock);
    }

    // This generates and returns a batchForwardDiff, that can be used to move the full state from fromBlock to toBlock.
    // for this fromBlock > toBlock - move back in time
    public IVerkleDiffDb GetReverseMergedDiff(long fromBlock, long toBlock)
    {
        return ReverseDiff.MergeDiffs(fromBlock, toBlock);
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

    // This method is called at the end of each block to flush the batch changes to the storage and generate forward and reverse diffs.
    // this should be called only once per block, right now it does not support multiple calls for the same block number.
    // if called multiple times, the full state would be fine - but it would corrupt the diffs and historical state will be lost
    // TODO: add capability to update the diffs instead of overwriting if Flush(long blockNumber) is called multiple times for the same block number
    public void Flush(long blockNumber)
    {
        // we should not have any null values in the Batch db - because deletion of values from verkle tree is not allowed
        // nullable values are allowed in MemoryStateDb only for reverse diffs.
        MemoryStateDb reverseDiff = new MemoryStateDb();

        foreach (KeyValuePair<byte[], byte[]?> entry in Batch.LeafNodes)
        {
            Debug.Assert(entry.Value is not null, "nullable value only for reverse diff");
            if (Storage.GetLeaf(entry.Key, out byte[]? node)) reverseDiff.LeafTable[entry.Key] = node;
            else reverseDiff.LeafTable[entry.Key] = null;

            Storage.SetLeaf(entry.Key, entry.Value);
        }

        foreach (KeyValuePair<byte[], SuffixTree?> entry in Batch.StemNodes)
        {
            Debug.Assert(entry.Value is not null, "nullable value only for reverse diff");
            if (Storage.GetStem(entry.Key, out SuffixTree? node)) reverseDiff.StemTable[entry.Key] = node;
            else reverseDiff.StemTable[entry.Key] = null;

            Storage.SetStem(entry.Key, entry.Value);
        }

        foreach (KeyValuePair<byte[], InternalNode?> entry in Batch.BranchNodes)
        {
            Debug.Assert(entry.Value is not null, "nullable value only for reverse diff");
            if (Storage.GetBranch(entry.Key, out InternalNode? node)) reverseDiff.BranchTable[entry.Key] = node;
            else reverseDiff.BranchTable[entry.Key] = null;

            Storage.SetBranch(entry.Key, entry.Value);
        }

        ForwardDiff.InsertDiff(blockNumber, Batch);
        ReverseDiff.InsertDiff(blockNumber, reverseDiff);
        FullStateBlock = blockNumber;
    }

    // now the full state back in time by one block.
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
        FullStateBlock -= 1;
    }

    // use the batch diff to move the full state back in time to access historical state.
    public void ReverseState(IVerkleDiffDb reverseBatch, long numBlocks)
    {
        MemoryStateDb reverseDiff = (MemoryStateDb)reverseBatch;

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
        FullStateBlock -= numBlocks;
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
    void ReverseState(IVerkleDiffDb reverseBatch, long numBlocks);

    public IVerkleDiffDb GetForwardMergedDiff(long fromBlock, long toBlock);

    public IVerkleDiffDb GetReverseMergedDiff(long fromBlock, long toBlock);
}
