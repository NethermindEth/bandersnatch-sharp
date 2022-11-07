using System.Linq;
using Nethermind.Int256;
using NUnit.Framework;

namespace Field.Test;

public class FiniteFieldTests
{

    [Test]
    public void TestAddition()
    {
        var a = new FiniteField((UInt256)10, (UInt256)13);
        var b = new FiniteField((UInt256)30, (UInt256)13);

        var got = a + b;
        var expected = new FiniteField((UInt256)40, (UInt256)13);
        Assert.True(got == expected);
    }

    [Test]
    public void TestSubtraction()
    {
        var a = new FiniteField((UInt256)10, (UInt256)13);
        var b = new FiniteField((UInt256)30, (UInt256)13);

        var got = a - b;
        var expected = new FiniteField(-20, (UInt256)13);
        Assert.True(got == expected);
    }

    [Test]
    public void TestMultiplication()
    {
        var a = new FiniteField((UInt256)200, (UInt256)13);
        var b = new FiniteField((UInt256)3, (UInt256)13);

        var got = a * b;
        var expected = new FiniteField((UInt256)2, (UInt256)13);
        Assert.True(got == expected);
    }

    [Test]
    public void TestDivisionInversion()
    {
        var a = new FiniteField((UInt256)200, (UInt256)13);
        var b = new FiniteField((UInt256)3, (UInt256)13);

        var got = a / b;
        var expected = FiniteField.Inverse(b);
        expected = expected * a;

        Assert.True(got == expected);
    }

    [Test]
    public void TestInversion()
    {
        var b = new FiniteField((UInt256)3, (UInt256)13);

        var bInverse = FiniteField.Inverse(b);
        var result = b * bInverse;

        var expected = new FiniteField((UInt256)1, (UInt256)13);

        Assert.True(result == expected);
    }

    [Test]
    public void TestSqrtSqr()
    {
        var b = new FiniteField((UInt256)3, (UInt256)13);
        Assert.True(b.Legendre() == 1);

        var bSqrt = FiniteField.Sqrt(b);

        var result = FiniteField.ExpMod(bSqrt, 2);

        Assert.True(result == b);
    }

    [Test]
    public void TestNegative()
    {
        var b = new FiniteField((UInt256)3, (UInt256)13);
        Assert.True(b.Legendre() == 1);

        var bNeg = FiniteField.Neg(b);

        var expected = new FiniteField(-3, (UInt256)13);

        Assert.True(expected == bNeg);
    }

    [Test]
    public void TestSerialize()
    {
        var b = new FiniteField((UInt256)3, (UInt256)13);

        var bytes = b.ToBytes();
        var desbytes = FiniteField.FromBytes(bytes, 13);

        Assert.True(desbytes == b);
    }

    [Test]
    public void TestMultiInv()
    {
        FiniteField[] values =
        {
            new FiniteField((UInt256) 1, (UInt256) 13),
            new FiniteField((UInt256) 2, (UInt256) 13),
            new FiniteField((UInt256) 3, (UInt256) 13)
        };

        var gotInverse = FiniteField.MultiInverse(values);
        var expectedInverse = NaiveMultiInverse(values);

        Assert.IsTrue(gotInverse.Length == expectedInverse.Length);
        for (int i = 0; i < gotInverse.Length; i++)
        {
            Assert.IsTrue(gotInverse[i] == expectedInverse[i]);
        }
    }

    private FiniteField?[] NaiveMultiInverse(FiniteField[] values)
    {
        return values.Select(t => FiniteField.Inverse(t)).ToArray();
    }


}