// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using System.Numerics;
using Nethermind.Int256;
using Nethermind.Verkle.Fields.FpEElement;

namespace Nethermind.Verkle.Bench;

public class BenchmarkBase
{
    private IEnumerable<BigInteger> Values => new[] { Numbers.UInt256Max }.Concat(UnaryOps.RandomUnsigned(1));

    public IEnumerable<(UInt256, FpE)> ValuesTuple => Values.Select(x => ((UInt256)x, (FpE)x));
}
