// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nethermind.Field.Bench;
using Nethermind.Int256;
using Nethermind.Verkle.Fields.FpEElement;
using Nethermind.Verkle.Fields.FrEElement;
using Nethermind.Verkle.Utils;

namespace Nethermind.Verkle.Bench
{
    public class BenchmarkVerkleBase
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
    }

    public class BenchmarkOpsBase
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
        private static IEnumerable<byte[]> Values => RandomBytes(5);
        public static IEnumerable<FpE> ValuesFpETuple => Values.Select(x => new FpE(x));
        public static IEnumerable<FrE> ValuesFrETuple => Values.Select(x => new FrE(x));
    }

    public class TwoParamBenchmarkBase : BenchmarkOpsBase
    {
        [ParamsSource(nameof(ValuesFpETuple))]
        public  FpE _a;

        [ParamsSource(nameof(ValuesFpETuple))]
        public FpE _b;
    }
}
