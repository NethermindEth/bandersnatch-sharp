// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Security;

namespace Nethermind.Field.Go
{
    public static partial class GoFieldInterop
    {
        [SuppressUnmanagedCodeSecurity]
        [LibraryImport("/home/sherlock/bandersnatch-fields/fp/a.so")]
        public static partial void CMul(ulong[] input1, ulong a0, ulong a1, ulong a2, ulong a3, ulong b0, ulong b1, ulong b2, ulong b3);
    }

    // public class ModularMultiplication
    // {
    //     static void Main(string[] args)
    //     {
    //         Ba
    //         // Modulus
    //         var modulus = new byte[32];
    //         var modulusBigInt = new BigInteger(13108968793781547619861935127046491459309155893440570251786403306729687672801);
    //         modulusBigInt.ToByteArray().CopyTo(modulus, 0);
    //         // Multiplicand
    //         var a = new byte[32];
    //         var aBigInt = BigInteger.Parse("9876543210987654321098765432109876543210987654321098765432109876");
    //         aBigInt.ToByteArray().CopyTo(a, 0);
    //         // Multiplier
    //         var b = new byte[32];
    //         var bBigInt = BigInteger.Parse("1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef");
    //         bBigInt.ToByteArray().CopyTo(b, 0);
    //
    //         var c = new byte[64];
    //         var n = new byte[32];
    //         var k = new byte[32];
    //
    //         Array.Copy(modulus, n, 32);
    //         Array.Copy(a, k, 32);
    //
    //         var res = new byte[64];
    //         var nInv = new byte[32];
    //
    //         using (var barrett = new BarrettReduction(n))
    //         {
    //             barrett.ModInverse(nInv);
    //             barrett.Multiply(res, k, b);
    //         }
    //         Array.Copy(res, c, 32);
    //         Console.WriteLine("Result: " + BitConverter.ToString(c).Replace("-", ""));
    //     }
    // }
}
