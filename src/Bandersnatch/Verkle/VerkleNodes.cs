using Curve;
using Field;
using Nethermind.Int256;

namespace Verkle;

using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public enum NodeType : byte
{
    BranchNode,
    Stem,
    Suffix,
    Unknown
}

public struct Suffix: IVerkleNode
{
    public byte[] Key { get; set; }
    public bool IsSuffix => NodeType == NodeType.Suffix;
    public bool IsStem => NodeType == NodeType.Stem;
    public bool IsBranchNode => NodeType == NodeType.BranchNode;
    public readonly byte[] Stem;
    public readonly Commitment C1;
    public readonly Commitment C2;
    public readonly Commitment ExtensionCommitment;
    public NodeType NodeType;

    public Suffix(byte[] stem)
    {
        Stem = stem;
        C1 = new Commitment();
        C2 = new Commitment();
        ExtensionCommitment = new Commitment();
        NodeType = NodeType.BranchNode;
        Encoded = new byte[] { };
        Data = null;
        Key = new byte[] { };
    }

    public Fr UpdateCommitment(Banderwagon deltaLeafCommitment, byte index)
    {
        var prevCommit = ExtensionCommitment.PointAsField.Dup();
        Banderwagon deltaCommit;
        if (index < 128)
        {
            var oldValue = C1.PointAsField.Dup();
            C1.AddPoint(deltaLeafCommitment);
            var delta = C1.PointAsField - oldValue;
            deltaCommit = Committer.ScalarMul(delta, 2);
        }
        else
        {
            var oldValue = C2.PointAsField.Dup();
            C2.AddPoint(deltaLeafCommitment);
            var delta = C2.PointAsField - oldValue;
            deltaCommit = Committer.ScalarMul(delta, 3);
        }
        ExtensionCommitment.AddPoint(deltaCommit);
        return ExtensionCommitment.PointAsField - prevCommit;
    }

    public Commitment RecalculateSuffixCommitment(Func<Fr[], Banderwagon> verkleCommitter)
    {
        var c1Field = C1.PointAsField;
        var c2Field = C2.PointAsField;

        Fr[] extCommit = new Fr[]
        {
            Fr.One, Fr.FromBytes(Stem) ?? throw new ArgumentException(), c1Field, c2Field
        };

        ExtensionCommitment.Point = verkleCommitter(extCommit);
        return ExtensionCommitment;
    }

    NodeType IVerkleNode.NodeType
    {
        get => NodeType;
        set => NodeType = value;
    }

    public byte[]? Encoded { get; set; }
    public object? Data { get; set; }
    public Fr UpdateCommitment(Banderwagon deltaPoint, byte[] key, byte index)
    {
        throw new NotImplementedException();
    }

    public byte[] Serialize()
    {
        throw new NotImplementedException();
    }

    public IVerkleNode Deserialize(byte[] node)
    {
        throw new NotImplementedException();
    }
}

public struct InternalNode: IVerkleNode
{
    public byte[] Key { get; set; }
    public bool IsSuffix => NodeType == NodeType.Suffix;
    public bool IsStem => NodeType == NodeType.Stem;
    public bool IsBranchNode => NodeType == NodeType.BranchNode;
    public Commitment InternalCommitment;

    private byte[]? _stem = null;
    public NodeType NodeType;

    public byte[]? Stem
    {
        get => _stem;
        set
        {
            if (IsBranchNode) throw new ArgumentException();
            _stem = value;
        }
    }

    public InternalNode(byte[] stem, Commitment suffixCommitment)
    {
        NodeType = NodeType.Stem; 
        _stem = stem;
        Encoded = new byte[] { };
        Data = null;
        Key = new byte[] { };
        InternalCommitment = suffixCommitment;
    }

    public InternalNode(bool isInternal)
    {
        NodeType = isInternal ? NodeType.BranchNode : NodeType.Stem;
        NodeType = NodeType.BranchNode;
        Encoded = new byte[] { };
        Data = null;
        Key = new byte[] { };
        InternalCommitment = new Commitment();
    }
    
    public Fr UpdateCommitment(Fr deltaHash, byte index)
    {
        Banderwagon point = Committer.ScalarMul(deltaHash, index);
        Fr prevCommit = InternalCommitment.PointAsField.Dup();
        InternalCommitment.AddPoint(point);
        return InternalCommitment.PointAsField - prevCommit;
    }
    
    public Fr UpdateCommitment(Banderwagon point)
    {
        Fr prevCommit = InternalCommitment.PointAsField.Dup();
        InternalCommitment.AddPoint(point);
        return InternalCommitment.PointAsField - prevCommit;
    }
    
    public Fr UpdateCommitment(Banderwagon[] points)
    {
        Fr prevCommit = InternalCommitment.PointAsField.Dup();
        foreach (var point in points)
        {
            InternalCommitment.AddPoint(point);
        }
        return InternalCommitment.PointAsField - prevCommit;
    }
    
    NodeType IVerkleNode.NodeType
    {
        get => NodeType;
        set => NodeType = value;
    }

    public byte[]? Encoded { get; set; }
    public object? Data { get; set; }
    public Fr UpdateCommitment(Banderwagon deltaPoint, byte[] key, byte index)
    {
        throw new NotImplementedException();
    }

    public byte[] Serialize()
    {
        throw new NotImplementedException();
    }

    public IVerkleNode Deserialize(byte[] node)
    {
        throw new NotImplementedException();
    }

    public void ChangeToBranch()
    {
        _stem = null;
        NodeType = NodeType.BranchNode;
    }
}


public struct VerkleValue
{
    public static Fr ValueExistsMarker
    {
        get
        {
            new UInt256(2).Exp(128, out var res);
            return new Fr(res);
        }
    }
    
    public static (Fr ,Fr) LowHigh(byte[]? value)
    {
        if (value is null) return (Fr.Zero, Fr.Zero);
        if (value.Length != 32) throw new ArgumentException();
        var lowFr = (Fr.FromBytes(value[..16]) ?? throw new ArgumentException()) + ValueExistsMarker;
        var highFr = Fr.FromBytes(value[16..]) ?? throw new AggregateException();
        return (lowFr, highFr);
    }

    public static Fr ValueExistMarker()
    {
        new UInt256(2).Exp(128, out var res);
        return new Fr(res);
    }
}

public interface IVerkleNode
{
    public NodeType NodeType { get; protected internal set; }
    byte[] Key { get; set; }
    
    public bool IsSuffix => NodeType == NodeType.Suffix;
    public bool IsStem => NodeType == NodeType.Stem;
    public bool IsBranchNode => NodeType == NodeType.BranchNode;
    
    public byte[]? Encoded {get; set;}
    public object? Data {get; set;}
    Fr UpdateCommitment(Banderwagon deltaPoint, byte[] key, byte index);
    byte[] Serialize();
    IVerkleNode Deserialize(byte[] node);
}
