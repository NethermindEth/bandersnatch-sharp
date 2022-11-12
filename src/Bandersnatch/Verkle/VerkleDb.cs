namespace Verkle;

public class ByteArrayComparer : IEqualityComparer<byte[]>
{
    public bool Equals(byte[]? x, byte[]? y)
    {
        if ( x == null || y == null ) {
            return x == y;
        }
        return x.SequenceEqual(y);
    }

    public int GetHashCode(byte[] value)
    {
        var hash = new HashCode();
        hash.AddBytes(value);
        return hash.ToHashCode();
    }
}

public struct MemoryDb
{
    public readonly Dictionary<byte[], byte[]> LeafTable;
    public readonly Dictionary<byte[], Suffix> StemTable;
    public readonly Dictionary<byte[], InternalNode> BranchTable;

    public MemoryDb()
    {
        LeafTable = new Dictionary<byte[], byte[]>(new ByteArrayComparer());
        StemTable = new Dictionary<byte[], Suffix>(new ByteArrayComparer());
        BranchTable = new Dictionary<byte[], InternalNode>(new ByteArrayComparer());
    }
}