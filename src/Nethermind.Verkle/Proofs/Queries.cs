// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Fields.FrEElement;
using Nethermind.Verkle.Polynomial;

namespace Nethermind.Verkle.Proofs;

public class IpaProverQuery
{
    public readonly FrE[] Polynomial;
    public readonly Banderwagon Commitment;
    public readonly FrE Point;
    public readonly FrE[] PointEvaluations;

    public IpaProverQuery(FrE[] polynomial, Banderwagon commitment, FrE point,
        FrE[] pointEvaluations)
    {
        Polynomial = polynomial;
        Commitment = commitment;
        Point = point;
        PointEvaluations = pointEvaluations;
    }
}

public class IpaVerifierQuery
{
    public readonly Banderwagon Commitment;
    public readonly FrE Point;
    public readonly FrE[] PointEvaluations;
    public readonly FrE OutputPoint;
    public readonly IpaProofStruct IpaProof;

    public IpaVerifierQuery(Banderwagon commitment, FrE point, FrE[] pointEvaluations, FrE outputPoint,
        IpaProofStruct ipaProof)
    {
        Commitment = commitment;
        Point = point;
        PointEvaluations = pointEvaluations;
        OutputPoint = outputPoint;
        IpaProof = ipaProof;
    }
}



public struct VerkleProverQuery
{
    public readonly LagrangeBasis ChildHashPoly;
    public readonly Banderwagon NodeCommitPoint;
    public readonly FrE ChildIndex;
    public readonly FrE ChildHash;

    public VerkleProverQuery(LagrangeBasis childHashPoly, Banderwagon nodeCommitPoint, FrE childIndex,
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
    public readonly FrE ChildIndex;
    public readonly FrE ChildHash;

    public VerkleVerifierQuery(Banderwagon nodeCommitPoint, FrE childIndex, FrE childHash)
    {
        NodeCommitPoint = nodeCommitPoint;
        ChildIndex = childIndex;
        ChildHash = childHash;
    }
}