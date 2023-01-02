// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Nethermind.Utils.Extensions;

namespace Nethermind.Tree.Forest
{
    public static class MemoryAllowance
    {
        public static long TrieNodeCacheMemory { get; set; } = 128.MB();

        public static int TrieNodeCacheCount => (int)(TrieNodeCacheMemory / PatriciaTree.OneNodeAvgMemoryEstimate);
    }
}
