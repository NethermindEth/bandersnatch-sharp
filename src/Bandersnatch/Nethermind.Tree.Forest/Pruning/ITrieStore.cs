// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Nethermind.Core.Crypto;
using Nethermind.Utils;
using Nethermind.Utils.Crypto;

namespace Nethermind.Tree.Forest.Pruning
{
    public interface ITrieStore : ITrieNodeResolver, IReadOnlyKeyValueStore, IDisposable
    {
        void CommitNode(long blockNumber, NodeCommitInfo nodeCommitInfo);

        void FinishBlockCommit(TrieType trieType, long blockNumber, TrieNode? root);

        bool IsPersisted(Commitment keccak);

        IReadOnlyTrieStore AsReadOnly(IKeyValueStore? keyValueStore);

        event EventHandler<ReorgBoundaryReached>? ReorgBoundaryReached;
    }
}
