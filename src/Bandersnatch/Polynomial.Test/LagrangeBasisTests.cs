using Curve;
using Field;
using Nethermind.Int256;
using NUnit.Framework;

namespace Polynomial.Test;
using Fp = FixedFiniteField<BandersnatchBaseFieldStruct>;
using Fr = FixedFiniteField<BandersnatchScalarFieldStruct>;

public class LagrangeBasisTests
{
    [Test]
    public void test_add_sub()
    {
        var domain = new Fr[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)1),
            new Fr((UInt256)2),
            new Fr((UInt256)3),
            new Fr((UInt256)4),
            new Fr((UInt256)5)
        };
        
        var domainSq = new Fr[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)1),
            new Fr((UInt256)4),
            new Fr((UInt256)9),
            new Fr((UInt256)16),
            new Fr((UInt256)25)
        };
        
        var domain_2 = new Fr[]
        {
            new Fr((UInt256)2),
            new Fr((UInt256)3),
            new Fr((UInt256)4),
            new Fr((UInt256)5),
            new Fr((UInt256)6),
            new Fr((UInt256)7)
        };

        var a = new LagrangeBasis(domainSq, domain);
        var b = new LagrangeBasis(domain_2, domain);

        var expected = new Fr[]
        {
            new Fr((UInt256)2),
            new Fr((UInt256)4),
            new Fr((UInt256)8),
            new Fr((UInt256)14),
            new Fr((UInt256)22),
            new Fr((UInt256)32)
        };
        var ex = new LagrangeBasis(expected, domain);
        var result = a + b;

        for (int i = 0; i < ex.Evaluations.Length; i++)
        {
            Assert.IsTrue(ex.Evaluations[i] == result.Evaluations[i]);
        }
        ex -= b;
        for (int i = 0; i < ex.Evaluations.Length; i++)
        {
            Assert.IsTrue(ex.Evaluations[i] == a.Evaluations[i]);
        }
    }
    
    [Test]
    public void test_mul()
    {
        var domain = new Fr[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)1),
            new Fr((UInt256)2),
            new Fr((UInt256)3),
            new Fr((UInt256)4),
            new Fr((UInt256)5)
        };
        
        var domainSq = new Fr[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)1),
            new Fr((UInt256)4),
            new Fr((UInt256)9),
            new Fr((UInt256)16),
            new Fr((UInt256)25)
        };
        var domainPow4 = new Fr[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)1),
            new Fr((UInt256)16),
            new Fr((UInt256)81),
            new Fr((UInt256)256),
            new Fr((UInt256)625)
        };


        var a = new LagrangeBasis(domainSq, domain);
        var result = a * a;
        
        var ex = new LagrangeBasis(domainPow4, domain);
        
        for (int i = 0; i < ex.Evaluations.Length; i++)
        {
            Assert.IsTrue(ex.Evaluations[i] == result.Evaluations[i]);
        }
    }
    
    [Test]
    public void test_scale()
    {
        var domain = new Fr[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)1),
            new Fr((UInt256)2),
            new Fr((UInt256)3),
            new Fr((UInt256)4),
            new Fr((UInt256)5)
        };
        
        var domainSq = new Fr[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)1),
            new Fr((UInt256)4),
            new Fr((UInt256)9),
            new Fr((UInt256)16),
            new Fr((UInt256)25)
        };

        var constant = new Fr((UInt256) 10);

        var a = new LagrangeBasis(domainSq, domain);
        var result = a * constant;

        var expected = new Fr[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)10),
            new Fr((UInt256)40),
            new Fr((UInt256)90),
            new Fr((UInt256)160),
            new Fr((UInt256)250)
        };
        var ex = new LagrangeBasis(expected, domain);
        
        for (int i = 0; i < ex.Evaluations.Length; i++)
        {
            Assert.IsTrue(ex.Evaluations[i] == result.Evaluations[i]);
        }
    }
    
    [Test]
    public void test_interpolation()
    {
        var domain = new Fr[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)1),
            new Fr((UInt256)2),
            new Fr((UInt256)3),
            new Fr((UInt256)4),
            new Fr((UInt256)5)
        };
        
        var domainSq = new Fr[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)1),
            new Fr((UInt256)4),
            new Fr((UInt256)9),
            new Fr((UInt256)16),
            new Fr((UInt256)25)
        };
        
        var xSquaredLagrange = new LagrangeBasis(domainSq, domain);
        var xSquaredCoeff = xSquaredLagrange.Interpolate();

        var expectedXSquaredCoeff = new MonomialBasis(
            new Fr[]{Fr.Zero, Fr.Zero, Fr.One});

        for (int i = 0; i < expectedXSquaredCoeff.Coeffs.Length; i++)
        {
            Assert.IsTrue(expectedXSquaredCoeff.Coeffs[i] == xSquaredCoeff.Coeffs[i]);
        }
    }
}