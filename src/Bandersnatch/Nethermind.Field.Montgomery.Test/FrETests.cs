using System;
using System.Collections.Generic;
using Nethermind.Field.Montgomery.FrEElement;
using NUnit.Framework;

namespace Nethermind.Field.Montgomery.Test;
public class FrETests
{
    [Test]
    public void TestNegativeValues()
    {
        using IEnumerator<FrE> set = FrE.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            FrE x = set.Current;
            FrE y = 0 - x;
            FrE z = y + x;
            Assert.IsTrue(z.IsZero);
            set.MoveNext();
        }
    }

    [Test]
    public void TestAddition()
    {
        FrE X = FrE.qElement - 1;
        FrE Y = X + X;
        FrE Z = Y - X;
        Assert.IsTrue(Z.Equals(X));

        using IEnumerator<FrE> set = FrE.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            FrE x = set.Current;
            FrE y = x + x + x + x;
            FrE z = y - x - x - x - x;
            Assert.IsTrue(z.IsZero);
            set.MoveNext();
        }
    }

    [Test]
    public void TestInverse()
    {
        using IEnumerator<FrE> set = FrE.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            FrE x = set.Current;
            if (x.IsZero)
            {
                set.MoveNext();
                continue;
            }
            FrE.Inverse(x, out FrE y);
            FrE.Inverse(y, out FrE z);
            Assert.IsTrue(z.Equals(x));
            set.MoveNext();
        }
    }

    [Test]
    public void TestInverseMultiplication()
    {
        using IEnumerator<FrE> set = FrE.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            FrE x = set.Current;
            if (x.IsZero)
            {
                set.MoveNext();
                continue;
            }
            FrE.Inverse(x, out FrE y);
            FrE.MultiplyMod(x, y, out FrE z);
            Assert.IsTrue(z.IsOne);
            set.MoveNext();
        }
    }

    [Test]
    public void TestSerialize()
    {
        using IEnumerator<FrE> set = FrE.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            FrE x = set.Current;
            Span<byte> bytes = x.ToBytes();
            FrE elem = FrE.FromBytes(bytes.ToArray());
            Assert.IsTrue(x.Equals(elem));
            set.MoveNext();
        }
    }

    [Test]
    public void TestSerializeBigEndian()
    {
        using IEnumerator<FrE> set = FrE.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            FrE x = set.Current;
            Span<byte> bytes = x.ToBytesBigEndian();
            FrE elem = FrE.FromBytes(bytes.ToArray(), true);
            Assert.IsTrue(x.Equals(elem));
            set.MoveNext();
        }
    }

    [Test]
    public void TestSqrt()
    {
        using IEnumerator<FrE> set = FrE.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            FrE x = set.Current;
            if (FrE.Legendre(x) != 1)
            {
                set.MoveNext();
                continue;
            }
            FrE.Sqrt(x, out FrE sqrtElem);
            FrE.Exp(sqrtElem, 2, out FrE res);
            Assert.IsTrue(x.Equals(res));
            set.MoveNext();
        }
    }

    [Test]
    public void TestMultiInv()
    {
        FrE[] values =
        {
            FrE.SetElement(1),
            FrE.SetElement(2),
            FrE.SetElement(3)
        };

        FrE[] gotInverse = FrE.MultiInverse(values);
        FrE?[] expectedInverse = NaiveMultiInverse(values);

        Assert.IsTrue(gotInverse.Length == expectedInverse.Length);
        for (int i = 0; i < gotInverse.Length; i++)
        {
            Assert.IsTrue(gotInverse[i].Equals(expectedInverse[i].Value));
        }
    }

    private static FrE?[] NaiveMultiInverse(IReadOnlyList<FrE> values)
    {
        FrE?[] res = new FrE?[values.Count];
        for (int i = 0; i < values.Count; i++)
        {
            FrE.Inverse(values[i], out FrE x);
            res[i] = x;
        }
        return res;
    }
}
