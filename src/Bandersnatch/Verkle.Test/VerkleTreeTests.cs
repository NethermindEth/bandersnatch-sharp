using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Verkle.Test;

[TestFixture]
public class VerkleTreeTests
{
    [Test]
    public void InsertKey0Value0()
    {
        VerkleTree tree = new();
        var key = new byte[32];

        tree.Insert(key, key);
        AssertRootNode(tree.RootHash,
            "ff00a9f3f2d4f58fc23bceebf6b2310419ceac2c30445e2f374e571487715015");
    }
    
    [Test]
    public void InsertKey1Value1()
    {
        VerkleTree tree = new();
        var key = new byte[]
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
            25, 26, 27, 28, 29, 30, 31, 32,
        };

        tree.Insert(key, key);
        AssertRootNode(tree.RootHash,
            "029b6c4c8af9001f0ac76472766c6579f41eec84a73898da06eb97ebdab80a09");
    }
    
    [Test]
    public void InsertKey1Value1Edge()
    {
        VerkleTree tree = new();
        var key = new byte[]
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
            25, 26, 27, 28, 29, 30, 31, 128,
        };

        tree.Insert(key, key);
        AssertRootNode(tree.RootHash,
            "b35b4add297834fba0b5160a46d8098a35b6961a875c44cae935507e05704707");
    }
    
    [Test]
    public void InsertSameStemTwoLeaves()
    {
        VerkleTree tree = new();
        var keyA = new byte[]
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
            25, 26, 27, 28, 29, 30, 31, 32,
        };
        
        var keyB = new byte[]
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
            25, 26, 27, 28, 29, 30, 31, 128,
        };

        tree.Insert(keyA, keyA);
        AssertRootNode(tree.RootHash,
            "029b6c4c8af9001f0ac76472766c6579f41eec84a73898da06eb97ebdab80a09");
        tree.Insert(keyB, keyB);
        AssertRootNode(tree.RootHash,
            "51e3b2b1b4e4fa85098c91c269af56b06a4474b69128dce99846f0549267fd09");
    }

    private void AssertRootNode(byte[] realRootHash, string expectedRootHash)
    {
        Convert.ToHexString(realRootHash).Should()
            .BeEquivalentTo(expectedRootHash);
    }
}