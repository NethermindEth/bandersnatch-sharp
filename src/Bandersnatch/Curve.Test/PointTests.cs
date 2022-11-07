using System;
using Field;
using Nethermind.Int256;
using NUnit.Framework;
using FluentAssertions;

namespace Curve.Test;
using Fp = FixedFiniteField<BandersnatchBaseFieldStruct>;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public class PointTests
{

    [Test]
    public void TestAddition()
    {
        var gen = ExtendedPoint.Generator();
        var resultAdd = gen + gen;

        var resultDouble = ExtendedPoint.Double(gen);
        
        Assert.IsTrue(resultAdd == resultDouble);
    }
    
    [Test]
    public void TestEq()
    {
        var gen = ExtendedPoint.Generator();
        var gen2 = ExtendedPoint.Generator();
        
        var negGen = ExtendedPoint.Neg(gen);
        
        Assert.IsTrue(gen == gen2);
        Assert.IsTrue(gen != negGen);
    }
    
    [Test]
    public void TestNeg()
    {
        var gen = ExtendedPoint.Generator();
        var expected = ExtendedPoint.Identity();
        var result = gen + ExtendedPoint.Neg(gen);
        
        Assert.IsTrue(expected == result);
    }
    
    [Test]
    public void TestSerialiseGen()
    {
        var gen = ExtendedPoint.Generator();

        var serialized = gen.ToBytes();
        const string expected = "18ae52a26618e7e1658499ad22c0792bf342be7b77113774c5340b2ccc32c129";
        Convert.ToHexString(serialized).Should().BeEquivalentTo(expected);
    }
    
    [Test]
    public void TestScalarMulSmoke()
    {
        var gen = ExtendedPoint.Generator();
        var scalar = new Fr((UInt256)2);
        var result = gen * scalar;
        var twoGen = ExtendedPoint.Double(gen);
        Assert.IsTrue(twoGen == result);
    }
    
    [Test]
    public void TestScalarMulMinusOne()
    {
        var gen = ExtendedPoint.Generator();
        
        const int x = -1;
        var scalar = new Fr(x);
        var result = gen * scalar;
        var serialized = result.ToBytes();
        const string expected = "e951ad5d98e7181e99d76452e0e343281295e38d90c602bf824892fd86742c4a";
        Convert.ToHexString(serialized).Should().BeEquivalentTo(expected);
    }
}