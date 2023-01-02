// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

namespace Nethermind.Tree.Forest
{
    public interface ILogManager
    {
        ILogger GetClassLogger(Type type);
        ILogger GetClassLogger<T>();
        ILogger GetClassLogger();
        ILogger GetLogger(string loggerName);

        void SetGlobalVariable(string name, object value) { }
    }
}
