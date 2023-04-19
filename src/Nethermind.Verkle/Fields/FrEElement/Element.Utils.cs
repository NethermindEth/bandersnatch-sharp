// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Nethermind.Int256;
using FE = Nethermind.Verkle.Fields.FrEElement.FrE;

namespace Nethermind.Verkle.Fields.FrEElement
{
    public readonly partial struct FrE
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

        public ulong this[int index] => index switch
        {
            0 => u0,
            1 => u1,
            2 => u2,
            3 => u3,
            var _ => throw new IndexOutOfRangeException()
        };

        public bool IsZero => (u0 | u1 | u2 | u3) == 0;
        public bool IsOne => Equals(One);

        public static IEnumerable<FE> GetRandom()
        {
            Random rand = new Random(0);
            byte[] data = new byte[32];

            while (true)
            {
                rand.NextBytes(data);
                yield return new FE(data);
            }
        }

        private static void SubtractMod(in UInt256 a, in UInt256 b, in UInt256 m, out UInt256 res)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Rsh(ulong a, int n)
        {
            return a >> n;
        }

        // It avoids c#'s way of shifting a 64-bit number by 64-bit, i.e. in c# a << 64 == a, in our version a << 64 == 0.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Lsh(ulong a, int n)
        {
            return a << n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddWithCarry(ulong x, ulong y, ref ulong carry, out ulong sum)
        {
            sum = x + y + carry;
            carry = sum < x || sum < y ? 1UL : 0UL;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong MAdd0(ulong a, ulong b, ulong c)
        {
            ulong carry = 0;
            ulong hi = Math.BigMul(a, b, out ulong lo);
            AddWithCarry(lo, c, ref carry, out lo);
            return hi + carry;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong MAdd1(ulong a, ulong b, ulong c, out ulong lo)
        {
            ulong hi = Math.BigMul(a, b, out lo);
            ulong carry = 0;

            AddWithCarry(lo, c, ref carry, out lo);
            hi += carry;
            return hi;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong MAdd2(ulong a, ulong b, ulong c, ulong d, out ulong lo)
        {
            ulong hi = Math.BigMul(a, b, out lo);
            ulong carry = 0;
            AddWithCarry(c, d, ref carry, out c);
            hi += carry;
            carry = 0;
            AddWithCarry(lo, c, ref carry, out lo);
            hi += carry;
            return hi;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong MAdd3(ulong a, ulong b, ulong c, ulong d, ulong e, out ulong lo)
        {
            ulong hi = Math.BigMul(a, b, out lo);
            ulong carry = 0;
            AddWithCarry(c, d, ref carry, out c);
            hi += carry;
            carry = 0;
            AddWithCarry(lo, c, ref carry, out lo);
            hi = hi + e + carry;
            return hi;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SubtractWithBorrow(ulong a, ulong b, ref ulong borrow, out ulong res)
        {
            res = a - b - borrow;
            borrow = (~a & b | ~(a ^ b) & res) >> 63;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Len64(ulong x) => 64 - BitOperations.LeadingZeroCount(x);

        private static void FromBytes(in ReadOnlySpan<byte> bytes, bool isBigEndian, out ulong u0, out ulong u1, out ulong u2, out ulong u3)
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

        private static Span<byte> ToBigEndian(scoped in ulong u0, scoped in ulong u1, scoped in ulong u2, scoped in ulong u3)
        {
            Span<byte> target = new byte[32];
            BinaryPrimitives.WriteUInt64BigEndian(target.Slice(0, 8), u3);
            BinaryPrimitives.WriteUInt64BigEndian(target.Slice(8, 8), u2);
            BinaryPrimitives.WriteUInt64BigEndian(target.Slice(16, 8), u1);
            BinaryPrimitives.WriteUInt64BigEndian(target.Slice(24, 8), u0);
            return target;
        }

        private static Span<byte> ToLittleEndian(scoped in ulong u0, scoped in ulong u1, scoped in ulong u2, scoped in ulong u3)
        {
            Span<byte> target = new byte[32];
            BinaryPrimitives.WriteUInt64LittleEndian(target.Slice(0, 8), u0);
            BinaryPrimitives.WriteUInt64LittleEndian(target.Slice(8, 8), u1);
            BinaryPrimitives.WriteUInt64LittleEndian(target.Slice(16, 8), u2);
            BinaryPrimitives.WriteUInt64LittleEndian(target.Slice(24, 8), u3);
            return target;
        }

        private static ReadOnlySpan<byte> SBroadcastLookup => new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        };
    }
}
