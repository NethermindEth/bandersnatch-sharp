// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nethermind.Verkle.Curve;

namespace Nethermind.Verkle.Bench;

[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[NoIntrinsicsJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class BenchBanderwagonSerialization
{
    private static readonly byte[] _encoded = element.ToBytes();

    private static readonly byte[] _encodedUncompressed = element.ToBytesUncompressed();

    private static Banderwagon element => CRS.Instance.BasisG[5];

    [Benchmark]
    public byte[] SerializeBanderwagon()
    {
        return element.ToBytes();
    }

    [Benchmark]
    public byte[] SerializeBanderwagonUncompressed()
    {
        return element.ToBytesUncompressed();
    }

    [Benchmark]
    public Banderwagon DeserializeBanderwagonWithOutSubgroupCheck()
    {
        return Banderwagon.FromBytes(_encoded, subgroupCheck: false)!.Value;
    }

    [Benchmark]
    public Banderwagon DeserializeBanderwagonWithSubgroupCheck()
    {
        return Banderwagon.FromBytes(_encoded, subgroupCheck: true)!.Value;
    }

    [Benchmark]
    public Banderwagon DeserializeBanderwagonCompressed()
    {
        return Banderwagon.FromBytesUncompressedUnchecked(_encodedUncompressed);
    }
}
