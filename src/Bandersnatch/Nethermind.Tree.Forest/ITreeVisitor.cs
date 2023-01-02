// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Nethermind.Core.Crypto;
using Nethermind.Utils.Crypto;

namespace Nethermind.Tree.Forest
{
    public interface ITreeVisitor
    {
        bool ShouldVisit(Commitment nextNode);

        void VisitTree(Commitment rootHash, TrieVisitContext trieVisitContext);

        void VisitMissingNode(Commitment nodeHash, TrieVisitContext trieVisitContext);

        void VisitBranch(TrieNode node, TrieVisitContext trieVisitContext);

        void VisitExtension(TrieNode node, TrieVisitContext trieVisitContext);

        void VisitLeaf(TrieNode node, TrieVisitContext trieVisitContext, byte[] value = null);

        void VisitCode(Commitment codeHash, TrieVisitContext trieVisitContext);
    }
}
