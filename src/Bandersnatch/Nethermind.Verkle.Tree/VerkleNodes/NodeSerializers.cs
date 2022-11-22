// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

using Nethermind.Serialization.Rlp;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Utils;

namespace Nethermind.Verkle.Tree.VerkleNodes;
using LeafStore = Dictionary<byte[], byte[]?>;
using SuffixStore = Dictionary<byte[], SuffixTree?>;
using BranchStore = Dictionary<byte[], InternalNode?>;

public class SuffixTreeSerializer : IRlpStreamDecoder<SuffixTree>, IRlpObjectDecoder<SuffixTree>
{
    public static SuffixTreeSerializer Instance => new SuffixTreeSerializer();
    public int GetLength(SuffixTree item, RlpBehaviors rlpBehaviors)
    {
        return 31 + 32 + 32 + 32;
    }

    public int GetLength(SuffixTree item, RlpBehaviors rlpBehaviors, out int contentLength)
    {
        contentLength = GetLength(item, rlpBehaviors);
        return Rlp.LengthOfSequence(contentLength);
    }

    public SuffixTree Decode(RlpStream rlpStream, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
    {
        byte[] stem = rlpStream.Read(31).ToArray();
        byte[] c1 = rlpStream.Read(32).ToArray();
        byte[] c2 = rlpStream.Read(32).ToArray();
        byte[] extCommit = rlpStream.Read(32).ToArray();
        return new SuffixTree(stem, c1, c2, extCommit);
    }
    public void Encode(RlpStream stream, SuffixTree item, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
    {
        stream.Write(item.Stem);
        stream.Write(item.C1.Point.ToBytes());
        stream.Write(item.C2.Point.ToBytes());
        stream.Write(item.ExtensionCommitment.Point.ToBytes());
    }
    public Rlp Encode(SuffixTree? item, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
    {
        int length = GetLength(item, rlpBehaviors);
        RlpStream stream = new RlpStream(Rlp.LengthOfSequence(length));
        stream.StartSequence(length);
        Encode(stream, item, rlpBehaviors);
        return new Rlp(stream.Data);
    }

    public SuffixTree Decode(byte[] data, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
    {
        RlpStream stream = data.AsRlpStream();
        stream.ReadSequenceLength();
        return Decode(stream, rlpBehaviors);
    }
}

public class InternalNodeSerializer : IRlpStreamDecoder<InternalNode>, IRlpObjectDecoder<InternalNode>
{
    public static InternalNodeSerializer Instance => new InternalNodeSerializer();
    public int GetLength(InternalNode item, RlpBehaviors rlpBehaviors)
    {
        return item.NodeType switch
        {
            NodeType.BranchNode => 32 + 1,
            NodeType.StemNode => 32 + 31 + 1,
            var _ => throw new ArgumentOutOfRangeException()
        };
    }

    public InternalNode Decode(RlpStream rlpStream, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
    {
        NodeType nodeType = (NodeType)rlpStream.ReadByte();
        switch (nodeType)
        {
            case NodeType.BranchNode:
                InternalNode node = new InternalNode(NodeType.BranchNode);
                node.UpdateCommitment(new Banderwagon(rlpStream.Read(32).ToArray()));
                return node;
            case NodeType.StemNode:
                return new InternalNode(NodeType.StemNode, rlpStream.Read(31).ToArray(), new Commitment(new Banderwagon(rlpStream.Read(32).ToArray())));
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public void Encode(RlpStream stream, InternalNode item, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
    {
        switch (item.NodeType)
        {
            case NodeType.BranchNode:
                stream.WriteByte((byte)NodeType.BranchNode);
                stream.Write(item._internalCommitment.Point.ToBytes());
                break;
            case NodeType.StemNode:
                stream.WriteByte((byte)NodeType.StemNode);
                stream.Write(item.Stem);
                stream.Write(item._internalCommitment.Point.ToBytes());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public Rlp Encode(InternalNode item, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
    {
        int length = GetLength(item, rlpBehaviors);
        RlpStream stream = new RlpStream(Rlp.LengthOfSequence(length));
        stream.StartSequence(length);
        Encode(stream, item, rlpBehaviors);
        return new Rlp(stream.Data);
    }
    public InternalNode Decode(byte[] data, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
    {
        RlpStream stream = data.AsRlpStream();
        stream.ReadSequenceLength();
        return Decode(stream, rlpBehaviors);
    }

}
