// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

using Nethermind.Int256;
using Nethermind.Verkle.Fields.FpEElement;
using Nethermind.Verkle.Fields.FrEElement;

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
        public static IEnumerable<FpE> ValuesFpETuple => Values.Select(x => FpE.FromBytesReduced(x));
        public static IEnumerable<FrE> ValuesFrETuple => Values.Select(x => FrE.FromBytesReduced(x));
    }
}
