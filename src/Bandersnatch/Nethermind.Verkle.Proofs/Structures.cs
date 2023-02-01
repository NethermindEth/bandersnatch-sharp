// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using Nethermind.Field.Montgomery.FrEElement;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Polynomial;

namespace Nethermind.Verkle.Proofs
{
    public struct IpaProverQuery
    {
        public readonly FrE[] _polynomial;
        public readonly Banderwagon _commitment;
        public FrE _point;
        public readonly FrE[] _pointEvaluations;

        public IpaProverQuery(FrE[] polynomial, Banderwagon commitment, FrE point,
            FrE[] pointEvaluations)
        {
            _polynomial = polynomial;
            _commitment = commitment;
            _point = point;
            _pointEvaluations = pointEvaluations;
        }
    }

    public struct IpaProofStruct
    {
        public readonly List<Banderwagon> _l;
        public FrE _a;
        public readonly List<Banderwagon> _r;

        public IpaProofStruct(List<Banderwagon> l, FrE a, List<Banderwagon> r)
        {
            _l = l;
            _a = a;
            _r = r;
        }
    }

    public struct IpaVerifierQuery
    {
        public readonly Banderwagon _commitment;
        public FrE _point;
        public readonly FrE[] _pointEvaluations;
        public FrE _outputPoint;
        public IpaProofStruct _ipaProof;

        public IpaVerifierQuery(Banderwagon commitment, FrE point, FrE[] pointEvaluations, FrE outputPoint, IpaProofStruct ipaProof)
        {
            _commitment = commitment;
            _point = point;
            _pointEvaluations = pointEvaluations;
            _outputPoint = outputPoint;
            _ipaProof = ipaProof;
        }
    }

    public struct VerkleProofStruct
    {
        public IpaProofStruct _ipaProof;
        public readonly Banderwagon _d;

        public VerkleProofStruct(IpaProofStruct ipaProof, Banderwagon d)
        {
            _ipaProof = ipaProof;
            _d = d;
        }
    }

    public struct VerkleProverQuery
    {
        public readonly LagrangeBasis _childHashPoly;
        public readonly Banderwagon _nodeCommitPoint;
        public FrE _childIndex;
        public FrE _childHash;

        public VerkleProverQuery(LagrangeBasis childHashPoly, Banderwagon nodeCommitPoint, FrE childIndex, FrE childHash)
        {
            _childHashPoly = childHashPoly;
            _nodeCommitPoint = nodeCommitPoint;
            _childIndex = childIndex;
            _childHash = childHash;
        }
    }

    public struct VerkleVerifierQuery
    {
        public readonly Banderwagon _nodeCommitPoint;
        public FrE _childIndex;
        public FrE _childHash;

        public VerkleVerifierQuery(Banderwagon nodeCommitPoint, FrE childIndex, FrE childHash)
        {
            _nodeCommitPoint = nodeCommitPoint;
            _childIndex = childIndex;
            _childHash = childHash;
        }
    }
}
