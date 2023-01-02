// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

namespace Nethermind.Tree.Forest;
public interface ILogger
{
    void Info(string text);
    void Warn(string text);
    void Debug(string text);
    void Trace(string text);
    void Error(string text, Exception ex = null);

    bool IsInfo { get; }
    bool IsWarn { get; }
    bool IsDebug { get; }
    bool IsTrace { get; }
    bool IsError { get; }
}


