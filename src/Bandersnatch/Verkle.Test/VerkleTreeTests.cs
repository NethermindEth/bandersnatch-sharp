using System;
using Curve;
using Field;
using FluentAssertions;
using NUnit.Framework;

namespace Verkle.Test;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

[TestFixture]
public class VerkleTreeTests
{
    [Test]
    public void InsertKey0Value0()
    {
        VerkleTree tree = new();
        byte[] key = new byte[32];

        tree.Insert(key, key);
        AssertRootNode(tree.RootHash,
            "ff00a9f3f2d4f58fc23bceebf6b2310419ceac2c30445e2f374e571487715015");
    }
    
    [Test]
    public void InsertKey1Value1()
    {
        VerkleTree tree = new();
        byte[] key = new byte[]
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
            25, 26, 27, 28, 29, 30, 31, 32,
        };

        tree.Insert(key, key);
        AssertRootNode(tree.RootHash,
            "029b6c4c8af9001f0ac76472766c6579f41eec84a73898da06eb97ebdab80a09");
    }
    
    [Test]
    public void InsertSameStemTwoLeaves()
    {
        VerkleTree tree = new();
        byte[] keyA = new byte[]
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
            25, 26, 27, 28, 29, 30, 31, 32,
        };
        
        byte[] keyB = new byte[]
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
    
    [Test]
    public void InsertKey1Val1Key2Val2()
    {
        VerkleTree tree = new();
        byte[] keyA = new byte[32];
        byte[] keyB = new byte[32];
        for (int i = 0; i < 32; i++)
        {
            keyB[i] = 1;
        }
        
        tree.Insert(keyA, keyA);
        AssertRootNode(tree.RootHash,
            "ff00a9f3f2d4f58fc23bceebf6b2310419ceac2c30445e2f374e571487715015");
        tree.Insert(keyB, keyB);
        AssertRootNode(tree.RootHash,
            "83ef6d10568d0ac4abbc5fdc17fe7172638803545fd2866aa1d9d204792a2c09");
    }
    
    [Test]
    public void InsertLongestPath()
    {
        VerkleTree tree = new();
        byte[] keyA = new byte[32];
        byte[] keyB = new byte[32];
        keyB[30] = 1;
        
        tree.Insert(keyA, keyA);
        AssertRootNode(tree.RootHash,
            "ff00a9f3f2d4f58fc23bceebf6b2310419ceac2c30445e2f374e571487715015");
        tree.Insert(keyB, keyB);
        AssertRootNode(tree.RootHash,
            "fe2e17833b90719eddcad493c352ccd491730643ecee39060c7c1fff5fcc621a");
    }
    
    [Test]
    public void InsertAndTraverseLongestPath()
    {
        VerkleTree tree = new();
        byte[] keyA = new byte[32];
        tree.Insert(keyA, keyA);
        AssertRootNode(tree.RootHash,
            "ff00a9f3f2d4f58fc23bceebf6b2310419ceac2c30445e2f374e571487715015");
        
        byte[] keyB = new byte[32];
        keyB[30] = 1;
        tree.Insert(keyB, keyB);
        AssertRootNode(tree.RootHash,
            "fe2e17833b90719eddcad493c352ccd491730643ecee39060c7c1fff5fcc621a");
        
        byte[] keyC = new byte[32];
        keyC[29] = 1;
        tree.Insert(keyC, keyC);
        AssertRootNode(tree.RootHash,
            "74ff8821eca20188de49340124f249dac94404efdb3838bb6b4d298e483cc20e");
        
    }
    
    [Test]
    public void TestEmptyTrie()
    {
        VerkleTree tree = new();
        tree.RootHash.Should().BeEquivalentTo(Fr.Zero.ToBytes());
    }
    
    

    private static void AssertRootNode(byte[] realRootHash, string expectedRootHash)
    {
        Convert.ToHexString(realRootHash).Should()
            .BeEquivalentTo(expectedRootHash);
    }
}