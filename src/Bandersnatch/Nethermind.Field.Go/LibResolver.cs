// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using System.Runtime.InteropServices;

namespace Nethermind.Field.Go
{
    public static class LibResolver
    {
        private static int _done;

        public static void Setup()
        {
            if (Interlocked.CompareExchange(ref _done, 1, 0) == 0)
            {
                NativeLibrary.SetDllImportResolver(typeof(GoFieldInterop).Assembly, NativeLib.ImportResolver);
            }
        }
    }
}
