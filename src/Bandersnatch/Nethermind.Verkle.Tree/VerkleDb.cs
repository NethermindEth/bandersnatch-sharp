namespace Nethermind.Verkle.Tree;

public class ByteArrayComparer : IEqualityComparer<byte[]>
{
    public bool Equals(byte[]? x, byte[]? y)
    {
        if (x == null || y == null)
        {
            return x == y;
        }
        return x.SequenceEqual(y);
    }

    public int GetHashCode(byte[] value)
    {
        HashCode hash = new HashCode();
        hash.AddBytes(value);
        return hash.ToHashCode();
    }
}

public class MemoryStateDb
{
    public Dictionary<byte[], byte[]> LeafTable { get; }
    public Dictionary<byte[], SuffixTree> StemTable { get; }
    public Dictionary<byte[], InternalNode> BranchTable { get; }

    public MemoryStateDb()
    {
        LeafTable = new Dictionary<byte[], byte[]>(new ByteArrayComparer());
        StemTable = new Dictionary<byte[], SuffixTree>(new ByteArrayComparer());
        BranchTable = new Dictionary<byte[], InternalNode>(new ByteArrayComparer());
    }
}

public class BlockDiff
{
    public Dictionary<long, byte[]> Forward { get; }
    public Dictionary<long, byte[]> Reverse { get; }
    public BlockDiff()
    {
        Forward = new Dictionary<long, byte[]>();
        Reverse = new Dictionary<long, byte[]>();
    }
}

public class VerkleDb
{
    private MemoryStateDb Storage { get; }
    private MemoryStateDb Batch { get; }
    private MemoryStateDb Cache { get; }
    private BlockDiff History { get; }

    public VerkleDb()
    {
        Storage = new MemoryStateDb();
        Batch = new MemoryStateDb();
        Cache = new MemoryStateDb();
        History = new BlockDiff();
    }

    public void InitRootHash()
    {
        Batch.BranchTable[Array.Empty<byte>()] = new BranchNode();
    }

    public byte[]? GetLeaf(byte[] key)
    {
        if (Cache.LeafTable.ContainsKey(key)) return Cache.LeafTable[key];
        if (Batch.LeafTable.ContainsKey(key)) return Batch.LeafTable[key];
        Storage.LeafTable.TryGetValue(key, out byte[]? value);
        return value;
    }

    public SuffixTree? GetStem(byte[] key)
    {
        if (Cache.StemTable.ContainsKey(key)) return Cache.StemTable[key];
        if (Batch.StemTable.ContainsKey(key)) return Batch.StemTable[key];
        Storage.StemTable.TryGetValue(key, out SuffixTree value);
        return value;
    }

    public InternalNode? GetBranch(byte[] key)
    {
        if (Cache.BranchTable.ContainsKey(key)) return Cache.BranchTable[key];
        if (Batch.BranchTable.ContainsKey(key)) return Batch.BranchTable[key];
        Storage.BranchTable.TryGetValue(key, out InternalNode? value);
        return value;
    }

    public void SetLeaf(byte[] leafKey, byte[] leafValue)
    {
        Cache.LeafTable[leafKey] = leafValue;
        Batch.LeafTable[leafKey] = leafValue;
    }

    public void SetStem(byte[] stemKey, SuffixTree suffixTree)
    {
        Cache.StemTable[stemKey] = suffixTree;
        Batch.StemTable[stemKey] = suffixTree;
    }

    public void SetBranch(byte[] branchKey, InternalNode internalNodeValue)
    {
        Cache.BranchTable[branchKey] = internalNodeValue;
        Batch.BranchTable[branchKey] = internalNodeValue;
    }
}
