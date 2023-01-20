using System;
using System.Linq;
using Nethermind.Int256;
using NUnit.Framework;

namespace Nethermind.Verkle.Utils.Test
{
    [TestFixture]
    public class PedersenHashTests
    {
        private readonly byte[] _treeKeyPrefixIndexZero;

        private readonly byte[] _testAddressBytes;
        public PedersenHashTests()
        {
            _treeKeyPrefixIndexZero = new byte[]
            {
                24, 203, 148, 64, 90, 4, 172, 63, 91, 103, 58, 173, 125, 107, 176, 236, 255, 23, 135, 61, 129, 207, 123, 108, 18, 153, 101, 247, 17, 48, 148
            };
            _testAddressBytes = new byte[]
            {
                183, 112, 90, 228, 198, 248, 27, 102, 205, 179, 35, 198, 95, 78, 129, 51, 105, 15, 192, 153
            };
        }

        [Test]
        public void PedersenHashTreeKeys()
        {
            Span<byte> address32 = VerkleUtils.ToAddress32(_testAddressBytes);
            Assert.IsTrue(PedersenHash.Hash(address32, UInt256.Zero)[..31].SequenceEqual(_treeKeyPrefixIndexZero));
        }


        [Test]
        public void PedersenHashTreeKeysGeneralized()
        {
            Span<byte> address32 = VerkleUtils.ToAddress32(_testAddressBytes);
            byte[]? hash = PedersenHash.Hash(new[]
            {
                new UInt256(address32), UInt256.Zero
            });
            if (hash.Length >= 31) Assert.IsTrue(hash[..31].SequenceEqual(_treeKeyPrefixIndexZero));
        }

        [Test]
        public void ProfilePedersenHashTreeKeys()
        {
            for (int i = 0; i < 1000; i++)
            {
                Span<byte> address32 = VerkleUtils.ToAddress32(_testAddressBytes);
                PedersenHash.Hash(address32, UInt256.Zero);
            }
        }
    }
}
