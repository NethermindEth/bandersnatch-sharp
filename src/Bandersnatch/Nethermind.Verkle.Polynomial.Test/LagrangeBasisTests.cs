using Nethermind.Field;
using Nethermind.Int256;
using Nethermind.Verkle.Curve;
using NUnit.Framework;
using Nethermind.Field.Montgomery;

namespace Nethermind.Verkle.Polynomial.Test;


public class LagrangeBasisTests
{
    [Test]
    public void test_add_sub()
    {
        FrE[] domain = new[]
        {
            FrE.SetElement(0),
            FrE.SetElement(1),
            FrE.SetElement(2),
            FrE.SetElement(3),
            FrE.SetElement(4),
            FrE.SetElement(5)
        };

        FrE[] domainSq = new[]
        {
            FrE.SetElement(0),
            FrE.SetElement(1),
            FrE.SetElement(4),
            FrE.SetElement(9),
            FrE.SetElement(16),
            FrE.SetElement(25)
        };

        FrE[] domain_2 = new[]
        {
            FrE.SetElement(2),
            FrE.SetElement(3),
            FrE.SetElement(4),
            FrE.SetElement(5),
            FrE.SetElement(6),
            FrE.SetElement(7)
        };

        LagrangeBasis a = new LagrangeBasis(domainSq, domain);
        LagrangeBasis b = new LagrangeBasis(domain_2, domain);

        FrE[] expected = new[]
        {
            FrE.SetElement(2),
            FrE.SetElement(4),
            FrE.SetElement(8),
            FrE.SetElement(14),
            FrE.SetElement(22),
            FrE.SetElement(32)
        };
        LagrangeBasis ex = new LagrangeBasis(expected, domain);
        LagrangeBasis result = a + b;

        for (int i = 0; i < ex.Evaluations.Length; i++)
        {
            Assert.IsTrue(ex.Evaluations[i].Equals(result.Evaluations[i]));
        }
        ex -= b;
        for (int i = 0; i < ex.Evaluations.Length; i++)
        {
            Assert.IsTrue(ex.Evaluations[i].Equals(a.Evaluations[i]));
        }
    }

    [Test]
    public void test_mul()
    {
        FrE[] domain = new[]
        {
            FrE.SetElement(0),
            FrE.SetElement(1),
            FrE.SetElement(2),
            FrE.SetElement(3),
            FrE.SetElement(4),
            FrE.SetElement(5)
        };

        FrE[] domainSq = new[]
        {
            FrE.SetElement(0),
            FrE.SetElement(1),
            FrE.SetElement(4),
            FrE.SetElement(9),
            FrE.SetElement(16),
            FrE.SetElement(25)
        };
        FrE[] domainPow4 = new[]
        {
            FrE.SetElement(0),
            FrE.SetElement(1),
            FrE.SetElement(16),
            FrE.SetElement(81),
            FrE.SetElement(256),
            FrE.SetElement(625)
        };


        LagrangeBasis a = new LagrangeBasis(domainSq, domain);
        LagrangeBasis result = a * a;

        LagrangeBasis ex = new LagrangeBasis(domainPow4, domain);

        for (int i = 0; i < ex.Evaluations.Length; i++)
        {
            Assert.IsTrue(ex.Evaluations[i].Equals(result.Evaluations[i]));
        }
    }

    [Test]
    public void test_scale()
    {
        FrE[] domain = new[]
        {
            FrE.SetElement(0),
            FrE.SetElement(1),
            FrE.SetElement(2),
            FrE.SetElement(3),
            FrE.SetElement(4),
            FrE.SetElement(5)
        };

        FrE[] domainSq = new[]
        {
            FrE.SetElement(0),
            FrE.SetElement(1),
            FrE.SetElement(4),
            FrE.SetElement(9),
            FrE.SetElement(16),
            FrE.SetElement(25)
        };

        FrE constant = FrE.SetElement(10);

        LagrangeBasis a = new LagrangeBasis(domainSq, domain);
        LagrangeBasis result = a * constant;

        FrE[] expected = new[]
        {
            FrE.SetElement(0),
            FrE.SetElement(10),
            FrE.SetElement(40),
            FrE.SetElement(90),
            FrE.SetElement(160),
            FrE.SetElement(250)
        };
        LagrangeBasis ex = new LagrangeBasis(expected, domain);

        for (int i = 0; i < ex.Evaluations.Length; i++)
        {
            Assert.IsTrue(ex.Evaluations[i].Equals(result.Evaluations[i]));
        }
    }

    // [Test]
    // public void test_interpolation()
    // {
    //     FrE[] domain = new[]
    //     {
    //         FrE.SetElement(0),
    //         FrE.SetElement(1),
    //         FrE.SetElement(2),
    //         FrE.SetElement(3),
    //         FrE.SetElement(4),
    //         FrE.SetElement(5)
    //     };
    //
    //     FrE[] domainSq = new[]
    //     {
    //         FrE.SetElement(0),
    //         FrE.SetElement(1),
    //         FrE.SetElement(4),
    //         FrE.SetElement(9),
    //         FrE.SetElement(16),
    //         FrE.SetElement(25)
    //     };
    //
    //     LagrangeBasis xSquaredLagrange = new LagrangeBasis(domainSq, domain);
    //     MonomialBasis xSquaredCoeff = xSquaredLagrange.Interpolate();
    //
    //     MonomialBasis expectedXSquaredCoeff = new MonomialBasis(
    //         new[] { FrE.Zero, FrE.Zero, FrE.One });
    //
    //     for (int i = 0; i < expectedXSquaredCoeff.Coeffs.Length; i++)
    //     {
    //         Assert.IsTrue(expectedXSquaredCoeff.Coeffs[i].Equals(xSquaredCoeff.Coeffs[i]));
    //     }
    // }
}
