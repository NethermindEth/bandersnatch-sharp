// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nethermind.Field.Arithmetic.Benchmark
{
    public static class Numbers
    {
        private const int Seed = 0;

        public static IEnumerable<ulong> _uLongTestCases = new ulong[]
        {
            0ul, 1ul, 2ul, 3ul, ushort.MaxValue, int.MaxValue, uint.MaxValue, long.MaxValue, ulong.MaxValue
        }.Concat(RandomUlong(10));

        public static IEnumerable<ulong> RandomUlong(int count)
        {
            Random rand = new Random(Seed);
            for (int i = 0; i < count; i++)
            {
                yield return (ulong)rand.NextInt64();
            }
        }
    }
}
