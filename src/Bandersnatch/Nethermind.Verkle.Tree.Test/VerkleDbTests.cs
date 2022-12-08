using System.Collections.Generic;
using Nethermind.Utils.Extensions;
using Nethermind.Verkle.Tree.VerkleNodes;
using Nethermind.Verkle.Tree.VerkleStateDb;
using NUnit.Framework;

namespace Nethermind.Verkle.Tree.Test;
using BranchStore = Dictionary<byte[], InternalNode?>;
using LeafStore = Dictionary<byte[], byte[]?>;
using SuffixStore = Dictionary<byte[], SuffixTree?>;

public class VerkleDbTests
{
    [Test]
    public void ByteArrayEqualityTestsDictionary()
    {
        byte[] a = { 1, 2 };
        byte[] b = { 1, 2 };

        Dictionary<byte[], byte[]> table = new Dictionary<byte[], byte[]>
        {
            [a] = b,
        };
        Assert.IsFalse(table.TryGetValue(b, out byte[] _));

        table = new Dictionary<byte[], byte[]>(Bytes.EqualityComparer)
        {
            [a] = b,
        };
        Assert.IsTrue(table.TryGetValue(b, out byte[] _));
    }

    [Test]
    public void TestDiffLayer()
    {
        DiffLayer forwardDiff = new DiffLayer(DiffType.Forward);
        DiffLayer reverseDiff = new DiffLayer(DiffType.Reverse);

        MemoryStateDb currentState = new MemoryStateDb();
        MemoryStateDb changes = new MemoryStateDb();


    }
}
