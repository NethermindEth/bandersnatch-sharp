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
        Fr[]? domain = new[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)1),
            new Fr((UInt256)2),
            new Fr((UInt256)3),
            new Fr((UInt256)4),
            new Fr((UInt256)5)
        };

        Fr[]? domainSq = new[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)1),
            new Fr((UInt256)4),
            new Fr((UInt256)9),
            new Fr((UInt256)16),
            new Fr((UInt256)25)
        };

        Fr[]? domain_2 = new[]
        {
            new Fr((UInt256)2),
            new Fr((UInt256)3),
            new Fr((UInt256)4),
            new Fr((UInt256)5),
            new Fr((UInt256)6),
            new Fr((UInt256)7)
        };

        LagrangeBasis? a = new LagrangeBasis(domainSq, domain);
        LagrangeBasis? b = new LagrangeBasis(domain_2, domain);

        Fr[]? expected = new[]
        {
            new Fr((UInt256)2),
            new Fr((UInt256)4),
            new Fr((UInt256)8),
            new Fr((UInt256)14),
            new Fr((UInt256)22),
            new Fr((UInt256)32)
        };
        LagrangeBasis? ex = new LagrangeBasis(expected, domain);
        LagrangeBasis? result = a + b;

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
        Fr[]? domain = new[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)1),
            new Fr((UInt256)2),
            new Fr((UInt256)3),
            new Fr((UInt256)4),
            new Fr((UInt256)5)
        };

        Fr[]? domainSq = new[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)1),
            new Fr((UInt256)4),
            new Fr((UInt256)9),
            new Fr((UInt256)16),
            new Fr((UInt256)25)
        };
        Fr[]? domainPow4 = new[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)1),
            new Fr((UInt256)16),
            new Fr((UInt256)81),
            new Fr((UInt256)256),
            new Fr((UInt256)625)
        };


        LagrangeBasis? a = new LagrangeBasis(domainSq, domain);
        LagrangeBasis? result = a * a;

        LagrangeBasis? ex = new LagrangeBasis(domainPow4, domain);

        for (int i = 0; i < ex.Evaluations.Length; i++)
        {
            Assert.IsTrue(ex.Evaluations[i] == result.Evaluations[i]);
        }
    }

    [Test]
    public void test_scale()
    {
        Fr[]? domain = new[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)1),
            new Fr((UInt256)2),
            new Fr((UInt256)3),
            new Fr((UInt256)4),
            new Fr((UInt256)5)
        };

        Fr[]? domainSq = new[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)1),
            new Fr((UInt256)4),
            new Fr((UInt256)9),
            new Fr((UInt256)16),
            new Fr((UInt256)25)
        };

        Fr? constant = new Fr((UInt256)10);

        LagrangeBasis? a = new LagrangeBasis(domainSq, domain);
        LagrangeBasis? result = a * constant;

        Fr[]? expected = new[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)10),
            new Fr((UInt256)40),
            new Fr((UInt256)90),
            new Fr((UInt256)160),
            new Fr((UInt256)250)
        };
        LagrangeBasis? ex = new LagrangeBasis(expected, domain);

        for (int i = 0; i < ex.Evaluations.Length; i++)
        {
            Assert.IsTrue(ex.Evaluations[i] == result.Evaluations[i]);
        }
    }

    [Test]
    public void test_interpolation()
    {
        Fr[]? domain = new[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)1),
            new Fr((UInt256)2),
            new Fr((UInt256)3),
            new Fr((UInt256)4),
            new Fr((UInt256)5)
        };

        Fr[]? domainSq = new[]
        {
            new Fr((UInt256)0),
            new Fr((UInt256)1),
            new Fr((UInt256)4),
            new Fr((UInt256)9),
            new Fr((UInt256)16),
            new Fr((UInt256)25)
        };

        LagrangeBasis? xSquaredLagrange = new LagrangeBasis(domainSq, domain);
        MonomialBasis? xSquaredCoeff = xSquaredLagrange.Interpolate();

        MonomialBasis? expectedXSquaredCoeff = new MonomialBasis(
            new[] { Fr.Zero, Fr.Zero, Fr.One });

        for (int i = 0; i < expectedXSquaredCoeff.Coeffs.Length; i++)
        {
            Assert.IsTrue(expectedXSquaredCoeff.Coeffs[i] == xSquaredCoeff.Coeffs[i]);
        }
    }
}
