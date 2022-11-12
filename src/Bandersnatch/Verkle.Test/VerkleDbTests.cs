using System.Collections.Generic;
using NUnit.Framework;

namespace Verkle.Test;

public class VerkleDbTests
{
    [Test]
    public void ByteArrayEqualityTestsDictionary()
    {
        byte[] a = {1, 2};
        byte[] b = {1, 2};
        
        Dictionary<byte[], byte[]> table = new()
        {
            [a] = b,
        };
        Assert.IsFalse(table.TryGetValue(b, out _));
        
        table = new Dictionary<byte[], byte[]>(new ByteArrayComparer())
        {
            [a] = b,
        };
        Assert.IsTrue(table.TryGetValue(b, out _));
    }
}