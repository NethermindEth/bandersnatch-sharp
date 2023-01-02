// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

namespace Nethermind.Tree.Forest.Pruning
{
    public interface IPruningStrategy
    {
        bool PruningEnabled { get; }
        bool ShouldPrune(in long currentMemory);
    }
}
