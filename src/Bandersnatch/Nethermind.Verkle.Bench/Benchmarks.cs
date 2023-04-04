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

        [ParamsSource(nameof(ValuesTuple))]
        protected (byte[], UInt256) _a;

    }

    public class BenchmarkOpsBase
    {

        protected static UInt256 _uMod = FpE._modulus.Value;
        protected static BigInteger _bMod = (BigInteger)_uMod;
        private static IEnumerable<BigInteger> Values => new[]
        {
            Numbers._uInt256Max
        }.Concat(UnaryOps.RandomUnsigned(1));
        public IEnumerable<(BigInteger, UInt256, FpE)> ValuesTuple => Values.Select(x => (x, (UInt256)x, (FpE)x));
        public IEnumerable<int> ValuesInt => UnaryOps.RandomInt(3);
    }

    public class TwoParamBenchmarkBase : BenchmarkOpsBase
    {
        [ParamsSource(nameof(ValuesTuple))]
        public (BigInteger, UInt256, FpE) _a;

        [ParamsSource(nameof(ValuesTuple))]
        public (BigInteger, UInt256, FpE) _b;
    }
}
