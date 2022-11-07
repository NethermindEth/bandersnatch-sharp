using Curve;
using Field;
using Nethermind.Int256;
using NUnit.Framework;

namespace Polynomial.Test;
using Fp = FixedFiniteField<BandersnatchBaseFieldStruct>;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public class MonomialBasisTests
{
    [Test]
    public void test_vanishing_poly()
    {
        var xs = new Fr[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)1),
            new Fr((UInt256)2),
            new Fr((UInt256)3),
            new Fr((UInt256)4),
            new Fr((UInt256)5)
        };

        var z = MonomialBasis.VanishingPoly(xs);

        foreach (var x in xs)
        {
            Assert.IsTrue(z.Evaluate(x).IsZero);
        }
    }
    
    [Test]
    public void test_poly_div()
    {
        var aL = new Fr[]
        {
            new Fr((UInt256)2),
            new Fr((UInt256)3),
            new Fr((UInt256)1),
        };
        var a = new MonomialBasis(aL);
        var bL = new Fr[]
        {
            new Fr((UInt256)1),
            new Fr((UInt256)1),
        };
        var b = new MonomialBasis(bL);

        var result = a / b;
        Assert.IsTrue(result.Coeffs[0] == new Fr((UInt256)2));
        Assert.IsTrue(result.Coeffs[1] == new Fr((UInt256)1));
    }
    
    [Test]
    public void test_derivative()
    {
        var aL = new Fr[]
        {
            new Fr((UInt256)9),
            new Fr((UInt256)20),
            new Fr((UInt256)10),
            new Fr((UInt256)5),
            new Fr((UInt256)6),
        };
        var a = new MonomialBasis(aL);
        var bL = new Fr[]
        {
            new Fr((UInt256)20),
            new Fr((UInt256)20),
            new Fr((UInt256)15),
            new Fr((UInt256)24),
        };
        var b = new MonomialBasis(bL);

        var gotAPrime = MonomialBasis.FormalDerivative(a);
        for (int i = 0; i < gotAPrime.Length(); i++)
        {
            Assert.IsTrue(b.Coeffs[i] == gotAPrime.Coeffs[i]);
        }
    }
}