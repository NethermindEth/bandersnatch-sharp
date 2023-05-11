// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using System.Runtime.InteropServices;

namespace Nethermind.Verkle.Fields;

[StructLayout(LayoutKind.Explicit)]
public struct U4
{
    /* in little endian order so u3 is the most significant ulong */
    [FieldOffset(0)] public ulong u0;
    [FieldOffset(8)] public ulong u1;
    [FieldOffset(16)] public ulong u2;
    [FieldOffset(24)] public ulong u3;
}

[StructLayout(LayoutKind.Explicit)]
public struct U3
{
    /* in little endian order so u3 is the most significant ulong */
    [FieldOffset(0)] public ulong u0;
    [FieldOffset(8)] public ulong u1;
    [FieldOffset(16)] public ulong u2;
    [FieldOffset(24)] public ulong u3;
}
