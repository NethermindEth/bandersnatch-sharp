// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

namespace Nethermind.Verkle.Tree.VerkleStateDb
{
    public class DiffLayer : IDiffLayer
    {
        public DiffLayer()
        {
            Diff = new Dictionary<long, byte[]>();
        }
        private Dictionary<long, byte[]> Diff { get; }
        public void InsertDiff(long blockNumber, IVerkleDiffDb diff)
        {
            Diff[blockNumber] = diff.Encode();
        }
        public byte[] FetchDiff(long blockNumber)
        {
            return Diff[blockNumber];
        }
    }
}
