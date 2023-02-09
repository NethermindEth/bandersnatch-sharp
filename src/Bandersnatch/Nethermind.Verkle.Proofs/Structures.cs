// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using System.Text;
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

        public byte[] Encode()
        {
            List<byte> encoded = new List<byte>();

            foreach (Banderwagon l in _l)
            {
                encoded.AddRange(l.ToBytesLittleEndian().Reverse().ToArray());
            }

            foreach (Banderwagon r in _r)
            {
                encoded.AddRange(r.ToBytesLittleEndian().Reverse().ToArray());
            }

            encoded.AddRange(_a.ToBytes().ToArray());

            return encoded.ToArray();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("\n#[_l]#\n");
            foreach (Banderwagon l in _l)
            {
                stringBuilder.AppendJoin(", ", l.ToBytesLittleEndian().Reverse().ToArray());
                stringBuilder.Append('\n');
            }
            stringBuilder.Append("\n#[_a]#\n");
            stringBuilder.AppendJoin(", ", _a.ToBytes().ToArray());
            stringBuilder.Append("\n#[_r]#\n");
            foreach (Banderwagon l in _r)
            {
                stringBuilder.AppendJoin(", ", l.ToBytesLittleEndian().Reverse().ToArray());
                stringBuilder.Append('\n');
            }
            return stringBuilder.ToString();
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

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("\n##[IPA Proof]##\n");
            stringBuilder.Append(_ipaProof.ToString());
            stringBuilder.Append("\n##[_d]##\n");
            stringBuilder.AppendJoin(", ", _d.ToBytesLittleEndian().Reverse().ToArray());
            return stringBuilder.ToString();
        }

        public byte[] Encode()
        {
            List<byte> encoded = new List<byte>();

            encoded.AddRange(_d.ToBytesLittleEndian().Reverse().ToArray());
            encoded.AddRange(_ipaProof.Encode());

            return encoded.ToArray();
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
