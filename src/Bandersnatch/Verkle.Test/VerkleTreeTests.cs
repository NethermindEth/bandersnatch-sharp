using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Verkle.Test;

[TestFixture]
public class VerkleTreeTests
{
    [Test]
    public void VerkleTreeInsert()
    {
        VerkleTree tree = new();
        var key = new byte[]
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
            25, 26, 27, 28, 29, 30, 31, 32,
        };
        
        tree.Insert(key, key);
        Convert.ToHexString(tree.RootNode.PointAsField.ToBytes()).Should()
            .BeEquivalentTo("029b6c4c8af9001f0ac76472766c6579f41eec84a73898da06eb97ebdab80a09");
    }
}