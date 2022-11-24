// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

using System.Numerics;
using System.Runtime.InteropServices;
using Nethermind.Int256;

namespace Nethermind.Field.Montgomery.ElementFactory;

[StructLayout(LayoutKind.Explicit)]
public readonly partial struct Element
{
    /* in little endian order so u3 is the most significant ulong */
    [FieldOffset(0)]
    public readonly ulong u0;
    [FieldOffset(8)]
    public readonly ulong u1;
    [FieldOffset(16)]
    public readonly ulong u2;
    [FieldOffset(24)]
    public readonly ulong u3;

    public ulong this[int index]
    {
        get
        {
            return index switch
            {
                0 => u0,
                1 => u1,
                2 => u2,
                3 => u3,
                var _ => throw new IndexOutOfRangeException()
            };
        }
    }

    public Element(ulong u0 = 0, ulong u1 = 0, ulong u2 = 0, ulong u3 = 0)
    {
        this.u0 = u0;
        this.u1 = u1;
        this.u2 = u2;
        this.u3 = u3;
    }

    public Element(in ReadOnlySpan<byte> bytes, bool isBigEndian = false)
    {
        UInt256 val = new UInt256(bytes, isBigEndian);
        val.Mod(_modulus.Value, out UInt256 res);
        Element inp = new Element(res.u0, res.u1, res.u2, res.u3);
        ToMontgomery(inp, out this);
    }

    internal Element(BigInteger value)
    {
        UInt256 res;
        if (value.Sign < 0)
        {
            ElementUtils.SubtractMod(UInt256.Zero, (UInt256)(-value), _modulus.Value, out res);
        }
        else
        {
            UInt256.Mod((UInt256)value, _modulus.Value, out res);
        }
        u0 = res.u0;
        u1 = res.u1;
        u2 = res.u2;
        u3 = res.u3;
    }

    public bool IsZero => (u0 | u1 | u2 | u3) == 0;
    public bool IsOne => Equals(One);

}


