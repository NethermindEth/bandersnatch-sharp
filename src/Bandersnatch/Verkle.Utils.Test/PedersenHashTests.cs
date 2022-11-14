using System.Linq;
using Nethermind.Int256;
using NUnit.Framework;

namespace Verkle.Test;

public class PedersenHashTests
{
    private readonly byte[] _treeKeyPrefixIndexZero =
    {
        24, 203, 148, 64, 90, 4, 172, 63, 91, 103, 58, 173, 125, 107, 176, 236, 255, 23, 135, 61, 129, 207, 123, 108,
        18, 153, 101, 247, 17, 48, 148
    };

    private readonly byte[] _testAddressBytes =
    {
        183, 112, 90, 228, 198, 248, 27, 102, 205, 179, 35, 198, 95, 78, 129, 51, 105, 15, 192, 153
    };


    [Test]
    public void PedersenHashTreeKeys()
    {
        byte[] address32 = ToAddress32(_testAddressBytes);
        Assert.IsTrue(PedersenHash.Hash(address32, UInt256.Zero)[..31].SequenceEqual(_treeKeyPrefixIndexZero));
    }

    [Test]
    public void PedersenHashTreeKeysGeneralized()
    {
        byte[] address32 = ToAddress32(_testAddressBytes);
        Assert.IsTrue(PedersenHash.Hash(new UInt256[] { new UInt256(address32), UInt256.Zero })[..31].SequenceEqual(_treeKeyPrefixIndexZero));
    }

    private static byte[] ToAddress32(byte[] address20)
    {
        byte[] address32 = new byte[32];
        address20.CopyTo(address32, 0);
        return address32;
    }

}
