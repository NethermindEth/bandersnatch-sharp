// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

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
