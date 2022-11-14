using System.Linq;
using Nethermind.Int256;
using NUnit.Framework;

namespace Field.Test;

public class FiniteFieldTests
{
    [Test]
    public void TestAddition()
    {
        FiniteField? a = new FiniteField((UInt256)10, (UInt256)13);
        FiniteField? b = new FiniteField((UInt256)30, (UInt256)13);

        FiniteField? got = a + b;
        FiniteField? expected = new FiniteField((UInt256)40, (UInt256)13);
        Assert.True(got == expected);
    }

    [Test]
    public void TestSubtraction()
    {
        FiniteField? a = new FiniteField((UInt256)10, (UInt256)13);
        FiniteField? b = new FiniteField((UInt256)30, (UInt256)13);

        FiniteField? got = a - b;
        FiniteField? expected = new FiniteField(-20, (UInt256)13);
        Assert.True(got == expected);
    }

    [Test]
    public void TestMultiplication()
    {
        FiniteField? a = new FiniteField((UInt256)200, (UInt256)13);
        FiniteField? b = new FiniteField((UInt256)3, (UInt256)13);

        FiniteField? got = a * b;
        FiniteField? expected = new FiniteField((UInt256)2, (UInt256)13);
        Assert.True(got == expected);
    }

    [Test]
    public void TestDivisionInversion()
    {
        FiniteField? a = new FiniteField((UInt256)200, (UInt256)13);
        FiniteField? b = new FiniteField((UInt256)3, (UInt256)13);

        FiniteField? got = a / b;
        FiniteField? expected = FiniteField.Inverse(b);
        expected = expected * a;

        Assert.True(got == expected);
    }

    [Test]
    public void TestInversion()
    {
        FiniteField? b = new FiniteField((UInt256)3, (UInt256)13);

        FiniteField? bInverse = FiniteField.Inverse(b);
        FiniteField? result = b * bInverse;

        FiniteField? expected = new FiniteField((UInt256)1, (UInt256)13);

        Assert.True(result == expected);
    }

    [Test]
    public void TestSqrtSqr()
    {
        FiniteField? b = new FiniteField((UInt256)3, (UInt256)13);
        Assert.True(b.Legendre() == 1);

        FiniteField? bSqrt = FiniteField.Sqrt(b);

        FiniteField? result = FiniteField.ExpMod(bSqrt, 2);

        Assert.True(result == b);
    }

    [Test]
    public void TestNegative()
    {
        FiniteField? b = new FiniteField((UInt256)3, (UInt256)13);
        Assert.True(b.Legendre() == 1);

        FiniteField? bNeg = FiniteField.Neg(b);

        FiniteField? expected = new FiniteField(-3, (UInt256)13);

        Assert.True(expected == bNeg);
    }

    [Test]
    public void TestSerialize()
    {
        FiniteField? b = new FiniteField((UInt256)3, (UInt256)13);

        byte[]? bytes = b.ToBytes();
        FiniteField? desbytes = FiniteField.FromBytes(bytes, 13);

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

        FiniteField[]? gotInverse = FiniteField.MultiInverse(values);
        FiniteField?[]? expectedInverse = NaiveMultiInverse(values);

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
