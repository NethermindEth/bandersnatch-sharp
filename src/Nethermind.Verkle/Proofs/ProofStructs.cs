// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using System.Text;
using Nethermind.Verkle.Fields.FrEElement;
using Nethermind.Verkle.Curve;

namespace Nethermind.Verkle.Proofs;

public class IpaProofStruct
{
    public readonly Banderwagon[] L;
    public readonly FrE A;
    public readonly Banderwagon[] R;

    public IpaProofStruct(Banderwagon[] l, FrE a, Banderwagon[] r)
    {
        L = l;
        A = a;
        R = r;
    }

    public byte[] Encode()
    {
        List<byte> encoded = new();

        foreach (Banderwagon l in L)
        {
            encoded.AddRange(l.ToBytes());
        }

        foreach (Banderwagon r in R)
        {
            encoded.AddRange(r.ToBytes());
        }

        encoded.AddRange(A.ToBytes());

        return encoded.ToArray();
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("\n#[_l]#\n");
        foreach (Banderwagon l in L)
        {
            stringBuilder.AppendJoin(", ", l.ToBytes());
            stringBuilder.Append('\n');
        }

        stringBuilder.Append("\n#[_a]#\n");
        stringBuilder.AppendJoin(", ", A.ToBytes());
        stringBuilder.Append("\n#[_r]#\n");
        foreach (Banderwagon l in R)
        {
            stringBuilder.AppendJoin(", ", l.ToBytes());
            stringBuilder.Append('\n');
        }

        return stringBuilder.ToString();
    }
}

public class VerkleProofStruct
{
    public readonly IpaProofStruct IpaProof;
    public readonly Banderwagon D;

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
        stringBuilder.AppendJoin(", ", D.ToBytes());
        return stringBuilder.ToString();
    }

    public byte[] Encode()
    {
        List<byte> encoded = new List<byte>();

        encoded.AddRange(D.ToBytes());
        encoded.AddRange(IpaProof.Encode());

        return encoded.ToArray();
    }
}
