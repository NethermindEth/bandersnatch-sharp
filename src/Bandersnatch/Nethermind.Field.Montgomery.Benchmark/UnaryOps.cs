using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Nethermind.Field.Montgomery.Benchmark
{
    public static class UnaryOps
    {
        public static IEnumerable<BigInteger> _testCases = new[]{
            0,
            1,
            2,
            3,
            short.MaxValue,
            ushort.MaxValue,
            int.MaxValue,
            uint.MaxValue,
            long.MaxValue,
            ulong.MaxValue,
            TestNumbers._twoTo64,
            TestNumbers._twoTo128,
            TestNumbers._twoTo192,
            TestNumbers._uInt128Max,
            TestNumbers._uInt192Max,
            TestNumbers._uInt256Max,
        }.Concat(RandomUnsigned(5));

        public static IEnumerable<BigInteger> _signedTestCases = new[]{
            0,
            1,
            2,
            3,
            short.MaxValue,
            ushort.MaxValue,
            int.MaxValue,
            uint.MaxValue,
            long.MaxValue,
            ulong.MaxValue,
            TestNumbers._twoTo64,
            TestNumbers._twoTo128,
            TestNumbers._twoTo192,
            TestNumbers._uInt128Max,
            TestNumbers._uInt192Max,
            TestNumbers._int256Max,
            TestNumbers._int256Min,
        }.Concat(RandomSigned(5));

        public static IEnumerable<ulong> _uLongTestCases =
        new ulong[]{
            0ul,
            1ul,
            2ul,
            3ul,
            ushort.MaxValue,
            int.MaxValue,
            uint.MaxValue,
            long.MaxValue,
            ulong.MaxValue,
        };

        public static IEnumerable<int> ShiftTestCases => Enumerable.Range(0, 257);

        const int Seed = 0;

        public static IEnumerable<BigInteger> RandomSigned(int count)
        {
            Random rand = new(Seed);
            byte[] data = new byte[256 / 8];
            for (int i = 0; i < count; i++)
            {
                rand.NextBytes(data);
                yield return new BigInteger(data);
            }
        }

        public static IEnumerable<BigInteger> RandomUnsigned(int count)
        {
            Random rand = new(Seed);
            byte[] data = new byte[256 / 8];
            for (int i = 0; i < count; i++)
            {
                rand.NextBytes(data);
                data[^1] &= 0x7F;
                yield return new BigInteger(data);
            }
        }

        public static IEnumerable<int> RandomInt(int count)
        {
            Random rand = new(Seed);
            for (int i = 0; i < count; i++)
            {
                yield return rand.Next();
            }
        }
    }
}
