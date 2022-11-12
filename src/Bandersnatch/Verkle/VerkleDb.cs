
namespace Verkle;

public struct MemoryDb
{
    public Dictionary<byte[], byte[]> LeafTable;
    public Dictionary<byte[], Suffix> StemTable;
    public Dictionary<byte[], InternalNode> BranchTable;

    public MemoryDb()
    {
        LeafTable = new Dictionary<byte[], byte[]>();
        StemTable = new Dictionary<byte[], Suffix>();
        BranchTable = new Dictionary<byte[], InternalNode>();
    }
}