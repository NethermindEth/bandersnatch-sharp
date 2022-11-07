using System.Linq;
using Nethermind.Int256;
using NUnit.Framework;

namespace Field.Test;
using Ft = FixedFiniteField<TestBandersnatchScalarFieldStruct>;

public struct TestBandersnatchScalarFieldStruct: IFieldDefinition
{
    private static readonly byte[] ModBytes =
    {
        225, 231, 118, 40, 181, 6, 253, 116, 113, 4, 25, 116, 0, 135, 143, 255, 0, 118, 104, 2, 2, 118, 206, 12, 82, 95,
        103, 202, 212, 105, 251, 28
    };
    public readonly UInt256 FieldMod => new (ModBytes);
}

public class FixedFiniteFieldTests
{
    [Test]
    public void TestAddition()
    {
        var a = new Ft((UInt256)10);
        var b = new Ft((UInt256)30);

        var got = a + b;
        var expected = new Ft((UInt256)40);
        Assert.True(got == expected);
    }

    [Test]
    public void TestSubtraction()
    {
        var a = new Ft((UInt256)10);
        var b = new Ft((UInt256)30);

        var got = a - b;
        var expected = new Ft(-20);
        Assert.True(got == expected);
    }

    [Test]
    public void TestMultiplication()
    {
        var a = new Ft((UInt256)200);
        var b = new Ft((UInt256)3);

        var got = a * b;
        var expected = new Ft((UInt256)600);
        Assert.True(got == expected);
    }

    [Test]
    public void TestDivisionInversion()
    {
        var a = new Ft((UInt256)200);
        var b = new Ft((UInt256)3);

        var got = a / b;
        var expected = Ft.Inverse(b);
        expected = expected * a;

        Assert.True(got == expected);
    }

    [Test]
    public void TestInversion()
    {
        var b = new Ft((UInt256)3);

        var bInverse = Ft.Inverse(b);
        var result = b * bInverse;

        var expected = new Ft((UInt256)1);

        Assert.True(result == expected);
    }

    [Test]
    public void TestSqrtSqr()
    {
        var b = new Ft((UInt256)3);
        Assert.True(b.Legendre() == 1);

        var bSqrt = Ft.Sqrt(b);

        var result = Ft.ExpMod(bSqrt, 2);

        Assert.True(result == b);
    }

    [Test]
    public void TestNegative()
    {
        var b = new Ft((UInt256)3);
        Assert.True(b.Legendre() == 1);

        var bNeg = Ft.Neg(b);

        var expected = new Ft(-3);

        Assert.True(expected == bNeg);
    }

    [Test]
    public void TestSerialize()
    {
        var b = new Ft((UInt256)3);

        var bytes = b.ToBytes();
        var desbytes = Ft.FromBytes(bytes, 13);

        Assert.True(desbytes == b);
    }

    [Test]
    public void TestMultiInv()
    {
        Ft[] values =
        {
            new Ft((UInt256) 1),
            new Ft((UInt256) 2),
            new Ft((UInt256) 3)
        };

        var gotInverse = Ft.MultiInverse(values);
        var expectedInverse = NaiveMultiInverse(values);

        Assert.IsTrue(gotInverse.Length == expectedInverse.Length);
        for (int i = 0; i < gotInverse.Length; i++)
        {
            Assert.IsTrue(gotInverse[i] == expectedInverse[i]);
        }
    }

    private Ft?[] NaiveMultiInverse(Ft[] values)
    {
        return values.Select(t => Ft.Inverse(t)).ToArray();
    }
}