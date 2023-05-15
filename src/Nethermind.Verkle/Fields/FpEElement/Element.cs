// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Nethermind.Int256;
using FE = Nethermind.Verkle.Fields.FpEElement.FpE;

[assembly: InternalsVisibleTo("Nethermind.Field.Montgomery.Test")]

namespace Nethermind.Verkle.Fields.FpEElement
{
    [StructLayout(LayoutKind.Explicit)]
    public readonly partial struct FpE
    {
        /* in little endian order so u3 is the most significant ulong */
        [FieldOffset(0)] private readonly ulong u0;
        [FieldOffset(8)] private readonly ulong u1;
        [FieldOffset(16)] private readonly ulong u2;
        [FieldOffset(24)] private readonly ulong u3;

        private ulong this[int index] => index switch
        {
            0 => u0,
            1 => u1,
            2 => u2,
            3 => u3,
            _ => throw new IndexOutOfRangeException()
        };

        public bool IsZero => (u0 | u1 | u2 | u3) == 0;
        public bool IsOne => Equals(One);

        public FpE(ulong u0 = 0, ulong u1 = 0, ulong u2 = 0, ulong u3 = 0)
        {
            if (Avx2.IsSupported)
            {
                Unsafe.SkipInit(out this.u0);
                Unsafe.SkipInit(out this.u1);
                Unsafe.SkipInit(out this.u2);
                Unsafe.SkipInit(out this.u3);
                Unsafe.As<ulong, Vector256<ulong>>(ref this.u0) = Vector256.Create(u0, u1, u2, u3);
            }
            else
            {
                this.u0 = u0;
                this.u1 = u1;
                this.u2 = u2;
                this.u3 = u3;
            }
        }

        public FpE(in ReadOnlySpan<byte> bytes, bool isBigEndian = false)
        {
            UInt256 val = new(bytes, isBigEndian);
            val.Mod(_modulus.Value, out UInt256 res);
            FE inp = new FE(res.u0, res.u1, res.u2, res.u3);
            ToMontgomery(inp, out this);
        }

        private FpE(BigInteger value)
        {
            UInt256 res;
            if (value.Sign < 0)
            {
                SubtractMod(UInt256.Zero, (UInt256)(-value), _modulus.Value, out res);
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
    }
}
