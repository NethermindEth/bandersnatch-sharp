// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Nethermind.Utils;

namespace Nethermind.Tree.Forest
{
    public class NullKeyValueStore : IKeyValueStore
    {
        private NullKeyValueStore()
        {
        }

        private static NullKeyValueStore _instance;
        public static NullKeyValueStore Instance => LazyInitializer.EnsureInitialized(ref _instance, () => new NullKeyValueStore());

        public byte[] this[byte[] key]
        {
            get => null;
            set => throw new NotSupportedException();
        }
    }
}
