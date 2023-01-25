// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nethermind.Field.Montgomery.FpEElement;
using Nethermind.Int256;
using Nethermind.Verkle.Utils;

namespace Nethermind.Verkle.Bench
{
    public class BenchmarkBase
    {
        private static IEnumerable<byte[]> RandomBytes(int count)
        {
            Random rand = new Random(1);
            byte[] data = new byte[32];
            for (int i = 0; i < count; i++)
            {
                rand.NextBytes(data);
                yield return data;
            }
        }

        private static IEnumerable<byte[]> Values => RandomBytes(1);
        public static IEnumerable<(byte[], UInt256)> ValuesTuple => Values.Select(x => (x, new UInt256(x)));

        [ParamsSource(nameof(ValuesTuple))]
        protected (byte[], UInt256) _a;

    }

    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class PedersenHashBench : BenchmarkBase
    {
        [Benchmark(Baseline = true)]
        public byte[] Bench()
        {
            return PedersenHash.Hash(_a.Item1, _a.Item2);
        }
    }
}
