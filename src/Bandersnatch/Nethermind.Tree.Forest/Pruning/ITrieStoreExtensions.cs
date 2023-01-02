// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Nethermind.Utils;

namespace Nethermind.Tree.Forest.Pruning
{
    // ReSharper disable once InconsistentNaming
    public static class ITrieStoreExtensions
    {
        public static IReadOnlyTrieStore AsReadOnly(this ITrieStore trieStore, IKeyValueStore? readOnlyStore = null) =>
            trieStore.AsReadOnly(readOnlyStore);
    }
}
