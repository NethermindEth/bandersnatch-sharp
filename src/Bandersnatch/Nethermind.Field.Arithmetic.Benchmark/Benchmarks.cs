using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Nethermind.Field.Arithmetic.Benchmark
{

    public class BenchmarkBase
    {
        public IEnumerable<ulong> Values => Numbers._uLongTestCases;

        [ParamsSource(nameof(Values))]
        public ulong a;

        [ParamsSource(nameof(Values))]
        public ulong b;

    }

    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class Mul64 : BenchmarkBase
    {
        [Benchmark(Baseline = true)]
        public ulong Scenario1()
        {
            return Math.BigMul(a, b, out ulong low);
        }

        [Benchmark]
        public ulong Scenario2()
        {
            return a * b;
        }
    }

    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class Lsh : BenchmarkBase
    {
        [Benchmark(Baseline = true)]
        public ulong Scenario1()
        {
            return ElementUtils.LshV1(a, 1);
        }

        [Benchmark]
        public ulong Scenario2()
        {
            return ElementUtils.LshV2(a, 1);
        }
    }

    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class Rsh : BenchmarkBase
    {
        [Benchmark(Baseline = true)]
        public ulong Scenario1()
        {
            return ElementUtils.RshV1(a, 1);
        }

        [Benchmark]
        public ulong Scenario2()
        {
            return ElementUtils.RshV2(a, 1);
        }
    }

    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class Add64 : BenchmarkBase
    {
        [Benchmark(Baseline = true)]
        public ulong Scenario1()
        {
            ulong carry = 0;
            ElementUtils.AddWithCarryV1(a, b, ref carry, out ulong res);
            return res;
        }

        [Benchmark]
        public ulong Scenario2()
        {
            ulong carry = 0;
            ElementUtils.AddWithCarryV2(a, b, ref carry, out ulong res);
            return res;
        }
    }

    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class Sub64 : BenchmarkBase
    {
        [Benchmark(Baseline = true)]
        public ulong Scenario1()
        {
            ulong borrow = 0;
            ElementUtils.SubtractWithBorrowV1(a, b, ref borrow, out ulong res);
            return res;
        }

        [Benchmark]
        public ulong Scenario2()
        {
            ulong borrow = 0;
            ElementUtils.SubtractWithBorrowV2(a, b, ref borrow, out ulong res);
            return res;
        }
    }

    public static class ElementUtils
    {
        // public static void SubtractModV1(in UInt256 a, in UInt256 b, in UInt256 m, out UInt256 res)
        // {
        //     if (UInt256.SubtractUnderflow(a, b, out res))
        //     {
        //         UInt256.Subtract(b, a, out res);
        //         UInt256.Mod(res, m, out res);
        //         UInt256.Subtract(m, res, out res);
        //     }
        //     else
        //     {
        //         UInt256.Mod(res, m, out res);
        //     }
        // }
        //
        // public static void SubtractModV2(in UInt256 a, in UInt256 b, in UInt256 m, out UInt256 res)
        // {
        //     if (UInt256.SubtractUnderflow(a, b, out res))
        //     {
        //         UInt256.Subtract(b, a, out res);
        //         UInt256.Mod(res, m, out res);
        //         UInt256.Subtract(m, res, out res);
        //     }
        //     else
        //     {
        //         UInt256.Mod(res, m, out res);
        //     }
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong RshV1(ulong a, int n)
        {
            int n1 = n >> 1;
            int n2 = n - n1;
            return a >> n1 >> n2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong RshV2(ulong a, int n)
        {
            return a >> n;
        }

        // It avoids c#'s way of shifting a 64-bit number by 64-bit, i.e. in c# a << 64 == a, in our version a << 64 == 0.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong LshV1(ulong a, int n)
        {
            int n1 = n >> 1;
            int n2 = n - n1;
            return a << n1 << n2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong LshV2(ulong a, int n)
        {
            return a << n;
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static bool AddOverflow(in ulong aU0, in ulong aU1, in ulong aU2, in ulong aU3, in ulong bU0, in ulong bU1, in ulong bU2, in ulong bU3, out ulong u0, out ulong u1, out ulong u2, out ulong u3)
        // {
        //     ulong carry = 0ul;
        //     AddWithCarry(aU0, bU0, ref carry, out u0);
        //     AddWithCarry(aU1, bU1, ref carry, out u1);
        //     AddWithCarry(aU2, bU2, ref carry, out u2);
        //     AddWithCarry(aU3, bU3, ref carry, out u3);
        //     return carry != 0;
        // }
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static bool AddOverflowV1(in ulong aU0, in ulong aU1, in ulong aU2, in ulong aU3, in ulong bU0, in ulong bU1, in ulong bU2, in ulong bU3, out ulong u0, out ulong u1, out ulong u2, out ulong u3)
        // {
        //     ulong carry = 0ul;
        //     AddWithCarryV1(aU0, bU0, ref carry, out u0);
        //     AddWithCarryV1(aU1, bU1, ref carry, out u1);
        //     AddWithCarryV1(aU2, bU2, ref carry, out u2);
        //     AddWithCarryV1(aU3, bU3, ref carry, out u3);
        //     return carry != 0;
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddWithCarryV2(ulong x, ulong y, ref ulong carry, out ulong sum)
        {
            sum = x + y + carry;
            carry = sum < x || sum < y? 1UL : 0UL;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddWithCarryV1(ulong x, ulong y, ref ulong carry, out ulong sum)
        {
            sum = x + y + carry;
            carry = (x & y | (x | y) & ~sum) >> 63;
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static ulong MAdd0(ulong a, ulong b, ulong c)
        // {
        //     ulong carry = 0;
        //     ulong hi = Math.BigMul(a, b, out ulong lo);
        //     AddWithCarry(lo, c, ref carry, out lo);
        //     return hi + carry;
        // }
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static ulong MAdd1(ulong a, ulong b, ulong c, out ulong lo)
        // {
        //     ulong hi = Math.BigMul(a, b, out lo);
        //     ulong carry = 0;
        //
        //     AddWithCarry(lo, c, ref carry, out lo);
        //     hi += carry;
        //     return hi;
        // }
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static ulong MAdd2(ulong a, ulong b, ulong c, ulong d, out ulong lo)
        // {
        //     ulong hi = Math.BigMul(a, b, out lo);
        //     ulong carry = 0;
        //     AddWithCarry(c, d, ref carry, out c);
        //     hi += carry;
        //     carry = 0;
        //     AddWithCarry(lo, c, ref carry, out lo);
        //     hi += carry;
        //     return hi;
        // }
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static ulong MAdd3(ulong a, ulong b, ulong c, ulong d, ulong e, out ulong lo)
        // {
        //     ulong hi = Math.BigMul(a, b, out lo);
        //     ulong carry = 0;
        //     AddWithCarry(c, d, ref carry, out c);
        //     hi += carry;
        //     carry = 0;
        //     AddWithCarry(lo, c, ref carry, out lo);
        //     hi = hi + e + carry;
        //     return hi;
        // }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static bool SubtractUnderflowV1(in ulong aU0, in ulong aU1, in ulong aU2, in ulong aU3, in ulong bU0, in ulong bU1, in ulong bU2, in ulong bU3, out ulong u0, out ulong u1, out ulong u2, out ulong u3)
        // {
        //     ulong borrow = 0;
        //     SubtractWithBorrowV1(aU0, bU0, ref borrow, out u0);
        //     SubtractWithBorrowV1(aU1, bU1, ref borrow, out u1);
        //     SubtractWithBorrowV1(aU2, bU2, ref borrow, out u2);
        //     SubtractWithBorrowV1(aU3, bU3, ref borrow, out u3);
        //     return borrow != 0;
        // }
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static bool SubtractUnderflowV2(in ulong aU0, in ulong aU1, in ulong aU2, in ulong aU3, in ulong bU0, in ulong bU1, in ulong bU2, in ulong bU3, out ulong u0, out ulong u1, out ulong u2, out ulong u3)
        // {
        //     ulong borrow = 0;
        //     SubtractWithBorrowV2(aU0, bU0, ref borrow, out u0);
        //     SubtractWithBorrowV2(aU1, bU1, ref borrow, out u1);
        //     SubtractWithBorrowV2(aU2, bU2, ref borrow, out u2);
        //     SubtractWithBorrowV2(aU3, bU3, ref borrow, out u3);
        //     return borrow != 0;
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractWithBorrowV1(ulong a, ulong b, ref ulong borrow, out ulong res)
        {
            res = a - b - borrow;
            borrow = res > a ? 1UL : 0UL;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractWithBorrowV2(ulong a, ulong b, ref ulong borrow, out ulong res)
        {
            res = a - b - borrow;
            borrow = (~a & b | ~(a ^ b) & res) >> 63;
        }

    }
}
