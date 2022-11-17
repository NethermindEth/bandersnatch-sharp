using System.Diagnostics;
using Nethermind.Field;
using Nethermind.Field.Montgomery;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Utils;

namespace Nethermind.Verkle.Tree;

public enum NodeType : byte
{
    BranchNode = 1,
    StemNode = 2
}

public class SuffixTree
{
    public byte[] Stem { get; }
    public Commitment C1 { get; }
    public Commitment C2 { get; }
    public Commitment ExtensionCommitment { get; }
    public FrE InitCommitmentHash { get; }

    public SuffixTree(byte[] stem)
    {
        Stem = stem;
        C1 = new Commitment();
        C2 = new Commitment();
        ExtensionCommitment = new Commitment();
        InitCommitmentHash = FrE.Zero;
        Banderwagon stemCommitment = GetInitialCommitment();
        ExtensionCommitment.AddPoint(stemCommitment);
        InitCommitmentHash = ExtensionCommitment.PointAsField.Dup();
    }

    internal SuffixTree(byte[] stem, byte[] c1, byte[] c2, byte[] extCommit)
    {
        Stem = stem;
        C1 = new Commitment(new Banderwagon(c1));
        C2 = new Commitment(new Banderwagon(c2));
        ExtensionCommitment = new Commitment(new Banderwagon(extCommit));
        InitCommitmentHash = FrE.Zero;
    }

    private Banderwagon GetInitialCommitment() => Committer.ScalarMul(FrE.One, 0) +
                                                  Committer.ScalarMul(FrE.FromBytesReduced(Stem.Reverse().ToArray()), 1);

    public FrE UpdateCommitment(LeafUpdateDelta deltaLeafCommitment)
    {
        FrE prevCommit = ExtensionCommitment.PointAsField.Dup();

        FrE oldC1Value = C1.PointAsField.Dup();
        FrE oldC2Value = C2.PointAsField.Dup();
        if (deltaLeafCommitment.DeltaC1 is not null) C1.AddPoint(deltaLeafCommitment.DeltaC1);
        if (deltaLeafCommitment.DeltaC2 is not null) C2.AddPoint(deltaLeafCommitment.DeltaC2);

        FrE deltaC1Commit = C1.PointAsField - oldC1Value;
        FrE deltaC2Commit = C2.PointAsField - oldC2Value;

        Banderwagon deltaCommit = Committer.ScalarMul(deltaC1Commit, 2)
                                  + Committer.ScalarMul(deltaC2Commit, 3);

        ExtensionCommitment.AddPoint(deltaCommit);
        return ExtensionCommitment.PointAsField - prevCommit;
    }

    public byte[] Encode()
    {
        int nodeLength = 31 + 32 + 32 + 32;
        byte[] rlp = new byte[nodeLength];
        Buffer.BlockCopy(Stem, 0, rlp, 0, 31);
        Buffer.BlockCopy(C1.Point.ToBytes(), 0, rlp, 31, 32);
        Buffer.BlockCopy(C2.Point.ToBytes(), 0, rlp, 63, 32);
        Buffer.BlockCopy(ExtensionCommitment.Point.ToBytes(), 0, rlp, 95, 32);
        return rlp;
    }

    public static SuffixTree Decode(byte[] rlp)
    {
        return new SuffixTree(rlp[..31], rlp[32..64], rlp[64..96], rlp[96..128]);
    }
}

public class StemNode : InternalNode
{
    public StemNode(byte[] stem, Commitment suffixCommitment) : base(NodeType.StemNode, stem, suffixCommitment)
    {
    }
}

public class BranchNode : InternalNode
{
    public BranchNode() : base(NodeType.BranchNode)
    {
    }
}

public class InternalNode
{
    public bool IsStem => NodeType == NodeType.StemNode;
    public bool IsBranchNode => NodeType == NodeType.BranchNode;

    public readonly Commitment _internalCommitment;

    public readonly NodeType NodeType;

    private byte[]? _stem;
    public byte[] Stem
    {
        get
        {
            Debug.Assert(_stem != null, nameof(_stem) + " != null");
            return _stem;
        }
    }

    public InternalNode(NodeType nodeType, byte[] stem, Commitment suffixCommitment)
    {
        switch (nodeType)
        {
            case NodeType.StemNode:
                NodeType = NodeType.StemNode;
                _stem = stem;
                _internalCommitment = suffixCommitment;
                break;
            case NodeType.BranchNode:
            default:
                throw new ArgumentOutOfRangeException(nameof(nodeType), nodeType, null);
        }
    }

    public InternalNode(NodeType nodeType)
    {
        switch (nodeType)
        {
            case NodeType.BranchNode:
                break;
            case NodeType.StemNode:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(nodeType), nodeType, null);
        }
        NodeType = nodeType;
        _internalCommitment = new Commitment();
    }
    public FrE UpdateCommitment(Banderwagon point)
    {
        FrE prevCommit = _internalCommitment.PointAsField.Dup();
        _internalCommitment.AddPoint(point);
        return _internalCommitment.PointAsField - prevCommit;
    }

    public byte[] Encode()
    {
        int nodeLength;
        byte[] rlp;
        switch (NodeType)
        {
            case NodeType.BranchNode:
                nodeLength = 32 + 1;
                rlp = new byte[nodeLength];
                rlp[0] = (byte)NodeType;
                Buffer.BlockCopy(_internalCommitment.Point.ToBytes(), 0, rlp, 1, 32);
                return rlp;
            case NodeType.StemNode:
                nodeLength = 32 + 31 + 1;
                rlp = new byte[nodeLength];
                rlp[0] = (byte)NodeType;
                Buffer.BlockCopy(_stem, 0, rlp, 1, 32);
                Buffer.BlockCopy(_internalCommitment.Point.ToBytes(), 0, rlp, 32, 32);
                return rlp;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static InternalNode Decode(byte[] rlp)
    {
        NodeType nodeType = (NodeType)rlp[0];
        switch (nodeType)
        {
            case NodeType.BranchNode:
                InternalNode node = new InternalNode(nodeType);
                node.UpdateCommitment(new Banderwagon(rlp[1..]));
                return node;
            case NodeType.StemNode:
                return new InternalNode(NodeType.StemNode, rlp[1..32], new Commitment(new Banderwagon(rlp[32..])));
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
