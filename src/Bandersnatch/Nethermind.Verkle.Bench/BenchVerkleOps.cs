// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nethermind.Verkle.Utils;

namespace Nethermind.Verkle.Bench
{
    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class PedersenHashBench : BenchmarkVerkleBase
    {
        [Benchmark(Baseline = true)]
        public byte[] Bench()
        {
            return PedersenHash.Hash(_a.Item1, _a.Item2);
        }
    }
}
