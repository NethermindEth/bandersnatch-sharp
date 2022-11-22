// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

using System.Buffers.Binary;
using Nethermind.Int256;

namespace Nethermind.Field.Montgomery;

public static class ElementUtils
{
    public static void SubtractMod(in UInt256 a, in UInt256 b, in UInt256 m, out UInt256 res)
    {
        if (UInt256.SubtractUnderflow(a, b, out res))
        {
            UInt256.Subtract(b, a, out res);
            UInt256.Mod(res, m, out res);
            UInt256.Subtract(m, res, out res);
        }
        else
        {
            UInt256.Mod(res, m, out res);
        }
    }

    public static void FromBytes(in ReadOnlySpan<byte> bytes, bool isBigEndian, out ulong u0, out ulong u1, out ulong u2, out ulong u3)
    {
        if (bytes.Length == 32)
        {
            if (isBigEndian)
            {
                u3 = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(0, 8));
                u2 = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(8, 8));
                u1 = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(16, 8));
                u0 = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(24, 8));
            }
            else
            {
                u0 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(0, 8));
                u1 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(8, 8));
                u2 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(16, 8));
                u3 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(24, 8));
            }
        }
        else
        {
            int byteCount = bytes.Length;
            int unalignedBytes = byteCount % 8;
            int dwordCount = byteCount / 8 + (unalignedBytes == 0 ? 0 : 1);

            ulong cs0 = 0;
            ulong cs1 = 0;
            ulong cs2 = 0;
            ulong cs3 = 0;

            if (dwordCount == 0)
            {
                u0 = u1 = u2 = u3 = 0;
                return;
            }

            if (dwordCount >= 1)
            {
                for (int j = 8; j > 0; j--)
                {
                    cs0 <<= 8;
                    if (j <= byteCount)
                    {
                        cs0 |= bytes[byteCount - j];
                    }
                }
            }

            if (dwordCount >= 2)
            {
                for (int j = 16; j > 8; j--)
                {
                    cs1 <<= 8;
                    if (j <= byteCount)
                    {
                        cs1 |= bytes[byteCount - j];
                    }
                }
            }

            if (dwordCount >= 3)
            {
                for (int j = 24; j > 16; j--)
                {
                    cs2 <<= 8;
                    if (j <= byteCount)
                    {
                        cs2 |= bytes[byteCount - j];
                    }
                }
            }

            if (dwordCount >= 4)
            {
                for (int j = 32; j > 24; j--)
                {
                    cs3 <<= 8;
                    if (j <= byteCount)
                    {
                        cs3 |= bytes[byteCount - j];
                    }
                }
            }

            u0 = cs0;
            u1 = cs1;
            u2 = cs2;
            u3 = cs3;
        }
    }

    public static Span<byte> ToBigEndian(in ulong u0, in ulong u1, in ulong u2, in ulong u3)
    {
        Span<byte> target = new byte[32];
        BinaryPrimitives.WriteUInt64BigEndian(target.Slice(0, 8), u3);
        BinaryPrimitives.WriteUInt64BigEndian(target.Slice(8, 8), u2);
        BinaryPrimitives.WriteUInt64BigEndian(target.Slice(16, 8), u1);
        BinaryPrimitives.WriteUInt64BigEndian(target.Slice(24, 8), u0);
        return target;
    }

    public static Span<byte> ToLittleEndian(in ulong u0, in ulong u1, in ulong u2, in ulong u3)
    {
        Span<byte> target = new byte[32];
        BinaryPrimitives.WriteUInt64LittleEndian(target.Slice(0, 8), u0);
        BinaryPrimitives.WriteUInt64LittleEndian(target.Slice(8, 8), u1);
        BinaryPrimitives.WriteUInt64LittleEndian(target.Slice(16, 8), u2);
        BinaryPrimitives.WriteUInt64LittleEndian(target.Slice(24, 8), u3);
        return target;
    }
}
