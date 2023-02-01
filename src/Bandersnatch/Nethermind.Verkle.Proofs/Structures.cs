// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using Nethermind.Field.Montgomery.FrEElement;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Polynomial;

namespace Nethermind.Verkle.Proofs
{
    public struct IpaProverQuery
    {
        public FrE[] Polynomial;
        public Banderwagon Commitment;
        public FrE Point;
        public FrE[] PointEvaluations;

        public IpaProverQuery(FrE[] polynomial, Banderwagon commitment, FrE point,
            FrE[] pointEvaluations)
        {
            Polynomial = polynomial;
            Commitment = commitment;
            Point = point;
            PointEvaluations = pointEvaluations;
        }
    }

    public struct IpaProofStruct
    {
        public List<Banderwagon> L;
        public FrE A;
        public List<Banderwagon> R;

        public IpaProofStruct(List<Banderwagon> l, FrE a, List<Banderwagon> r)
        {
            L = l;
            A = a;
            R = r;
        }
    }

    public struct IpaVerifierQuery
    {
        public Banderwagon Commitment;
        public FrE Point;
        public FrE[] PointEvaluations;
        public FrE OutputPoint;
        public IpaProofStruct _ipaProof;

        public IpaVerifierQuery(Banderwagon commitment, FrE point, FrE[] pointEvaluations, FrE outputPoint, IpaProofStruct ipaProof)
        {
            Commitment = commitment;
            Point = point;
            PointEvaluations = pointEvaluations;
            OutputPoint = outputPoint;
            _ipaProof = ipaProof;
        }
    }

    public struct VerkleProofStruct
    {
        public IpaProofStruct _ipaProof;
        public Banderwagon D;

        public VerkleProofStruct(IpaProofStruct ipaProof, Banderwagon d)
        {
            _ipaProof = ipaProof;
            D = d;
        }
    }

    public struct VerkleProverQuery
    {
        public LagrangeBasis f;
        public Banderwagon C;
        public FrE z;
        public FrE y;

        public VerkleProverQuery(LagrangeBasis _f, Banderwagon _C, FrE _z, FrE _y)
        {
            f = _f;
            C = _C;
            z = _z;
            y = _y;
        }
    }

    public struct VerkleVerifierQuery
    {
        public Banderwagon C;
        public FrE z;
        public FrE y;

        public VerkleVerifierQuery(Banderwagon _C, FrE _z, FrE _y)
        {
            C = _C;
            z = _z;
            y = _y;
        }
    }
}
