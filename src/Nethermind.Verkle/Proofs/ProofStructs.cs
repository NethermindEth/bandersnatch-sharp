// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using System.Text;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Fields.FrEElement;

namespace Nethermind.Verkle.Proofs;

public readonly struct IpaProofStruct(Banderwagon[] l, FrE a, Banderwagon[] r)
{
    public readonly FrE A = a;
    public readonly Banderwagon[] L = l;
    public readonly Banderwagon[] R = r;

    public byte[] Encode()
    {
        List<byte> encoded = [];

        foreach (Banderwagon l in L) encoded.AddRange(l.ToBytes());

        foreach (Banderwagon r in R) encoded.AddRange(r.ToBytes());

        encoded.AddRange(A.ToBytes());

        return encoded.ToArray();
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new();
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

public readonly struct IpaProofStructSerialized(byte[] a, byte[] l, byte[] r)
{
    public readonly byte[] A = a;
    public readonly byte[] L = l;
    public readonly byte[] R = r;

    public byte[] Encode()
    {
        List<byte> encoded = [.. L, .. A, .. R];
        return encoded.ToArray();
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("\n#[_l]#\n");
        stringBuilder.AppendJoin(", ", L);
        stringBuilder.Append("\n#[_a]#\n");
        stringBuilder.AppendJoin(", ", A);
        stringBuilder.Append("\n#[_r]#\n");
        stringBuilder.AppendJoin(", ", R);
        stringBuilder.Append('\n');
        return stringBuilder.ToString();
    }
}

public readonly struct VerkleProofStruct
{
    public readonly Banderwagon D;
    public readonly IpaProofStruct IpaProof;

    public VerkleProofStruct(IpaProofStruct ipaProof, Banderwagon d)
    {
        IpaProof = ipaProof;
        D = d;
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new();
        stringBuilder.Append("\n##[IPA Proof]##\n");
        stringBuilder.Append(IpaProof);
        stringBuilder.Append("\n##[_d]##\n");
        stringBuilder.AppendJoin(", ", D.ToBytes());
        return stringBuilder.ToString();
    }

    public byte[] Encode()
    {
        List<byte> encoded = [];

        encoded.AddRange(D.ToBytes());
        encoded.AddRange(IpaProof.Encode());

        return encoded.ToArray();
    }
}

public readonly struct VerkleProofStructSerialized(IpaProofStructSerialized ipaProofSerialized, byte[] d)
{
    public readonly byte[] D = d;
    public readonly IpaProofStructSerialized IpaProofSerialized = ipaProofSerialized;

    public byte[] Encode()
    {
        List<byte> encoded = [.. D, .. IpaProofSerialized.Encode()];
        return encoded.ToArray();
    }

    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("\n#[_d]#\n");
        stringBuilder.AppendJoin(", ", D);
        stringBuilder.Append(IpaProofSerialized.ToString());
        return stringBuilder.ToString();
    }
}