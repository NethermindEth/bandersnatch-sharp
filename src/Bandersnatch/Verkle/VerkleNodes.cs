using System.Diagnostics;
using Curve;
using Field;

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

    public Fr InitCommitmentHash = Fr.Zero;

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
        SetInitialCommitment();
    }

    private void SetInitialCommitment()
    {
        Banderwagon stemCommitment = Committer.ScalarMul(Fr.One, 0) +
                                     Committer.ScalarMul(Fr.FromBytesReduced(Stem.Reverse().ToArray()), 1);
        ExtensionCommitment.AddPoint(stemCommitment);
        InitCommitmentHash = ExtensionCommitment.PointAsField.Dup();
    }

    public Fr UpdateCommitment(LeafUpdateDelta deltaLeafCommitment)
    {
        Fr prevCommit = ExtensionCommitment.PointAsField.Dup();
        
        Fr oldC1Value = C1.PointAsField.Dup();
        Fr oldC2Value = C2.PointAsField.Dup();
        if(deltaLeafCommitment.DeltaC1 is not null) C1.AddPoint(deltaLeafCommitment.DeltaC1);
        if(deltaLeafCommitment.DeltaC2 is not null) C2.AddPoint(deltaLeafCommitment.DeltaC2);

        Fr deltaC1Commit = C1.PointAsField - oldC1Value;
        Fr deltaC2Commit = C2.PointAsField - oldC2Value;

        Banderwagon deltaCommit = Committer.ScalarMul(deltaC1Commit, 2) 
                                  + Committer.ScalarMul(deltaC2Commit, 3);
        
        ExtensionCommitment.AddPoint(deltaCommit);
        return ExtensionCommitment.PointAsField - prevCommit;
    }

    public Commitment RecalculateSuffixCommitment(Func<Fr[], Banderwagon> verkleCommitter)
    {
        Fr c1Field = C1.PointAsField;
        Fr c2Field = C2.PointAsField;

        Fr[] extCommit = {
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
    public byte[] Key
    {
        get
        {
            Debug.Assert(_stem != null, nameof(_stem) + " != null");
            return _stem;
        }
        set => _stem = value;
    }

    public bool IsSuffix => NodeType == NodeType.Suffix;
    public bool IsStem => NodeType == NodeType.Stem;
    public bool IsBranchNode => NodeType == NodeType.BranchNode;
    public readonly Commitment InternalCommitment;

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
        InternalCommitment = suffixCommitment;
    }

    public InternalNode(bool isInternal)
    {
        NodeType = isInternal ? NodeType.BranchNode : NodeType.Stem;
        NodeType = NodeType.BranchNode;
        Encoded = new byte[] { };
        Data = null;
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
        foreach (Banderwagon point in points)
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
