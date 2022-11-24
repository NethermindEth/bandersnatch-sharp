using System;
using System.Collections.Generic;
using Nethermind.Field.Montgomery.FpEElement;
using NUnit.Framework;

namespace Nethermind.Field.Montgomery.Test;
public class FpETests
{
    [Test]
    public void TestNegativeValues()
    {
        using IEnumerator<FpE> set = FpE.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            FpE x = set.Current;
            FpE y = 0 - x;
            FpE z = y + x;
            Assert.IsTrue(z.IsZero);
            set.MoveNext();
        }
    }

    [Test]
    public void TestAddition()
    {
        FpE X = FpE.qElement - 1;
        FpE Y = X + X;
        FpE Z = Y - X;
        Assert.IsTrue(Z.Equals(X));

        using IEnumerator<FpE> set = FpE.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            FpE x = set.Current;
            FpE y = x + x + x + x;
            FpE z = y - x - x - x - x;
            Assert.IsTrue(z.IsZero);
            set.MoveNext();
        }
    }

    [Test]
    public void TestInverse()
    {
        using IEnumerator<FpE> set = FpE.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            FpE x = set.Current;
            if (x.IsZero)
            {
                set.MoveNext();
                continue;
            }
            FpE.Inverse(x, out FpE y);
            FpE.Inverse(y, out FpE z);
            Assert.IsTrue(z.Equals(x));
            set.MoveNext();
        }
    }

    [Test]
    public void TestInverseMultiplication()
    {
        using IEnumerator<FpE> set = FpE.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            FpE x = set.Current;
            if (x.IsZero)
            {
                set.MoveNext();
                continue;
            }
            FpE.Inverse(x, out FpE y);
            FpE.MultiplyMod(x, y, out FpE z);
            Assert.IsTrue(z.IsOne);
            set.MoveNext();
        }
    }

    [Test]
    public void TestSerialize()
    {
        using IEnumerator<FpE> set = FpE.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            FpE x = set.Current;
            Span<byte> bytes = x.ToBytes();
            FpE elem = FpE.FromBytes(bytes.ToArray());
            Assert.IsTrue(x.Equals(elem));
            set.MoveNext();
        }
    }

    [Test]
    public void TestSerializeBigEndian()
    {
        using IEnumerator<FpE> set = FpE.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            FpE x = set.Current;
            Span<byte> bytes = x.ToBytesBigEndian();
            FpE elem = FpE.FromBytes(bytes.ToArray(), true);
            Assert.IsTrue(x.Equals(elem));
            set.MoveNext();
        }
    }

    [Test]
    public void TestSqrt()
    {
        using IEnumerator<FpE> set = FpE.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            FpE x = set.Current;
            if (FpE.Legendre(x) != 1)
            {
                set.MoveNext();
                continue;
            }
            FpE.Sqrt(x, out FpE sqrtElem);
            FpE.Exp(sqrtElem, 2, out FpE res);
            Assert.IsTrue(x.Equals(res));
            set.MoveNext();
        }
    }

    [Test]
    public void TestMultiInv()
    {
        FpE[] values =
        {
            FpE.SetElement(1),
            FpE.SetElement(2),
            FpE.SetElement(3)
        };

        FpE[] gotInverse = FpE.MultiInverse(values);
        FpE?[] expectedInverse = NaiveMultiInverse(values);

        Assert.IsTrue(gotInverse.Length == expectedInverse.Length);
        for (int i = 0; i < gotInverse.Length; i++)
        {
            Assert.IsTrue(gotInverse[i].Equals(expectedInverse[i].Value));
        }
    }

    private static FpE?[] NaiveMultiInverse(IReadOnlyList<FpE> values)
    {
        FpE?[] res = new FpE?[values.Count];
        for (int i = 0; i < values.Count; i++)
        {
            FpE.Inverse(values[i], out FpE x);
            res[i] = x;
        }
        return res;
    }
}
