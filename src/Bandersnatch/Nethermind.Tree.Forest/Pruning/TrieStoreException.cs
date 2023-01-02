// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only 

namespace Nethermind.Tree.Forest.Pruning
{
    [Serializable]
    public class TrieStoreException : TrieException
    {
        public TrieStoreException() { }

        public TrieStoreException(string message) : base(message) { }

        public TrieStoreException(string message, Exception inner) : base(message, inner) { }
    }
}
