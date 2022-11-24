using System.Numerics;
using Nethermind.Int256;

namespace Nethermind.Field.Montgomery.FpEElement;

public readonly partial struct FpE
{

    public static FpE operator +(in FpE a, in FpE b)
    {
        AddMod(in a, in b, out FpE res);
        return res;
    }

    public static FpE operator -(in FpE a, in FpE b)
    {
        SubtractMod(in a, in b, out FpE c);
        return c;
    }

    public static FpE operator *(in FpE a, in FpE b)
    {
        MultiplyMod(a, b, out FpE x);
        return x;
    }


    public static FpE operator /(in FpE a, in FpE b)
    {
        Divide(in a, in b, out FpE c);
        return c;
    }

    public static FpE operator >>(in FpE a, int n)
    {
        a.RightShift(n, out FpE res);
        return res;
    }
    public static FpE operator <<(in FpE a, int n)
    {
        a.LeftShift(n, out FpE res);
        return res;
    }

    public static bool operator ==(in FpE a, int b) => a.Equals(b);
    public static bool operator ==(int a, in FpE b) => b.Equals(a);
    public static bool operator ==(in FpE a, uint b) => a.Equals(b);
    public static bool operator ==(uint a, in FpE b) => b.Equals(a);
    public static bool operator ==(in FpE a, long b) => a.Equals(b);
    public static bool operator ==(long a, in FpE b) => b.Equals(a);
    public static bool operator ==(in FpE a, ulong b) => a.Equals(b);
    public static bool operator ==(ulong a, in FpE b) => b.Equals(a);
    public static bool operator !=(in FpE a, int b) => !a.Equals(b);
    public static bool operator !=(int a, in FpE b) => !b.Equals(a);
    public static bool operator !=(in FpE a, uint b) => !a.Equals(b);
    public static bool operator !=(uint a, in FpE b) => !b.Equals(a);
    public static bool operator !=(in FpE a, long b) => !a.Equals(b);
    public static bool operator !=(long a, in FpE b) => !b.Equals(a);
    public static bool operator !=(in FpE a, ulong b) => !a.Equals(b);
    public static bool operator !=(ulong a, in FpE b) => !b.Equals(a);

    public static bool operator <(in FpE a, in FpE b) => LessThan(in a, in b);
    public static bool operator <(in FpE a, int b) => LessThan(in a, b);
    public static bool operator <(int a, in FpE b) => LessThan(a, in b);
    public static bool operator <(in FpE a, uint b) => LessThan(in a, b);
    public static bool operator <(uint a, in FpE b) => LessThan(a, in b);
    public static bool operator <(in FpE a, long b) => LessThan(in a, b);
    public static bool operator <(long a, in FpE b) => LessThan(a, in b);
    public static bool operator <(in FpE a, ulong b) => LessThan(in a, b);
    public static bool operator <(ulong a, in FpE b) => LessThan(a, in b);
    public static bool operator <=(in FpE a, in FpE b) => !LessThan(in b, in a);
    public static bool operator <=(in FpE a, int b) => !LessThan(b, in a);
    public static bool operator <=(int a, in FpE b) => !LessThan(in b, a);
    public static bool operator <=(in FpE a, uint b) => !LessThan(b, in a);
    public static bool operator <=(uint a, in FpE b) => !LessThan(in b, a);
    public static bool operator <=(in FpE a, long b) => !LessThan(b, in a);
    public static bool operator <=(long a, in FpE b) => !LessThan(in b, a);
    public static bool operator <=(in FpE a, ulong b) => !LessThan(b, in a);
    public static bool operator <=(ulong a, FpE b) => !LessThan(in b, a);
    public static bool operator >(in FpE a, in FpE b) => LessThan(in b, in a);
    public static bool operator >(in FpE a, int b) => LessThan(b, in a);
    public static bool operator >(int a, in FpE b) => LessThan(in b, a);
    public static bool operator >(in FpE a, uint b) => LessThan(b, in a);
    public static bool operator >(uint a, in FpE b) => LessThan(in b, a);
    public static bool operator >(in FpE a, long b) => LessThan(b, in a);
    public static bool operator >(long a, in FpE b) => LessThan(in b, a);
    public static bool operator >(in FpE a, ulong b) => LessThan(b, in a);
    public static bool operator >(ulong a, in FpE b) => LessThan(in b, a);
    public static bool operator >=(in FpE a, in FpE b) => !LessThan(in a, in b);
    public static bool operator >=(in FpE a, int b) => !LessThan(in a, b);
    public static bool operator >=(int a, in FpE b) => !LessThan(a, in b);
    public static bool operator >=(in FpE a, uint b) => !LessThan(in a, b);
    public static bool operator >=(uint a, in FpE b) => !LessThan(a, in b);
    public static bool operator >=(in FpE a, long b) => !LessThan(in a, b);
    public static bool operator >=(long a, in FpE b) => !LessThan(a, in b);
    public static bool operator >=(in FpE a, ulong b) => !LessThan(in a, b);
    public static bool operator >=(ulong a, in FpE b) => !LessThan(a, in b);

    public static implicit operator FpE(ulong value) => new FpE(value, 0ul, 0ul, 0ul);
    public static implicit operator FpE(ulong[] value) => new FpE(value[0], value[1], value[2], value[3]);

    public static explicit operator FpE(in BigInteger value)
    {
        byte[] bytes32 = value.ToBytes32(true);
        return new FpE(bytes32, true);
    }

}
