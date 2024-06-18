// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using System.Reflection;
using System.Runtime.InteropServices;

namespace Nethermind.RustVerkle;

public enum OsPlatform
{
    Windows,
    Linux,
    LinuxArm64,
    LinuxArm,
    MacArm64,
    Mac
}

public static class NativeLib
{
    private static OsPlatform GetPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.OSArchitecture.ToString() == "Arm")
        {
            return OsPlatform.LinuxArm;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.OSArchitecture.ToString() == "Arm64")
        {
            return OsPlatform.LinuxArm64;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && RuntimeInformation.OSArchitecture.ToString() == "Arm64")
        {
            return OsPlatform.MacArm64;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return OsPlatform.Windows;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return OsPlatform.Linux;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return OsPlatform.Mac;
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            return OsPlatform.Linux;
        }

        throw new InvalidOperationException("Unsupported platform.");
    }

    public static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        OsPlatform platform = GetPlatform();
        string libPath = platform switch
        {
            OsPlatform.Linux => $"RustVerkle/runtimes/linux-x64/lib{libraryName}.so",
            OsPlatform.Mac => $"RustVerkle/runtimes/osx-x64/native/lib{libraryName}.dylib",
            OsPlatform.Windows => $"RustVerkle/runtimes\\win-x64\\{libraryName}.dll",
            OsPlatform.LinuxArm => $"RustVerkle/runtimes/linux-arm/lib{libraryName}.so",
            OsPlatform.LinuxArm64 => $"RustVerkle/runtimes/linux-arm64/lib{libraryName}.so",
            OsPlatform.MacArm64 => $"RustVerkle/runtimes/osx-arm64/lib{libraryName}.dylib",
            _ => throw new NotSupportedException($"Platform support missing: {platform}")
        };

        // Console.WriteLine($"Trying to load a lib {libraryName} from {libPath} with search path {searchPath} for asembly {assembly} on platform {platform}");
        NativeLibrary.TryLoad(libPath, assembly, searchPath, out IntPtr libHandle);
        return libHandle;
    }
}
