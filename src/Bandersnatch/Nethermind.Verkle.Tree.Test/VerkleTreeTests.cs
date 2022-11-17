using System;
using FluentAssertions;
using Nethermind.Field;
using Nethermind.Verkle.Curve;
using NUnit.Framework;

namespace Nethermind.Verkle.Tree.Test;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

[TestFixture]
public class VerkleTreeTests
{
    private byte[] _array1To32 =
    {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32,
    };
    private byte[] _array1To32Last128 =
    {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 128,
    };
    private byte[] _emptyArray =
    {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    };
    private byte[] _arrayAll1 =
    {
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1
    };

    private byte[] _arrayAll0Last2 =
    {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2
    };


    private byte[] _keyVersion =
    {
        121, 85, 7, 198, 131, 230, 143, 90, 165, 129, 173, 81, 186, 89, 19, 191, 13, 107, 197, 120, 243, 229, 224, 183, 72, 25, 6, 8, 210, 159, 31, 0,
    };
    private byte[] _keyBalance =
    {
        121, 85, 7, 198, 131, 230, 143, 90, 165, 129, 173, 81, 186, 89, 19, 191, 13, 107, 197, 120, 243, 229, 224, 183, 72, 25, 6, 8, 210, 159, 31, 1,
    };
    private byte[] _keyNonce =
    {
        121, 85, 7, 198, 131, 230, 143, 90, 165, 129, 173, 81, 186, 89, 19, 191, 13, 107, 197, 120, 243, 229, 224, 183, 72, 25, 6, 8, 210, 159, 31, 2,
    };
    private byte[] _keyCodeKeccak = {
        121, 85, 7, 198, 131, 230, 143, 90, 165, 129, 173, 81, 186, 89, 19, 191, 13, 107, 197, 120, 243, 229, 224, 183, 72, 25, 6, 8, 210, 159, 31, 3,
    };
    private byte[] _keyCodeSize =
    {
        121, 85, 7, 198, 131, 230, 143, 90, 165, 129, 173, 81, 186, 89, 19, 191, 13, 107, 197, 120, 243, 229, 224, 183, 72, 25, 6, 8, 210, 159, 31, 4,
    };
    private byte[] _valueEmptyCodeHashValue =
    {
        197, 210, 70, 1, 134, 247, 35, 60, 146, 126, 125, 178, 220, 199, 3, 192, 229, 0, 182, 83, 202, 130, 39, 59, 123, 250, 216, 4, 93, 133, 164, 112,
    };


    [Test]
    public void InsertKey0Value0()
    {
        VerkleTree tree = new VerkleTree();
        byte[] key = _emptyArray;

        tree.Insert(key, key);
        AssertRootNode(tree.RootHash,
            "ff00a9f3f2d4f58fc23bceebf6b2310419ceac2c30445e2f374e571487715015");

        tree.Get(key).Should().BeEquivalentTo(key);
    }

    [Test]
    public void InsertKey1Value1()
    {
        VerkleTree tree = new VerkleTree();
        byte[] key = _array1To32;

        tree.Insert(key, key);
        AssertRootNode(tree.RootHash,
            "029b6c4c8af9001f0ac76472766c6579f41eec84a73898da06eb97ebdab80a09");

        tree.Get(key).Should().BeEquivalentTo(key);
    }

    [Test]
    public void InsertSameStemTwoLeaves()
    {
        VerkleTree tree = new();
        byte[] keyA = _array1To32;

        byte[] keyB = _array1To32Last128;

        tree.Insert(keyA, keyA);
        AssertRootNode(tree.RootHash,
            "029b6c4c8af9001f0ac76472766c6579f41eec84a73898da06eb97ebdab80a09");
        tree.Insert(keyB, keyB);
        AssertRootNode(tree.RootHash,
            "51e3b2b1b4e4fa85098c91c269af56b06a4474b69128dce99846f0549267fd09");

        tree.Get(keyA).Should().BeEquivalentTo(keyA);
        tree.Get(keyB).Should().BeEquivalentTo(keyB);
    }

    [Test]
    public void InsertKey1Val1Key2Val2()
    {
        VerkleTree tree = new VerkleTree();
        byte[] keyA = _emptyArray;
        byte[] keyB = _arrayAll1;

        tree.Insert(keyA, keyA);
        AssertRootNode(tree.RootHash,
            "ff00a9f3f2d4f58fc23bceebf6b2310419ceac2c30445e2f374e571487715015");
        tree.Insert(keyB, keyB);
        AssertRootNode(tree.RootHash,
            "83ef6d10568d0ac4abbc5fdc17fe7172638803545fd2866aa1d9d204792a2c09");

        tree.Get(keyA).Should().BeEquivalentTo(keyA);
        tree.Get(keyB).Should().BeEquivalentTo(keyB);
    }

