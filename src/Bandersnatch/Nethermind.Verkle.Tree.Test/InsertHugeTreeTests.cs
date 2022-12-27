// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

using System;
using System.Collections.Generic;
using System.IO;
using Nethermind.Verkle.Db;
using NUnit.Framework;

namespace Nethermind.Verkle.Tree.Test;

[TestFixture]
public class InsertHugeTreeTests
{
    public static Random Random { get; } = new();
    public static int numKeys = 1000;
    private static string GetDbPathForTest()
    {
        string tempDir = Path.GetTempPath();
        string dbname = "VerkleTrie_TestID_" + TestContext.CurrentContext.Test.ID;
        return Path.Combine(tempDir, dbname);
    }

    private static VerkleTree GetVerkleTreeForTest(DbMode dbMode)
    {
        switch (dbMode)
        {
            case DbMode.MemDb:
                return new VerkleTree(dbMode, null);
            case DbMode.PersistantDb:
                return new VerkleTree(dbMode, GetDbPathForTest());
            case DbMode.ReadOnlyDb:
            default:
                throw new ArgumentOutOfRangeException(nameof(dbMode), dbMode, null);
        }
    }

    [TearDown]
    public void CleanTestData()
    {
        string dbPath = GetDbPathForTest();
        if (Directory.Exists(dbPath))
        {
            Directory.Delete(dbPath, true);
        }
    }


    [Test]
    public void CreateDbWith190MillionAccounts()
    {
        const string currDir = "/home/eurus/bandersnatch-sharp/src/Bandersnatch/Nethermind.Verkle.Tree.Test";
        Console.WriteLine(currDir);
        const string dbname = "VerkleTrie190ML";
        string path = Path.Combine(currDir, dbname);

        byte[] stem = new byte[31];
        byte[] val = new byte[32];

        VerkleTree verkleTree = new VerkleTree(DbMode.PersistantDb, path);

        for (int i = 0; i < 190000000; i++)
        {
            Console.WriteLine(i);
            Dictionary<byte, byte[]> leafIndexValueMap = new Dictionary<byte, byte[]>();

            Random.NextBytes(val);
            leafIndexValueMap.Add(0, val);
            Random.NextBytes(val);
            leafIndexValueMap.Add(1, val);
            Random.NextBytes(val);
            leafIndexValueMap.Add(2, val);
            Random.NextBytes(val);
            leafIndexValueMap.Add(3, val);
            Random.NextBytes(val);
            leafIndexValueMap.Add(4, val);

            Random.NextBytes(stem);
            verkleTree.InsertStemBatch(stem, leafIndexValueMap);
            if (i % 1000 == 0)
            {
                verkleTree.Flush(0);
            }
        }

    }

    [TestCase(DbMode.MemDb)]
    [TestCase(DbMode.PersistantDb)]
    public void InsertHugeTree(DbMode dbMode)
    {
        long block = 0;
        VerkleTree tree = GetVerkleTreeForTest(dbMode);
        byte[] key = new byte[32];
        byte[] value = new byte[32];
        DateTime start = DateTime.Now;
        for (int i = 0; i < numKeys; i++)
        {
            Random.NextBytes(key);
            Random.NextBytes(value);
            tree.Insert(key, value);
        }
        DateTime check1 = DateTime.Now;
        tree.Flush(block++);
        DateTime check2 = DateTime.Now;
        Console.WriteLine($"{block} Insert: {(check1 - start).TotalMilliseconds}");
        Console.WriteLine($"{block} Flush: {(check2 - check1).TotalMilliseconds}");
        for (int i = 100; i < numKeys; i += 100)
        {
            DateTime check5 = DateTime.Now;
            Random.NextBytes(key);
            Random.NextBytes(value);
            for (int j = 0; j < i; j += 1)
            {
                Random.NextBytes(key);
                Random.NextBytes(value);
                tree.Insert(key, value);
            }
            DateTime check3 = DateTime.Now;
            tree.Flush(block++);
            DateTime check4 = DateTime.Now;
            Console.WriteLine($"{block} Insert: {(check3 - check5).TotalMilliseconds}");
            Console.WriteLine($"{block} Flush: {(check4 - check3).TotalMilliseconds}");
        }
        DateTime check6 = DateTime.Now;
        Console.WriteLine($"Loop Time: {(check6 - check2).TotalMilliseconds}");
        Console.WriteLine($"Total Time: {(check6 - start).TotalMilliseconds}");
    }
}
