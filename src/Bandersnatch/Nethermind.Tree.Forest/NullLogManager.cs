// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

namespace Nethermind.Tree.Forest
{
    public class NullLogManager : ILogManager
    {
        private NullLogManager()
        {
        }

        public static ILogManager Instance { get; } = new NullLogManager();

        public ILogger GetClassLogger(Type type)
        {
            return NullLogger.Instance;
        }

        public ILogger GetClassLogger<T>()
        {
            return NullLogger.Instance;
        }

        public ILogger GetClassLogger()
        {
            return NullLogger.Instance;
        }

        public ILogger GetLogger(string loggerName)
        {
            return NullLogger.Instance;
        }
    }
}