    [Test]
    public void InsertLongestPath()
    {
        VerkleTree tree = new VerkleTree();
        byte[] keyA = _emptyArray;
        byte[] keyB = (byte[])_emptyArray.Clone();
        keyB[30] = 1;

        tree.Insert(keyA, keyA);
        AssertRootNode(tree.RootHash,
            "ff00a9f3f2d4f58fc23bceebf6b2310419ceac2c30445e2f374e571487715015");
        tree.Insert(keyB, keyB);
        AssertRootNode(tree.RootHash,
            "fe2e17833b90719eddcad493c352ccd491730643ecee39060c7c1fff5fcc621a");

        tree.Get(keyA).Should().BeEquivalentTo(keyA);
        tree.Get(keyB).Should().BeEquivalentTo(keyB);
    }

    [Test]
    public void InsertAndTraverseLongestPath()
    {
        VerkleTree tree = new VerkleTree();
        byte[] keyA = _emptyArray;
        tree.Insert(keyA, keyA);
        AssertRootNode(tree.RootHash,
            "ff00a9f3f2d4f58fc23bceebf6b2310419ceac2c30445e2f374e571487715015");

        byte[] keyB = (byte[])_emptyArray.Clone();
        keyB[30] = 1;
        tree.Insert(keyB, keyB);
        AssertRootNode(tree.RootHash,
            "fe2e17833b90719eddcad493c352ccd491730643ecee39060c7c1fff5fcc621a");

        byte[] keyC = (byte[])_emptyArray.Clone();
        keyC[29] = 1;
        tree.Insert(keyC, keyC);
        AssertRootNode(tree.RootHash,
            "74ff8821eca20188de49340124f249dac94404efdb3838bb6b4d298e483cc20e");

        tree.Get(keyA).Should().BeEquivalentTo(keyA);
        tree.Get(keyB).Should().BeEquivalentTo(keyB);
        tree.Get(keyC).Should().BeEquivalentTo(keyC);
    }

    [Test]
    public void TestEmptyTrie()
    {
        VerkleTree tree = new VerkleTree();
        tree.RootHash.Should().BeEquivalentTo(Fr.Zero.ToBytes());
    }

    [Test]
    public void TestSimpleUpdate()
    {
        VerkleTree tree = new VerkleTree();
        byte[] key = _array1To32;
        byte[] value = _emptyArray;
        tree.Insert(key, value);
        AssertRootNode(tree.RootHash,
            "77a0747bd526d9d9af60bd5665d24d6cb421f5c8e726b1de62f914f3ff9a361c");
        tree.Get(key).Should().BeEquivalentTo(value);

        tree.Insert(key, key);
        AssertRootNode(tree.RootHash,
            "029b6c4c8af9001f0ac76472766c6579f41eec84a73898da06eb97ebdab80a09");
        tree.Get(key).Should().BeEquivalentTo(key);
    }

    [Test]
    public void TestInsertGet()
    {
        VerkleTree tree = new VerkleTree();

        tree.Insert(_keyVersion, _emptyArray);
        AssertRootNode(tree.RootHash,
            "012cc5c81083b484e578390ca619725ff8597753a8da7f26676459e2ab543b08");

        tree.Insert(_keyBalance, _arrayAll0Last2);
        AssertRootNode(tree.RootHash,
            "1b3e9d60e1e510defdca6a382bbc6249c98870e341744a99906a230d9193350d");

        tree.Insert(_keyNonce, _emptyArray);
        AssertRootNode(tree.RootHash,
            "5bcb12efaf7f407743ea0258d2b1fc12b0856a423c3fe268c10d53b89a43771c");

        tree.Insert(_keyCodeKeccak, _valueEmptyCodeHashValue);
        AssertRootNode(tree.RootHash,
            "828983030205ddd526a2444f707f63f187d079872d33e5fba334f77fe8bb301c");

        tree.Insert(_keyCodeSize, _emptyArray);
        AssertRootNode(tree.RootHash,
            "1f470a52c36f350d24aba63cf5de6d676deff21fbd3f844150841197c1c6af19");

        tree.Get(_keyVersion).Should().BeEquivalentTo(_emptyArray);
        tree.Get(_keyBalance).Should().BeEquivalentTo(_arrayAll0Last2);
        tree.Get(_keyNonce).Should().BeEquivalentTo(_emptyArray);
        tree.Get(_keyCodeKeccak).Should().BeEquivalentTo(_valueEmptyCodeHashValue);
        tree.Get(_keyCodeSize).Should().BeEquivalentTo(_emptyArray);
    }

    private static void AssertRootNode(byte[] realRootHash, string expectedRootHash)
    {
        Convert.ToHexString(realRootHash).Should()
            .BeEquivalentTo(expectedRootHash);
    }

