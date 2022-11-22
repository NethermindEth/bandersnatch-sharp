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

    public byte[] Encode()
    {
        return Array.Empty<byte>();
    }

}

public class MemoryReverseStateDb
{
    public Dictionary<byte[], byte[]?> LeafTable { get; }
    public Dictionary<byte[], SuffixTree?> StemTable { get; }
    public Dictionary<byte[], InternalNode?> BranchTable { get; }

    public MemoryReverseStateDb()
    {
        LeafTable = new Dictionary<byte[], byte[]?>(new ByteArrayComparer());
        StemTable = new Dictionary<byte[], SuffixTree?>(new ByteArrayComparer());
        BranchTable = new Dictionary<byte[], InternalNode?>(new ByteArrayComparer());
    }

    public byte[] Encode()
    {
        return Array.Empty<byte>();
    }

}

public class DiffLayer
{
    public Dictionary<long, MemoryStateDb> Forward { get; }
    public Dictionary<long, MemoryReverseStateDb> Reverse { get; }
    public MemoryStateDb Batch { get; }
    public DiffLayer()
    {
        Forward = new Dictionary<long, MemoryStateDb>();
        Reverse = new Dictionary<long, MemoryReverseStateDb>();
        Batch = new MemoryStateDb();
    }
}

public class VerkleDb: IVerkleDb
{
    private long FullStateBlock { get; set; }
    private MemoryStateDb Storage { get; }
    // private MemoryStateDb InMemoryStateDbDiffLayer { get; }
    private MemoryStateDb Batch { get; }
    private MemoryStateDb Cache { get; }
    private DiffLayer History { get; }

    public VerkleDb()
    {
        Storage = new MemoryStateDb();
        // InMemoryStateDbDiffLayer = new MemoryStateDb();
        Batch = new MemoryStateDb();
        Cache = new MemoryStateDb();
        History = new DiffLayer();
        FullStateBlock = 0;
    }

    public void InitRootHash()
    {
        Batch.BranchTable[Array.Empty<byte>()] = new BranchNode();
    }

    public byte[]? GetLeaf(byte[] key)
    {
        if (Cache.LeafTable.ContainsKey(key)) return Cache.LeafTable[key];
        if (Batch.LeafTable.ContainsKey(key)) return Batch.LeafTable[key];
        // if (InMemoryStateDbDiffLayer.LeafTable.ContainsKey(key)) return InMemoryStateDbDiffLayer.LeafTable[key];
        Storage.LeafTable.TryGetValue(key, out byte[]? value);
        return value;
    }

    public SuffixTree? GetStem(byte[] key)
    {
        if (Cache.StemTable.ContainsKey(key)) return Cache.StemTable[key];
        if (Batch.StemTable.ContainsKey(key)) return Batch.StemTable[key];
        // if (InMemoryStateDbDiffLayer.StemTable.ContainsKey(key)) return InMemoryStateDbDiffLayer.StemTable[key];
        Storage.StemTable.TryGetValue(key, out SuffixTree value);
        return value;
    }

    public InternalNode? GetBranch(byte[] key)
    {
        if (Cache.BranchTable.ContainsKey(key)) return Cache.BranchTable[key];
        if (Batch.BranchTable.ContainsKey(key)) return Batch.BranchTable[key];
        // if (InMemoryStateDbDiffLayer.BranchTable.ContainsKey(key)) return InMemoryStateDbDiffLayer.BranchTable[key];
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

    public void Flush(long blockNumber)
    {
        MemoryReverseStateDb reverseDiff = new MemoryReverseStateDb();

        foreach (KeyValuePair<byte[], byte[]> entry in Batch.LeafTable)
        {
            if(Storage.LeafTable.TryGetValue(entry.Key, out byte[]? node)) reverseDiff.LeafTable[entry.Key] = node;
            else reverseDiff.LeafTable[entry.Key] = null;

            Storage.LeafTable[entry.Key] = entry.Value;
        }

        foreach (KeyValuePair<byte[], SuffixTree> entry in Batch.StemTable)
        {
            if(Storage.StemTable.TryGetValue(entry.Key, out SuffixTree node)) reverseDiff.StemTable[entry.Key] = node;
            else reverseDiff.StemTable[entry.Key] = null;

            Storage.StemTable[entry.Key] = entry.Value;
        }

        foreach (KeyValuePair<byte[], InternalNode> entry in Batch.BranchTable)
        {
            if(Storage.BranchTable.TryGetValue(entry.Key, out InternalNode? node)) reverseDiff.BranchTable[entry.Key] = node;
            else reverseDiff.BranchTable[entry.Key] = null;

            Storage.BranchTable[entry.Key] = entry.Value;
        }
        History.Forward[blockNumber] = Batch;
        History.Reverse[blockNumber] = reverseDiff;
        FullStateBlock = blockNumber;
    }

    public void ReverseState()
    {
        MemoryReverseStateDb reverseDiff = History.Reverse[FullStateBlock];

        foreach (KeyValuePair<byte[], byte[]?> entry in reverseDiff.LeafTable)
        {
            reverseDiff.LeafTable.TryGetValue(entry.Key, out byte[]? node);
            if (node is null)
            {
                Cache.LeafTable.Remove(entry.Key);
                Storage.LeafTable.Remove(entry.Key);
            }
            else
            {
                Cache.LeafTable[entry.Key] = node;
                Storage.LeafTable[entry.Key] = node;
            }
        }

        foreach (KeyValuePair<byte[], SuffixTree?> entry in reverseDiff.StemTable)
        {
            reverseDiff.StemTable.TryGetValue(entry.Key, out SuffixTree? node);
            if (node is null)
            {
                Cache.StemTable.Remove(entry.Key);
                Storage.StemTable.Remove(entry.Key);
            }
            else
            {
                Cache.StemTable[entry.Key] = node.Value;
                Storage.StemTable[entry.Key] = node.Value;
            }
        }

        foreach (KeyValuePair<byte[], InternalNode?> entry in reverseDiff.BranchTable)
        {
            reverseDiff.BranchTable.TryGetValue(entry.Key, out InternalNode? node);
            if (node is null)
            {
                Cache.BranchTable.Remove(entry.Key);
                Storage.BranchTable.Remove(entry.Key);
            }
            else
            {
                Cache.BranchTable[entry.Key] = node;
                Storage.BranchTable[entry.Key] = node;
            }
        }

    }
}

public interface IVerkleDb
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