// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Nethermind.Core.Crypto;
using Nethermind.Serialization.Rlp;
using Nethermind.Utils.Crypto;

[assembly: InternalsVisibleTo("Ethereum.Trie.Test")]
[assembly: InternalsVisibleTo("Nethermind.Blockchain.Test")]
[assembly: InternalsVisibleTo("Nethermind.Trie.Test")]

namespace Nethermind.Tree.Forest
{
    public partial class TrieNode
    {
        private class TrieNodeDecoder
        {
            public byte[] Encode(Pruning.ITrieNodeResolver tree, TrieNode? item)
            {
                Metrics.TreeNodeRlpEncodings++;

                if (item is null)
                {
                    throw new TrieException("An attempt was made to RLP encode a null node.");
                }

                return item.NodeType switch
                {
                    NodeType.Branch => RlpEncodeBranch(tree, item),
                    NodeType.Extension => EncodeExtension(tree, item),
                    NodeType.Leaf => EncodeLeaf(item),
                    _ => throw new TrieException($"An attempt was made to encode a trie node of type {item.NodeType}")
                };
            }

            private static byte[] EncodeExtension(Pruning.ITrieNodeResolver tree, TrieNode item)
            {
                Debug.Assert(item.NodeType == NodeType.Extension,
                    $"Node passed to {nameof(EncodeExtension)} is {item.NodeType}");
                Debug.Assert(item.Key is not null,
                    "Extension key is null when encoding");

                byte[] keyBytes = item.Key.ToBytes();
                TrieNode nodeRef = item.GetChild(tree, 0);
                Debug.Assert(nodeRef is not null,
                    "Extension child is null when encoding.");

                nodeRef.ResolveKey(tree, false);

                int contentLength = Rlp.LengthOf(keyBytes) + (nodeRef.Commitment is null ? nodeRef.FullRlp.Length : Rlp.LengthOfCommitmentRlp);
                int totalLength = Rlp.LengthOfSequence(contentLength);
                RlpStream rlpStream = new(totalLength);
                rlpStream.StartSequence(contentLength);
                rlpStream.Encode(keyBytes);
                if (nodeRef.Commitment is null)
                {
                    // I think it can only happen if we have a short extension to a branch with a short extension as the only child?
                    // so |
                    // so |
                    // so E - - - - - - - - - - - - - - -
                    // so |
                    // so |
                    rlpStream.Write(nodeRef.FullRlp);
                }
                else
                {
                    rlpStream.Encode(nodeRef.Commitment);
                }

                return rlpStream.Data;
            }

            private static byte[] EncodeLeaf(TrieNode node)
            {
                if (node.Key is null)
                {
                    throw new TrieException($"Hex prefix of a leaf node is null at node {node.Commitment}");
                }

                byte[] keyBytes = node.Key.ToBytes();
                int contentLength = Rlp.LengthOf(keyBytes) + Rlp.LengthOf(node.Value);
                int totalLength = Rlp.LengthOfSequence(contentLength);
                RlpStream rlpStream = new(totalLength);
                rlpStream.StartSequence(contentLength);
                rlpStream.Encode(keyBytes);
                rlpStream.Encode(node.Value);
                return rlpStream.Data;
            }

            private static byte[] RlpEncodeBranch(Pruning.ITrieNodeResolver tree, TrieNode item)
            {
                int valueRlpLength = AllowBranchValues ? Rlp.LengthOf(item.Value) : 1;
                int contentLength = valueRlpLength + GetChildrenRlpLength(tree, item);
                int sequenceLength = Rlp.LengthOfSequence(contentLength);
                byte[] result = new byte[sequenceLength];
                Span<byte> resultSpan = result.AsSpan();
                int position = Rlp.StartSequence(result, 0, contentLength);
                WriteChildrenRlp(tree, item, resultSpan.Slice(position, contentLength - valueRlpLength));
                position = sequenceLength - valueRlpLength;
                if (AllowBranchValues)
                {
                    Rlp.Encode(result, position, item.Value);
                }
                else
                {
                    result[position] = 128;
                }

                return result;
            }

            private static int GetChildrenRlpLength(Pruning.ITrieNodeResolver tree, TrieNode item)
            {
                int totalLength = 0;
                item.InitData();
                item.SeekChild(0);
                for (int i = 0; i < BranchesCount; i++)
                {
                    if (item._rlpStream is not null && item._data![i] is null)
                    {
                        (int prefixLength, int contentLength) = item._rlpStream.PeekPrefixAndContentLength();
                        totalLength += prefixLength + contentLength;
                    }
                    else
                    {
                        if (ReferenceEquals(item._data![i], _nullNode) || item._data[i] is null)
                        {
                            totalLength++;
                        }
                        else if (item._data[i] is Commitment)
                        {
                            totalLength += Rlp.LengthOfCommitmentRlp;
                        }
                        else
                        {
                            TrieNode childNode = (TrieNode)item._data[i];
                            childNode!.ResolveKey(tree, false);
                            totalLength += childNode.Commitment is null ? childNode.FullRlp!.Length : Rlp.LengthOfCommitmentRlp;
                        }
                    }

                    item._rlpStream?.SkipItem();
                }

                return totalLength;
            }

            private static void WriteChildrenRlp(Pruning.ITrieNodeResolver tree, TrieNode item, Span<byte> destination)
            {
                int position = 0;
                RlpStream rlpStream = item._rlpStream;
                item.InitData();
                item.SeekChild(0);
                for (int i = 0; i < BranchesCount; i++)
                {
                    if (rlpStream is not null && item._data![i] is null)
                    {
                        int length = rlpStream.PeekNextRlpLength();
                        Span<byte> nextItem = rlpStream.Data.AsSpan().Slice(rlpStream.Position, length);
                        nextItem.CopyTo(destination.Slice(position, nextItem.Length));
                        position += nextItem.Length;
                        rlpStream.SkipItem();
                    }
                    else
                    {
                        rlpStream?.SkipItem();
                        if (ReferenceEquals(item._data![i], _nullNode) || item._data[i] is null)
                        {
                            destination[position++] = 128;
                        }
                        else if (item._data[i] is Commitment)
                        {
                            position = Rlp.Encode(destination, position, (item._data[i] as Commitment)!.Bytes);
                        }
                        else
                        {
                            TrieNode childNode = (TrieNode)item._data[i];
                            childNode!.ResolveKey(tree, false);
                            if (childNode.Commitment is null)
                            {
                                Span<byte> fullRlp = childNode.FullRlp.AsSpan();
                                fullRlp.CopyTo(destination.Slice(position, fullRlp.Length));
                                position += fullRlp.Length;
                            }
                            else
                            {
                                position = Rlp.Encode(destination, position, childNode.Commitment.Bytes);
                            }
                        }
                    }
                }
            }
        }
    }
}