    [Test]
    public void TestValueSameBeforeAndAfterFlush()
    {
        VerkleTree tree = new VerkleTree();


        tree.Insert(_keyVersion, _emptyArray);
        tree.Insert(_keyBalance, _emptyArray);
        tree.Insert(_keyNonce, _emptyArray);
        tree.Insert(_keyCodeKeccak, _valueEmptyCodeHashValue);
        tree.Insert(_keyCodeSize, _emptyArray);

        tree.Get(_keyVersion).Should().BeEquivalentTo(_emptyArray);
        tree.Get(_keyBalance).Should().BeEquivalentTo(_emptyArray);
        tree.Get(_keyNonce).Should().BeEquivalentTo(_emptyArray);
        tree.Get(_keyCodeKeccak).Should().BeEquivalentTo(_valueEmptyCodeHashValue);
        tree.Get(_keyCodeSize).Should().BeEquivalentTo(_emptyArray);

        tree.Flush(0);

        tree.Get(_keyVersion).Should().BeEquivalentTo(_emptyArray);
        tree.Get(_keyBalance).Should().BeEquivalentTo(_emptyArray);
        tree.Get(_keyNonce).Should().BeEquivalentTo(_emptyArray);
        tree.Get(_keyCodeKeccak).Should().BeEquivalentTo(_valueEmptyCodeHashValue);
        tree.Get(_keyCodeSize).Should().BeEquivalentTo(_emptyArray);
    }

    [Test]
    public void TestInsertGetMultiBlock()
    {
        VerkleTree tree = new VerkleTree();

        tree.Insert(_keyVersion, _emptyArray);
        tree.Insert(_keyBalance, _emptyArray);
        tree.Insert(_keyNonce, _emptyArray);
        tree.Insert(_keyCodeKeccak, _valueEmptyCodeHashValue);
        tree.Insert(_keyCodeSize, _emptyArray);
        tree.Flush(0);

        tree.Get(_keyVersion).Should().BeEquivalentTo(_emptyArray);
        tree.Get(_keyBalance).Should().BeEquivalentTo(_emptyArray);
        tree.Get(_keyNonce).Should().BeEquivalentTo(_emptyArray);
        tree.Get(_keyCodeKeccak).Should().BeEquivalentTo(_valueEmptyCodeHashValue);
        tree.Get(_keyCodeSize).Should().BeEquivalentTo(_emptyArray);

        tree.Insert(_keyVersion, _arrayAll0Last2);
        tree.Insert(_keyBalance, _arrayAll0Last2);
        tree.Insert(_keyNonce, _arrayAll0Last2);
        tree.Insert(_keyCodeKeccak, _valueEmptyCodeHashValue);
        tree.Insert(_keyCodeSize, _arrayAll0Last2);
        tree.Flush(1);

        tree.Get(_keyVersion).Should().BeEquivalentTo(_arrayAll0Last2);
        tree.Get(_keyBalance).Should().BeEquivalentTo(_arrayAll0Last2);
        tree.Get(_keyNonce).Should().BeEquivalentTo(_arrayAll0Last2);
        tree.Get(_keyCodeKeccak).Should().BeEquivalentTo(_valueEmptyCodeHashValue);
        tree.Get(_keyCodeSize).Should().BeEquivalentTo(_arrayAll0Last2);
    }

    [Test]
    public void TestInsertGetMultiBlockReverseState()
    {
        VerkleTree tree = new VerkleTree();

        tree.Insert(_keyVersion, _emptyArray);
        tree.Insert(_keyBalance, _emptyArray);
        tree.Insert(_keyNonce, _emptyArray);
        tree.Insert(_keyCodeKeccak, _valueEmptyCodeHashValue);
        tree.Insert(_keyCodeSize, _emptyArray);
        tree.Flush(0);

        tree.Get(_keyVersion).Should().BeEquivalentTo(_emptyArray);
        tree.Get(_keyBalance).Should().BeEquivalentTo(_emptyArray);
        tree.Get(_keyNonce).Should().BeEquivalentTo(_emptyArray);
        tree.Get(_keyCodeKeccak).Should().BeEquivalentTo(_valueEmptyCodeHashValue);
        tree.Get(_keyCodeSize).Should().BeEquivalentTo(_emptyArray);

        tree.Insert(_keyVersion, _arrayAll0Last2);
        tree.Insert(_keyBalance, _arrayAll0Last2);
        tree.Insert(_keyNonce, _arrayAll0Last2);
        tree.Insert(_keyCodeKeccak, _valueEmptyCodeHashValue);
        tree.Insert(_keyCodeSize, _arrayAll0Last2);
        tree.Flush(1);

        tree.Get(_keyVersion).Should().BeEquivalentTo(_arrayAll0Last2);
        tree.Get(_keyBalance).Should().BeEquivalentTo(_arrayAll0Last2);
        tree.Get(_keyNonce).Should().BeEquivalentTo(_arrayAll0Last2);
        tree.Get(_keyCodeKeccak).Should().BeEquivalentTo(_valueEmptyCodeHashValue);
        tree.Get(_keyCodeSize).Should().BeEquivalentTo(_arrayAll0Last2);

        tree.ReverseState();

        tree.Get(_keyVersion).Should().BeEquivalentTo(_emptyArray);
        tree.Get(_keyBalance).Should().BeEquivalentTo(_emptyArray);
        tree.Get(_keyNonce).Should().BeEquivalentTo(_emptyArray);
        tree.Get(_keyCodeKeccak).Should().BeEquivalentTo(_valueEmptyCodeHashValue);
        tree.Get(_keyCodeSize).Should().BeEquivalentTo(_emptyArray);
    }
}
