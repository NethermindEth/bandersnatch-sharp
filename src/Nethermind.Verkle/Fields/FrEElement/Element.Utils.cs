// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using System.Buffers.Binary;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Nethermind.Int256;
using FE = Nethermind.Verkle.Fields.FrEElement.FrE;

namespace Nethermind.Verkle.Fields.FrEElement;

public readonly partial struct FrE
{
    public static IEnumerable<FE> GetRandom()
    {
        Random rand = new(0);
        byte[] data = new byte[32];

        while (true)
        {
            rand.NextBytes(data);
            yield return FromBytesReducedMultiple(data);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SubtractMod(in UInt256 a, in UInt256 b, in UInt256 m, out UInt256 res)
    {
        if (UInt256.SubtractUnderflow(a, b, out res))
        {
            UInt256.Subtract(b, a, out res);
            UInt256.Mod(res, m, out res);
            UInt256.Subtract(m, res, out res);
        }
        else
            UInt256.Mod(res, m, out res);
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
        borrow = ((~a & b) | (~(a ^ b) & res)) >> 63;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Len64(ulong x)
    {
        return 64 - BitOperations.LeadingZeroCount(x);
    }

    private static byte[] ToBigEndian(scoped in ulong u0, scoped in ulong u1, scoped in ulong u2,
        scoped in ulong u3)
    {
        byte[] returnEncoding = new byte[32];
        Span<byte> target = returnEncoding;
        BinaryPrimitives.WriteUInt64BigEndian(target.Slice(0, 8), u3);
        BinaryPrimitives.WriteUInt64BigEndian(target.Slice(8, 8), u2);
        BinaryPrimitives.WriteUInt64BigEndian(target.Slice(16, 8), u1);
        BinaryPrimitives.WriteUInt64BigEndian(target.Slice(24, 8), u0);
        return returnEncoding;
    }

    private static byte[] ToLittleEndian(scoped in ulong u0, scoped in ulong u1, scoped in ulong u2,
        scoped in ulong u3)
    {
        byte[] returnEncoding = new byte[32];
        Span<byte> target = returnEncoding;
        if (Avx.IsSupported)
        {
            Unsafe.As<byte, Vector256<ulong>>(ref MemoryMarshal.GetReference(target)) =
                Unsafe.As<ulong, Vector256<ulong>>(ref Unsafe.AsRef(in u0));
        }
        else
        {
            BinaryPrimitives.WriteUInt64LittleEndian(target.Slice(0, 8), u0);
            BinaryPrimitives.WriteUInt64LittleEndian(target.Slice(8, 8), u1);
            BinaryPrimitives.WriteUInt64LittleEndian(target.Slice(16, 8), u2);
            BinaryPrimitives.WriteUInt64LittleEndian(target.Slice(24, 8), u3);
        }

        return returnEncoding;
    }

    private static ReadOnlySpan<byte> SBroadcastLookup => new byte[]
    {
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    };

    [DoesNotReturn]
    [StackTraceHidden]
    private static void ThrowInvalidConstraintException() => throw new InvalidConstraintException("this should not be possible");

    [DoesNotReturn]
    [StackTraceHidden]
    private static ulong ThrowIndexOutOfRangeException() => throw new IndexOutOfRangeException();
}
