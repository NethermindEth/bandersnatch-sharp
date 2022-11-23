using Nethermind.Field.Montgomery;
using NUnit.Framework;

namespace Nethermind.Verkle.Polynomial.Test;


public class MonomialBasisTests
{
    // [Test]
    // public void test_vanishing_poly()
    // {
    //     FrE[] xs = new[]
    //     {
    //         FrE.SetElement(0),
    //         FrE.SetElement(1),
    //         FrE.SetElement(2),
    //         FrE.SetElement(3),
    //         FrE.SetElement(4),
    //         FrE.SetElement(5)
    //     };
    //
    //     MonomialBasis z = MonomialBasis.VanishingPoly(xs);
    //
    //     foreach (FrE x in xs)
    //     {
    //         Assert.IsTrue(z.Evaluate(x).IsZero);
    //     }
    // }

    [Test]
    public void test_poly_div()
    {
        FrE[] aL = new[]
        {
            FrE.SetElement(2),
            FrE.SetElement(3),
            FrE.SetElement(1),
        };
        MonomialBasis a = new MonomialBasis(aL);
        FrE[] bL = new[]
        {
            FrE.SetElement(1),
            FrE.SetElement(1),
        };
        MonomialBasis b = new MonomialBasis(bL);

        MonomialBasis result = a / b;
        Assert.IsTrue(result.Coeffs[0].Equals(FrE.SetElement(2)));
        Assert.IsTrue(result.Coeffs[1].Equals(FrE.SetElement(1)));
    }

    [Test]
    public void test_derivative()
    {
        FrE[] aL = new[]
        {
            FrE.SetElement(9),
            FrE.SetElement(20),
            FrE.SetElement(10),
            FrE.SetElement(5),
            FrE.SetElement(6),
        };
        MonomialBasis a = new MonomialBasis(aL);
        FrE[] bL = new[]
        {
            FrE.SetElement(20),
            FrE.SetElement(20),
            FrE.SetElement(15),
            FrE.SetElement(24),
        };
        MonomialBasis b = new MonomialBasis(bL);

        MonomialBasis gotAPrime = MonomialBasis.FormalDerivative(a);
        for (int i = 0; i < gotAPrime.Length(); i++)
        {
            Assert.IsTrue(b.Coeffs[i].Equals(gotAPrime.Coeffs[i]));
        }
    }
}
