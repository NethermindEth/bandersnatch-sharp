// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

using Nethermind.Db;
using Nethermind.Serialization.Rlp;
using Nethermind.Utils;
using Nethermind.Verkle.Db;
using Nethermind.Verkle.Tree.VerkleNodes;

namespace Nethermind.Verkle.Tree.VerkleStateDb;

public class DiskDb: IVerkleDb
{
    private readonly IDbProvider _dbProvider;

    public IDb LeafDb => _dbProvider.LeafDb;
    public IDb StemDb => _dbProvider.StemDb;
    public IDb BranchDb => _dbProvider.BranchDb;
    public DiskDb(DbMode dbMode, string? dbPath)
    {
        _dbProvider = DbFactory.InitDatabase(dbMode, dbPath);
    }
    public byte[]? GetLeaf(byte[] key) => LeafDb[key];

    public SuffixTree? GetStem(byte[] key)
    {
        return StemDb[key] is null ? null : SuffixTreeSerializer.Instance.Decode(StemDb[key]);
    }

    public InternalNode? GetBranch(byte[] key)
    {
        return BranchDb[key] is null ? null : InternalNodeSerializer.Instance.Decode(BranchDb[key]);
    }

    public void SetLeaf(byte[] leafKey, byte[]? leafValue, IKeyValueStore db) => db[leafKey] = leafValue;

    public void SetStem(byte[] stemKey, SuffixTree? suffixTree, IKeyValueStore db) => db[stemKey] = SuffixTreeSerializer.Instance.Encode(suffixTree).Bytes;

    public void SetBranch(byte[] branchKey, InternalNode? internalNodeValue, IKeyValueStore db) => db[branchKey] = InternalNodeSerializer.Instance.Encode(internalNodeValue).Bytes;

    public bool GetLeaf(byte[] key, out byte[]? value)
    {
        value = GetLeaf(key);
        return value is not null;
    }
    public bool GetStem(byte[] key, out SuffixTree? value)
    {
        value = GetStem(key);
        return value is not null;
    }
    public bool GetBranch(byte[] key, out InternalNode? value)
    {
        value = GetBranch(key);
        return value is not null;
    }
    public void SetLeaf(byte[] leafKey, byte[] leafValue) => SetLeaf(leafKey, leafValue, LeafDb);
    public void SetStem(byte[] stemKey, SuffixTree suffixTree) => SetStem(stemKey, suffixTree, StemDb);
    public void SetBranch(byte[] branchKey, InternalNode internalNodeValue) => SetBranch(branchKey, internalNodeValue, BranchDb);
    public void RemoveLeaf(byte[] leafKey)
    {
        LeafDb.Remove(leafKey);
    }
    public void RemoveStem(byte[] stemKey)
    {
        StemDb.Remove(stemKey);
    }
    public void RemoveBranch(byte[] branchKey)
    {
        BranchDb.Remove(branchKey);
    }

    public void BatchLeafInsert(IEnumerable<KeyValuePair<byte[], byte[]?>> keyLeaf)
    {
        using IBatch batch = LeafDb.StartBatch();
        foreach ((byte[] key, byte[]? value) in keyLeaf)
        {
            SetLeaf(key, value, batch);
        }
    }
    public void BatchStemInsert(IEnumerable<KeyValuePair<byte[], SuffixTree?>> suffixLeaf)
    {
        using IBatch batch = StemDb.StartBatch();
        foreach ((byte[] key, SuffixTree? value) in suffixLeaf)
        {
            SetStem(key, value, batch);
        }
    }
    public void BatchBranchInsert(IEnumerable<KeyValuePair<byte[], InternalNode?>> branchLeaf)
    {
        using IBatch batch = BranchDb.StartBatch();
        foreach ((byte[] key, InternalNode? value) in branchLeaf)
        {
            SetBranch(key, value, batch);
        }
    }
}
