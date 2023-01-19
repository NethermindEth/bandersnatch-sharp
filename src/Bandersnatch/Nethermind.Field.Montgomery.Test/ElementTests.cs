using System;
using System.Collections.Generic;
using NUnit.Framework;
using FE = Nethermind.Field.Montgomery.ElementFactory.Element;

namespace Nethermind.Field.Montgomery.Test
{
    [TestFixture]
    public class ElementTests
    {

        [Test]
        public void TestNegativeValues()
        {
            using IEnumerator<FE> set = FE.GetRandom().GetEnumerator();
            for (int i = 0; i < 1000; i++)
            {
                FE x = set.Current;
                FE y = 0 - x;
                FE z = y + x;
                Assert.IsTrue(z.IsZero);
                set.MoveNext();
            }
        }

        [Test]
        public void TestAddition()
        {
            FE X = FE.qElement - 1;
            FE Y = X + X;
            FE Z = Y - X;
            Assert.IsTrue(Z.Equals(X));

            using IEnumerator<FE> set = FE.GetRandom().GetEnumerator();
            for (int i = 0; i < 1000; i++)
            {
                FE x = set.Current;
                FE y = x + x + x + x;
                FE z = y - x - x - x - x;
                Assert.IsTrue(z.IsZero);
                set.MoveNext();
            }
        }

        [Test]
        public void ProfileHotPath()
        {
            using IEnumerator<FE> set = FE.GetRandom().GetEnumerator();
            for (int i = 0; i < 100000; i++)
            {
                FE x = set.Current;
                if (x.IsZero)
                {
                    set.MoveNext();
                    continue;
                }
                FE.Inverse(x, out FE y);
                FE.MultiplyMod(x, y, out FE z);
                Assert.IsTrue(z.IsOne);
                set.MoveNext();
            }
        }

        [Test]
        public void TestInverse()
        {
            using IEnumerator<FE> set = FE.GetRandom().GetEnumerator();
            for (int i = 0; i < 1000; i++)
            {
                FE x = set.Current;
                if (x.IsZero)
                {
                    set.MoveNext();
                    continue;
                }
                FE.Inverse(x, out FE y);
                FE.Inverse(y, out FE z);
                Assert.IsTrue(z.Equals(x));
                set.MoveNext();
            }
        }

        [Test]
        public void TestInverseMultiplication()
        {
            using IEnumerator<FE> set = FE.GetRandom().GetEnumerator();
            for (int i = 0; i < 1000; i++)
            {
                FE x = set.Current;
                if (x.IsZero)
                {
                    set.MoveNext();
                    continue;
                }
                FE.Inverse(x, out FE y);
                FE.MultiplyMod(x, y, out FE z);
                Assert.IsTrue(z.IsOne);
                set.MoveNext();
            }
        }

        [Test]
        public void ProfileMultiplication()
        {
            using IEnumerator<FE> set = FE.GetRandom().GetEnumerator();
            for (int i = 0; i < 100000; i++)
            {
                FE x = set.Current;
                if (x.IsZero)
                {
                    set.MoveNext();
                    continue;
                }
                FE.MultiplyMod(x, x, out FE z);
                set.MoveNext();
            }
        }

        [Test]
        public void TestSerialize()
        {
            using IEnumerator<FE> set = FE.GetRandom().GetEnumerator();
            for (int i = 0; i < 1000; i++)
            {
                FE x = set.Current;
                Span<byte> bytes = x.ToBytes();
                FE elem = FE.FromBytes(bytes.ToArray());
                Assert.IsTrue(x.Equals(elem));
                set.MoveNext();
            }
        }

        [Test]
        public void TestSerializeBigEndian()
        {
            using IEnumerator<FE> set = FE.GetRandom().GetEnumerator();
            for (int i = 0; i < 1000; i++)
            {
                FE x = set.Current;
                Span<byte> bytes = x.ToBytesBigEndian();
                FE elem = FE.FromBytes(bytes.ToArray(), true);
                Assert.IsTrue(x.Equals(elem));
                set.MoveNext();
            }
        }

        [Test]
        public void TestSqrt()
        {
            using IEnumerator<FE> set = FE.GetRandom().GetEnumerator();
            for (int i = 0; i < 1000; i++)
            {
                FE x = set.Current;
                if (FE.Legendre(x) != 1)
                {
                    set.MoveNext();
                    continue;
                }
                FE.Sqrt(x, out FE sqrtElem);
                FE.Exp(sqrtElem, 2, out FE res);
                Assert.IsTrue(x.Equals(res));
                set.MoveNext();
            }
        }

        [Test]
        public void TestMultiInv()
        {
            FE[] values = new[]
            {
                FE.SetElement(1), FE.SetElement(2), FE.SetElement(3)
            };

            FE[] gotInverse = FE.MultiInverse(values);
            FE?[] expectedInverse = NaiveMultiInverse(values);

            Assert.IsTrue(gotInverse.Length == expectedInverse.Length);
            for (int i = 0; i < gotInverse.Length; i++)
            {
                Assert.IsTrue(gotInverse[i].Equals(expectedInverse[i].Value));
            }
        }

        static private FE?[] NaiveMultiInverse(IReadOnlyList<FE> values)
        {
            FE?[] res = new FE?[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                FE.Inverse(values[i], out FE x);
                res[i] = x;
            }
            return res;
        }
    }
}
