using Curve;
using Field;

namespace Verkle;

using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public class VerkleTree
{
    private readonly MemoryDb _db;
    public Commitment RootNode { get; private set; }

    public VerkleTree()
    {
        _db = new MemoryDb();
        RootNode = new Commitment();
    }
    
    private ref struct TraverseContext
    {
        public Banderwagon LeafUpdateDelta;
        public Span<byte> Key { get; }
        public Span<byte> Value { get; }
        public Span<byte> Stem => Key[..31];
        public int CurrentIndex { get; set; }

        public List<byte> CurrentPath;

        public TraverseContext(
            Span<byte> key,
            Span<byte> updateValue,
            Banderwagon point)
        {
            Key = key;
            if (updateValue.Length == 0)
            {
                updateValue = null;
            }

            Value = updateValue;
            CurrentIndex = 0;
            LeafUpdateDelta = point;
            CurrentPath = new();
        }
    }
}