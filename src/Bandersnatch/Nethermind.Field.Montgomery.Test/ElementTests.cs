using System;
using System.Collections.Generic;
using Nethermind.Field.Montgomery.ElementFactory;
using NUnit.Framework;

namespace Nethermind.Field.Montgomery.Test;

[TestFixture]
public class ElementTests
{

    [Test]
    public void TestNegativeValues()
    {
        using IEnumerator<Element> set = Element.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            Element x = set.Current;
            Element y = 0 - x;
            Element z = y + x;
            Assert.IsTrue(z.IsZero);
            set.MoveNext();
        }
    }

    [Test]
    public void TestAddition()
    {
        Element X = Element.qElement - 1;
        Element Y = X + X;
        Element Z = Y - X;
        Assert.IsTrue(Z.Equals(X));

        using IEnumerator<Element> set = Element.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            Element x = set.Current;
            Element y = x + x + x + x;
            Element z = y - x - x - x - x;
            Assert.IsTrue(z.IsZero);
            set.MoveNext();
        }
    }

    [Test]
    public void TestInverse()
    {
        using IEnumerator<Element> set = Element.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            Element x = set.Current;
            if (x.IsZero)
            {
                set.MoveNext();
                continue;
            }
            Element.Inverse(x, out Element y);
            Element.Inverse(y, out Element z);
            Assert.IsTrue(z.Equals(x));
            set.MoveNext();
        }
    }

    [Test]
    public void TestInverseMultiplication()
    {
        using IEnumerator<Element> set = Element.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            Element x = set.Current;
            if (x.IsZero)
            {
                set.MoveNext();
                continue;
            }
            Element.Inverse(x, out Element y);
            Element.MultiplyMod(x, y, out Element z);
            Assert.IsTrue(z.IsOne);
            set.MoveNext();
        }
    }

    [Test]
    public void TestSerialize()
    {
        using IEnumerator<Element> set = Element.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            Element x = set.Current;
            Span<byte> bytes = x.ToBytes();
            Element elem = Element.FromBytes(bytes.ToArray());
            Assert.IsTrue(x.Equals(elem));
            set.MoveNext();
        }
    }

    [Test]
    public void TestSerializeBigEndian()
    {
        using IEnumerator<Element> set = Element.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            Element x = set.Current;
            Span<byte> bytes = x.ToBytesBigEndian();
            Element elem = Element.FromBytes(bytes.ToArray(), true);
            Assert.IsTrue(x.Equals(elem));
            set.MoveNext();
        }
    }

    [Test]
    public void TestSqrt()
    {
        using IEnumerator<Element> set = Element.GetRandom().GetEnumerator();
        for (int i = 0; i < 1000; i++)
        {
            Element x = set.Current;
            if (Element.Legendre(x) != 1)
            {
                set.MoveNext();
                continue;
            }
            Element.Sqrt(x, out Element sqrtElem);
            Element.Exp(sqrtElem, 2, out Element res);
            Assert.IsTrue(x.Equals(res));
            set.MoveNext();
        }
    }

    [Test]
    public void TestMultiInv()
    {
        Element[] values =
        {
            Element.SetElement(1),
            Element.SetElement(2),
            Element.SetElement(3)
        };

        Element[] gotInverse = Element.MultiInverse(values);
        Element?[] expectedInverse = NaiveMultiInverse(values);

        Assert.IsTrue(gotInverse.Length == expectedInverse.Length);
        for (int i = 0; i < gotInverse.Length; i++)
        {
            Assert.IsTrue(gotInverse[i].Equals(expectedInverse[i].Value));
        }
    }

    private static Element?[] NaiveMultiInverse(IReadOnlyList<Element> values)
    {
        Element?[] res = new Element?[values.Count];
        for (int i = 0; i < values.Count; i++)
        {
            Element.Inverse(values[i], out Element x);
            res[i] = x;
        }
        return res;
    }
}
