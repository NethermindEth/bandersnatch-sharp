// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using System.Text;
using Nethermind.Verkle.Fields.FrEElement;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Polynomial;

namespace Nethermind.Verkle.Proofs
{
    public class IpaProverQuery
    {
        public FrE[] Polynomial { get; }
        public Banderwagon Commitment { get; }
        public FrE Point { get; }
        public FrE[] PointEvaluations { get; }

        public IpaProverQuery(FrE[] polynomial, Banderwagon commitment, FrE point,
            FrE[] pointEvaluations)
        {
            Polynomial = polynomial;
            Commitment = commitment;
            Point = point;
            PointEvaluations = pointEvaluations;
        }
    }

    public class IpaProofStruct
    {
        public Banderwagon[] L { get; }
        public FrE A { get; }
        public Banderwagon[] R { get; }

        public IpaProofStruct(Banderwagon[] l, FrE a, Banderwagon[] r)
        {
            L = l;
            A = a;
            R = r;
        }

        public byte[] Encode()
        {
            List<byte> encoded = new List<byte>();

            foreach (Banderwagon l in L)
            {
                encoded.AddRange(l.ToBytesLittleEndian().Reverse().ToArray());
            }

            foreach (Banderwagon r in R)
            {
                encoded.AddRange(r.ToBytesLittleEndian().Reverse().ToArray());
            }

            encoded.AddRange(A.ToBytes().ToArray());

            return encoded.ToArray();
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("\n#[_l]#\n");
            foreach (Banderwagon l in L)
            {
                stringBuilder.AppendJoin(", ", l.ToBytesLittleEndian().Reverse().ToArray());
                stringBuilder.Append('\n');
            }
            stringBuilder.Append("\n#[_a]#\n");
            stringBuilder.AppendJoin(", ", A.ToBytes().ToArray());
            stringBuilder.Append("\n#[_r]#\n");
            foreach (Banderwagon l in R)
            {
                stringBuilder.AppendJoin(", ", l.ToBytesLittleEndian().Reverse().ToArray());
                stringBuilder.Append('\n');
            }
            return stringBuilder.ToString();
        }
    }

    public class IpaVerifierQuery
    {
        public Banderwagon Commitment { get; }
        public FrE Point { get; }
        public FrE[] PointEvaluations { get; }
        public FrE OutputPoint { get; }
        public IpaProofStruct IpaProof { get; }

        public IpaVerifierQuery(Banderwagon commitment, FrE point, FrE[] pointEvaluations, FrE outputPoint, IpaProofStruct ipaProof)
        {
            Commitment = commitment;
            Point = point;
            PointEvaluations = pointEvaluations;
            OutputPoint = outputPoint;
            IpaProof = ipaProof;
        }
    }

    public class VerkleProofStruct
    {
        public IpaProofStruct IpaProof { get; }
        public Banderwagon D { get; }

        public VerkleProofStruct(IpaProofStruct ipaProof, Banderwagon d)
        {
            IpaProof = ipaProof;
            D = d;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("\n##[IPA Proof]##\n");
            stringBuilder.Append(IpaProof.ToString());
            stringBuilder.Append("\n##[_d]##\n");
            stringBuilder.AppendJoin(", ", D.ToBytesLittleEndian().Reverse().ToArray());
            return stringBuilder.ToString();
        }

        public byte[] Encode()
        {
            List<byte> encoded = new List<byte>();

            encoded.AddRange(D.ToBytesLittleEndian().Reverse().ToArray());
            encoded.AddRange(IpaProof.Encode());

            return encoded.ToArray();
        }
    }

    public struct VerkleProverQuery
    {
        public LagrangeBasis ChildHashPoly { get; }
        public Banderwagon NodeCommitPoint { get; }
        public FrE ChildIndex { get; }
        public FrE ChildHash { get; }

        public VerkleProverQuery(LagrangeBasis childHashPoly, Banderwagon nodeCommitPoint, FrE childIndex, FrE childHash)
        {
            ChildHashPoly = childHashPoly;
            NodeCommitPoint = nodeCommitPoint;
            ChildIndex = childIndex;
            ChildHash = childHash;
        }
    }

    public struct VerkleVerifierQuery
    {
        public Banderwagon NodeCommitPoint { get; }
        public FrE ChildIndex { get; }
        public FrE ChildHash { get; }

        public VerkleVerifierQuery(Banderwagon nodeCommitPoint, FrE childIndex, FrE childHash)
        {
            NodeCommitPoint = nodeCommitPoint;
            ChildIndex = childIndex;
            ChildHash = childHash;
        }
    }
}
