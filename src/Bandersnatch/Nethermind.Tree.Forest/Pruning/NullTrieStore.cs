// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Nethermind.Core.Crypto;
using Nethermind.Utils;
using Nethermind.Utils.Crypto;

namespace Nethermind.Tree.Forest.Pruning
{
    public class NullTrieStore : IReadOnlyTrieStore
    {
        private NullTrieStore() { }

        public static NullTrieStore Instance { get; } = new();

        public void CommitNode(long blockNumber, NodeCommitInfo nodeCommitInfo) { }

        public void FinishBlockCommit(TrieType trieType, long blockNumber, TrieNode? root) { }

        public void HackPersistOnShutdown() { }

        public IReadOnlyTrieStore AsReadOnly(IKeyValueStore keyValueStore)
        {
            return this;
        }

        public event EventHandler<ReorgBoundaryReached> ReorgBoundaryReached
        {
            add { }
            remove { }
        }

        public TrieNode FindCachedOrUnknown(Commitment hash)
        {
            return new(NodeType.Unknown, hash);
        }

        public byte[] LoadRlp(Commitment hash)
        {
            return Array.Empty<byte>();
        }

        public bool IsPersisted(Commitment keccak) => true;

        public void Dispose() { }

        public byte[]? this[byte[] key] => null;
    }
}
