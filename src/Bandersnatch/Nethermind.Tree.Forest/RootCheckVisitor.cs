// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Nethermind.Core.Crypto;
using Nethermind.Utils.Crypto;

namespace Nethermind.Tree.Forest
{
    public class RootCheckVisitor : ITreeVisitor
    {
        public bool HasRoot { get; set; } = true;

        public bool ShouldVisit(Commitment nextNode)
        {
            return false;
        }

        public void VisitTree(Commitment rootHash, TrieVisitContext trieVisitContext)
        {
        }

        public void VisitMissingNode(Commitment nodeHash, TrieVisitContext trieVisitContext)
        {
            HasRoot = false;
        }

        public void VisitBranch(TrieNode node, TrieVisitContext trieVisitContext)
        {
        }

        public void VisitExtension(TrieNode node, TrieVisitContext trieVisitContext)
        {
        }

        public void VisitLeaf(TrieNode node, TrieVisitContext trieVisitContext, byte[] value = null)
        {
        }

        public void VisitCode(Commitment codeHash, TrieVisitContext trieVisitContext)
        {
        }
    }
}
