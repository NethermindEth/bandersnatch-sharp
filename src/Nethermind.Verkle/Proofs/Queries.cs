// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using System.Text;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Fields.FrEElement;
using Nethermind.Verkle.Polynomial;

namespace Nethermind.Verkle.Proofs;

public readonly ref struct IpaProverQuery(
    Span<FrE> polynomial,
    Banderwagon commitment,
    FrE point,
    Span<FrE> pointEvaluations)
{
    public readonly Banderwagon Commitment = commitment;
    public readonly FrE Point = point;
    public readonly Span<FrE> PointEvaluations = pointEvaluations;
    public readonly Span<FrE> Polynomial = polynomial;
}

public class IpaVerifierQuery(
    Banderwagon commitment,
    FrE point,
    FrE[] pointEvaluations,
    FrE outputPoint,
    IpaProofStruct ipaProof)
{
    public readonly Banderwagon Commitment = commitment;
    public readonly IpaProofStruct IpaProof = ipaProof;
    public readonly FrE OutputPoint = outputPoint;
    public readonly FrE Point = point;
    public readonly FrE[] PointEvaluations = pointEvaluations;
}

public struct VerkleProverQuery
{
    public readonly LagrangeBasis ChildHashPoly;
    public readonly Banderwagon NodeCommitPoint;
    public readonly byte ChildIndex;
    public readonly FrE ChildHash;

    public VerkleProverQuery(LagrangeBasis childHashPoly, Banderwagon nodeCommitPoint, byte childIndex,
        FrE childHash)
    {
        ChildHashPoly = childHashPoly;
        NodeCommitPoint = nodeCommitPoint;
        ChildIndex = childIndex;
        ChildHash = childHash;
    }
}

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

public struct VerkleVerifierQuery
{
    public readonly Banderwagon NodeCommitPoint;
    public readonly byte ChildIndex;
    public readonly FrE ChildHash;

    public VerkleVerifierQuery(Banderwagon nodeCommitPoint, byte childIndex, FrE childHash)
    {
        NodeCommitPoint = nodeCommitPoint;
        ChildIndex = childIndex;
        ChildHash = childHash;
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