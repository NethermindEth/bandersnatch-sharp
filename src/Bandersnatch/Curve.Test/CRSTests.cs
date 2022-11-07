using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Field;
using NUnit.Framework;

namespace Curve.Test;

public class CRSTests
{

    [Test]
    public void TestCrsIsConsistent()
    {
        var crs = CRSStruct.GetCRS();
        Assert.IsTrue(256 == crs.Length);
        
        var gotFirstPoint = Convert.ToHexString(crs[0].ToBytes()).ToLower();
        const string expectedFistPoint = "01587ad1336675eb912550ec2a28eb8923b824b490dd2ba82e48f14590a298a0";
        Assert.IsTrue(gotFirstPoint.SequenceEqual(expectedFistPoint));

        var got256ThPoint = Convert.ToHexString(crs[255].ToBytes()).ToLower();
        const string expected256ThPoint = "3de2be346b539395b0c0de56a5ccca54a317f1b5c80107b0802af9a62276a4d8";
        Assert.IsTrue(got256ThPoint.SequenceEqual(expected256ThPoint));

        var hasher = SHA256.Create();
        List<byte> hashData = new List<byte>();
        foreach (var point in crs)
        {
            hashData.AddRange(point!.ToBytes());
        }

        var result = Convert.ToHexString(hasher.ComputeHash(hashData.ToArray())).ToLower();

        Assert.IsTrue(result.SequenceEqual("1fcaea10bf24f750200e06fa473c76ff0468007291fa548e2d99f09ba9256fdb"));
    }
    
    [Test]
    public void TestCrsNotGenerator()
    {
        var crs = CRSStruct.GetCRS();
        var generator = Banderwagon.Generator();

        foreach (var point in crs)
        {
            Assert.IsTrue(generator != point);
        }
    }
}