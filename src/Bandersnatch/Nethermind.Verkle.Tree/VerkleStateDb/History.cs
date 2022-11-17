// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

namespace Nethermind.Verkle.Tree.VerkleStateDb;

public class DiffLayer: IDiffLayer
{
    private Dictionary<long, byte[]> Diff { get; }
    public DiffLayer()
    {
        Diff = new Dictionary<long, byte[]>();
    }
    public void InsertDiff(long blockNumber, IVerkleDiffDb diff)
    {
        Diff[blockNumber] = diff.Encode();
    }
    public byte[] FetchDiff(long blockNumber) => Diff[blockNumber];
}
