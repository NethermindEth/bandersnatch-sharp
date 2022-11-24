using System.Numerics;
using Nethermind.Int256;

namespace Nethermind.Field.Montgomery.ElementFactory;

public readonly partial struct Element
{

    public static Element operator +(in Element a, in Element b)
    {
        AddMod(in a, in b, out Element res);
        return res;
    }

    public static Element operator -(in Element a, in Element b)
    {
        SubtractMod(in a, in b, out Element c);
        return c;
    }

    public static Element operator *(in Element a, in Element b)
    {
        MultiplyMod(a, b, out Element x);
        return x;
    }


    public static Element operator /(in Element a, in Element b)
    {
        Divide(in a, in b, out Element c);
        return c;
    }

    public static Element operator >>(in Element a, int n)
    {
        a.RightShift(n, out Element res);
        return res;
    }
    public static Element operator <<(in Element a, int n)
    {
        a.LeftShift(n, out Element res);
        return res;
    }

    public static bool operator ==(in Element a, int b) => a.Equals(b);
    public static bool operator ==(int a, in Element b) => b.Equals(a);
    public static bool operator ==(in Element a, uint b) => a.Equals(b);
    public static bool operator ==(uint a, in Element b) => b.Equals(a);
    public static bool operator ==(in Element a, long b) => a.Equals(b);
    public static bool operator ==(long a, in Element b) => b.Equals(a);
    public static bool operator ==(in Element a, ulong b) => a.Equals(b);
    public static bool operator ==(ulong a, in Element b) => b.Equals(a);
    public static bool operator !=(in Element a, int b) => !a.Equals(b);
    public static bool operator !=(int a, in Element b) => !b.Equals(a);
    public static bool operator !=(in Element a, uint b) => !a.Equals(b);
    public static bool operator !=(uint a, in Element b) => !b.Equals(a);
    public static bool operator !=(in Element a, long b) => !a.Equals(b);
    public static bool operator !=(long a, in Element b) => !b.Equals(a);
    public static bool operator !=(in Element a, ulong b) => !a.Equals(b);
    public static bool operator !=(ulong a, in Element b) => !b.Equals(a);

    public static bool operator <(in Element a, in Element b) => LessThan(in a, in b);
    public static bool operator <(in Element a, int b) => LessThan(in a, b);
    public static bool operator <(int a, in Element b) => LessThan(a, in b);
    public static bool operator <(in Element a, uint b) => LessThan(in a, b);
    public static bool operator <(uint a, in Element b) => LessThan(a, in b);
    public static bool operator <(in Element a, long b) => LessThan(in a, b);
    public static bool operator <(long a, in Element b) => LessThan(a, in b);
    public static bool operator <(in Element a, ulong b) => LessThan(in a, b);
    public static bool operator <(ulong a, in Element b) => LessThan(a, in b);
    public static bool operator <=(in Element a, in Element b) => !LessThan(in b, in a);
    public static bool operator <=(in Element a, int b) => !LessThan(b, in a);
    public static bool operator <=(int a, in Element b) => !LessThan(in b, a);
    public static bool operator <=(in Element a, uint b) => !LessThan(b, in a);
    public static bool operator <=(uint a, in Element b) => !LessThan(in b, a);
    public static bool operator <=(in Element a, long b) => !LessThan(b, in a);
    public static bool operator <=(long a, in Element b) => !LessThan(in b, a);
    public static bool operator <=(in Element a, ulong b) => !LessThan(b, in a);
    public static bool operator <=(ulong a, Element b) => !LessThan(in b, a);
    public static bool operator >(in Element a, in Element b) => LessThan(in b, in a);
    public static bool operator >(in Element a, int b) => LessThan(b, in a);
    public static bool operator >(int a, in Element b) => LessThan(in b, a);
    public static bool operator >(in Element a, uint b) => LessThan(b, in a);
    public static bool operator >(uint a, in Element b) => LessThan(in b, a);
    public static bool operator >(in Element a, long b) => LessThan(b, in a);
    public static bool operator >(long a, in Element b) => LessThan(in b, a);
    public static bool operator >(in Element a, ulong b) => LessThan(b, in a);
    public static bool operator >(ulong a, in Element b) => LessThan(in b, a);
    public static bool operator >=(in Element a, in Element b) => !LessThan(in a, in b);
    public static bool operator >=(in Element a, int b) => !LessThan(in a, b);
    public static bool operator >=(int a, in Element b) => !LessThan(a, in b);
    public static bool operator >=(in Element a, uint b) => !LessThan(in a, b);
    public static bool operator >=(uint a, in Element b) => !LessThan(a, in b);
    public static bool operator >=(in Element a, long b) => !LessThan(in a, b);
    public static bool operator >=(long a, in Element b) => !LessThan(a, in b);
    public static bool operator >=(in Element a, ulong b) => !LessThan(in a, b);
    public static bool operator >=(ulong a, in Element b) => !LessThan(a, in b);

    public static implicit operator Element(ulong value) => new Element(value, 0ul, 0ul, 0ul);
    public static implicit operator Element(ulong[] value) => new Element(value[0], value[1], value[2], value[3]);

    public static explicit operator Element(in BigInteger value)
    {
        byte[] bytes32 = value.ToBytes32(true);
        return new Element(bytes32, true);
    }

}
