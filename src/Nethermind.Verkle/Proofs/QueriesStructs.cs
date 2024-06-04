using System.Text;
using Nethermind.Verkle.Fields.FrEElement;
using Nethermind.Verkle.Proofs;

public readonly struct VerkleProverQuerySerialized(byte[][] childHashPoly, byte[] nodeCommitPoint, byte childIndex, byte[] childHash)
{
    public readonly byte[][] ChildHashPoly = childHashPoly;
    public readonly byte[] NodeCommitPoint = nodeCommitPoint;
    public readonly byte ChildIndex = childIndex;
    public readonly byte[] ChildHash = childHash;

    public static VerkleProverQuerySerialized CreateProverQuerySerialized(VerkleProverQuery query)
    {
        byte[] nodeCommitPoint = query.NodeCommitPoint.ToBytesUncompressed();
        List<byte[]> childHashPoly = new List<byte[]>();
        foreach (FrE eval in query.ChildHashPoly.Evaluations)
        {
            childHashPoly.Add(eval.ToBytes());
        }
        byte childIndex = query.ChildIndex;
        byte[] childHash = query.ChildHash.ToBytes();

        return new VerkleProverQuerySerialized(childHashPoly.ToArray(), nodeCommitPoint, childIndex, childHash);
    }

    public byte[] Encode()
    {
        List<byte> encoded = [.. NodeCommitPoint];
        foreach (byte[] eval in ChildHashPoly) encoded.AddRange(eval);
        encoded.Add(ChildIndex);
        encoded.AddRange(ChildHash);
        return encoded.ToArray();
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new();
        stringBuilder.Append("\n#[_ChildHashPoly]#\n");
        foreach (byte[] eval in ChildHashPoly)
        {
            stringBuilder.AppendJoin(", ", eval);
            stringBuilder.Append('\n');
        }
        stringBuilder.Append("\n#[_NodeCommitPoint]#\n");
        stringBuilder.AppendJoin(", ", NodeCommitPoint);
        stringBuilder.Append("\n#[_ChildIndex]#\n");
        stringBuilder.AppendJoin(", ", ChildIndex);
        stringBuilder.Append("\n#[_ChildHash]#\n");
        stringBuilder.AppendJoin(", ", ChildHash);
        return stringBuilder.ToString();
    }
}

public readonly struct VerkleVerifierQuerySerialized(byte[] NodeCommitPoint, byte ChildIndex, byte[] ChildHash)
{
    public readonly byte[] NodeCommitPoint = NodeCommitPoint;
    public readonly byte ChildIndex = ChildIndex;
    public readonly byte[] ChildHash = ChildHash;

    public byte[] Encode()
    {
        List<byte> encoded = [];
        encoded.AddRange(NodeCommitPoint);
        encoded.Add(ChildIndex);
        encoded.AddRange(ChildHash);
        return encoded.ToArray();
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new();
        stringBuilder.Append("\n#[_NodeCommitPoint]#\n");
        stringBuilder.AppendJoin(", ", NodeCommitPoint);
        stringBuilder.Append("\n#[_ChildIndex]#\n");
        stringBuilder.AppendJoin(", ", ChildIndex);
        stringBuilder.Append("\n#[_ChildHash]#\n");
        stringBuilder.AppendJoin(", ", ChildHash);
        return stringBuilder.ToString();
    }
}