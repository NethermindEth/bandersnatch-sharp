// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Nethermind.Int256;
using FE = Nethermind.Field.Montgomery.FpEElement.FpE;

[assembly: InternalsVisibleTo("Nethermind.Field.Montgomery.Test")]

namespace Nethermind.Field.Montgomery.FpEElement
{
    [StructLayout(LayoutKind.Explicit)]
    public readonly partial struct FpE
    {
        public FpE(ulong u0 = 0, ulong u1 = 0, ulong u2 = 0, ulong u3 = 0)
        {
            this.u0 = u0;
            this.u1 = u1;
            this.u2 = u2;
            this.u3 = u3;
        }

        public FpE(in ReadOnlySpan<byte> bytes, bool isBigEndian = false)
        {
            UInt256 val = new UInt256(bytes, isBigEndian);
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
