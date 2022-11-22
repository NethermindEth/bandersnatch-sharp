// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

using Nethermind.Serialization.Rlp;
using Nethermind.Verkle.Curve;
using Nethermind.Verkle.Tree.VerkleNodes;
using Nethermind.Verkle.Utils;

namespace Nethermind.Verkle.Tree.VerkleStateDb;
using LeafStore = Dictionary<byte[], byte[]?>;
using SuffixStore = Dictionary<byte[], SuffixTree?>;
using BranchStore = Dictionary<byte[], InternalNode?>;


public class LeafStoreSerializer: IRlpStreamDecoder<LeafStore>
{
    public static LeafStoreSerializer Instance => new LeafStoreSerializer();
    public int GetLength(LeafStore item, RlpBehaviors rlpBehaviors)
    {
        int length = Rlp.LengthOf(item.Count);
        foreach (KeyValuePair<byte[], byte[]?> pair in item)
        {
            length += Rlp.LengthOf(pair.Key);
            length += Rlp.LengthOf(pair.Value);
        }
        return length;
    }

    public LeafStore Decode(RlpStream rlpStream, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
    {
        LeafStore item = new LeafStore();
        int length = rlpStream.DecodeInt();
        for (int i = 0; i < length; i++)
        {
            item[rlpStream.DecodeByteArray()] = rlpStream.DecodeByteArray();
        }
        return item;
    }

    public void Encode(RlpStream stream, LeafStore item, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
    {
        stream.Encode(item.Count);
        foreach (KeyValuePair<byte[], byte[]?> pair in item)
        {
            stream.Encode(pair.Key);
            stream.Encode(pair.Value);
        }
    }
}

public class SuffixStoreSerializer: IRlpStreamDecoder<SuffixStore>
{
    private static SuffixTreeSerializer SuffixTreeSerializer => SuffixTreeSerializer.Instance;

    public static SuffixStoreSerializer Instance => new SuffixStoreSerializer();

    public int GetLength(SuffixStore item, RlpBehaviors rlpBehaviors)
    {
        int length = Rlp.LengthOf(item.Count);
        foreach (KeyValuePair<byte[], SuffixTree?> pair in item)
        {
            length += Rlp.LengthOf(pair.Key);
            length += pair.Value == null? Rlp.EmptyArrayByte: SuffixTreeSerializer.GetLength(pair.Value, RlpBehaviors.None);
        }
        return length;
    }

    public SuffixStore Decode(RlpStream rlpStream, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
    {
        SuffixStore item = new SuffixStore();
        int length = rlpStream.DecodeInt();
        for (int i = 0; i < length; i++)
        {
            byte[] key = rlpStream.DecodeByteArray();
            if (rlpStream.PeekNextItem().Length == 0)
            {
                item[key] = null;
                rlpStream.SkipItem();
            }
            else
            {
                item[key] = SuffixTreeSerializer.Decode(rlpStream);
            }
        }
        return item;
    }
    public void Encode(RlpStream stream, SuffixStore item, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
    {
        stream.Encode(item.Count);
        foreach (KeyValuePair<byte[], SuffixTree?> pair in item)
        {
            stream.Encode(pair.Key);
            if (pair.Value is null) stream.EncodeEmptyByteArray();
            else SuffixTreeSerializer.Encode(stream, pair.Value);
        }
    }
}


public class BranchStoreSerializer: IRlpStreamDecoder<BranchStore>
{
    private static InternalNodeSerializer InternalNodeSerializer => InternalNodeSerializer.Instance;

    public static BranchStoreSerializer Instance => new BranchStoreSerializer();
    public int GetLength(BranchStore item, RlpBehaviors rlpBehaviors)
    {
        int length = Rlp.LengthOf(item.Count);
        foreach (KeyValuePair<byte[], InternalNode?> pair in item)
        {
            length += Rlp.LengthOf(pair.Key);
            length += pair.Value == null? Rlp.EmptyArrayByte: InternalNodeSerializer.GetLength(pair.Value, RlpBehaviors.None);
        }
        return length;
    }

    public BranchStore Decode(RlpStream rlpStream, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
    {
        BranchStore item = new BranchStore();
        int length = rlpStream.DecodeInt();
        for (int i = 0; i < length; i++)
        {
            byte[] key = rlpStream.DecodeByteArray();
            if (rlpStream.PeekNextItem().Length == 0)
            {
                item[key] = null;
                rlpStream.SkipItem();
            }
            else
            {
                item[key] = InternalNodeSerializer.Decode(rlpStream);
            }
        }
        return item;
    }
    public void Encode(RlpStream stream, BranchStore item, RlpBehaviors rlpBehaviors = RlpBehaviors.None)
    {
        stream.Encode(item.Count);
        foreach (KeyValuePair<byte[], InternalNode?> pair in item)
        {
            stream.Encode(pair.Key);
            if (pair.Value is null) stream.EncodeEmptyByteArray();
            else InternalNodeSerializer.Encode(stream, pair.Value);
        }
    }
}
