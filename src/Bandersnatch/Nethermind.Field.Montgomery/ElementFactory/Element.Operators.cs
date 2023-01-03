using System.Numerics;
using Nethermind.Int256;

namespace Nethermind.Field.Montgomery.ElementFactory
{
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

        public static Element operator >> (in Element a, int n)
        {
            a.RightShift(n, out Element res);
            return res;
        }
        public static Element operator <<(in Element a, int n)
        {
            a.LeftShift(n, out Element res);
            return res;
        }

        public static bool operator ==(in Element a, int b)
        {
            return a.Equals(b);
        }
        public static bool operator ==(int a, in Element b)
        {
            return b.Equals(a);
        }
        public static bool operator ==(in Element a, uint b)
        {
            return a.Equals(b);
        }
        public static bool operator ==(uint a, in Element b)
        {
            return b.Equals(a);
        }
        public static bool operator ==(in Element a, long b)
        {
            return a.Equals(b);
        }
        public static bool operator ==(long a, in Element b)
        {
            return b.Equals(a);
        }
        public static bool operator ==(in Element a, ulong b)
        {
            return a.Equals(b);
        }
        public static bool operator ==(ulong a, in Element b)
        {
            return b.Equals(a);
        }
        public static bool operator !=(in Element a, int b)
        {
            return !a.Equals(b);
        }
        public static bool operator !=(int a, in Element b)
        {
            return !b.Equals(a);
        }
        public static bool operator !=(in Element a, uint b)
        {
            return !a.Equals(b);
        }
        public static bool operator !=(uint a, in Element b)
        {
            return !b.Equals(a);
        }
        public static bool operator !=(in Element a, long b)
        {
            return !a.Equals(b);
        }
        public static bool operator !=(long a, in Element b)
        {
            return !b.Equals(a);
        }
        public static bool operator !=(in Element a, ulong b)
        {
            return !a.Equals(b);
        }
        public static bool operator !=(ulong a, in Element b)
        {
            return !b.Equals(a);
        }

        public static bool operator <(in Element a, in Element b)
        {
            return LessThan(in a, in b);
        }
        public static bool operator <(in Element a, int b)
        {
            return LessThan(in a, b);
        }
        public static bool operator <(int a, in Element b)
        {
            return LessThan(a, in b);
        }
        public static bool operator <(in Element a, uint b)
        {
            return LessThan(in a, b);
        }
        public static bool operator <(uint a, in Element b)
        {
            return LessThan(a, in b);
        }
        public static bool operator <(in Element a, long b)
        {
            return LessThan(in a, b);
        }
        public static bool operator <(long a, in Element b)
        {
            return LessThan(a, in b);
        }
        public static bool operator <(in Element a, ulong b)
        {
            return LessThan(in a, b);
        }
        public static bool operator <(ulong a, in Element b)
        {
            return LessThan(a, in b);
        }
        public static bool operator <=(in Element a, in Element b)
        {
            return !LessThan(in b, in a);
        }
        public static bool operator <=(in Element a, int b)
        {
            return !LessThan(b, in a);
        }
        public static bool operator <=(int a, in Element b)
        {
            return !LessThan(in b, a);
        }
        public static bool operator <=(in Element a, uint b)
        {
            return !LessThan(b, in a);
        }
        public static bool operator <=(uint a, in Element b)
        {
            return !LessThan(in b, a);
        }
        public static bool operator <=(in Element a, long b)
        {
            return !LessThan(b, in a);
        }
        public static bool operator <=(long a, in Element b)
        {
            return !LessThan(in b, a);
        }
        public static bool operator <=(in Element a, ulong b)
        {
            return !LessThan(b, in a);
        }
        public static bool operator <=(ulong a, Element b)
        {
            return !LessThan(in b, a);
        }
        public static bool operator >(in Element a, in Element b)
        {
            return LessThan(in b, in a);
        }
        public static bool operator >(in Element a, int b)
        {
            return LessThan(b, in a);
        }
        public static bool operator >(int a, in Element b)
        {
            return LessThan(in b, a);
        }
        public static bool operator >(in Element a, uint b)
        {
            return LessThan(b, in a);
        }
        public static bool operator >(uint a, in Element b)
        {
            return LessThan(in b, a);
        }
        public static bool operator >(in Element a, long b)
        {
            return LessThan(b, in a);
        }
        public static bool operator >(long a, in Element b)
        {
            return LessThan(in b, a);
        }
        public static bool operator >(in Element a, ulong b)
        {
            return LessThan(b, in a);
        }
        public static bool operator >(ulong a, in Element b)
        {
            return LessThan(in b, a);
        }
        public static bool operator >=(in Element a, in Element b)
        {
            return !LessThan(in a, in b);
        }
        public static bool operator >=(in Element a, int b)
        {
            return !LessThan(in a, b);
        }
        public static bool operator >=(int a, in Element b)
        {
            return !LessThan(a, in b);
        }
        public static bool operator >=(in Element a, uint b)
        {
            return !LessThan(in a, b);
        }
        public static bool operator >=(uint a, in Element b)
        {
            return !LessThan(a, in b);
        }
        public static bool operator >=(in Element a, long b)
        {
            return !LessThan(in a, b);
        }
        public static bool operator >=(long a, in Element b)
        {
            return !LessThan(a, in b);
        }
        public static bool operator >=(in Element a, ulong b)
        {
            return !LessThan(in a, b);
        }
        public static bool operator >=(ulong a, in Element b)
        {
            return !LessThan(a, in b);
        }

        public static implicit operator Element(ulong value)
        {
            return new Element(value);
        }
        public static implicit operator Element(ulong[] value)
        {
            return new Element(value[0], value[1], value[2], value[3]);
        }

        public static explicit operator Element(in BigInteger value)
        {
            byte[] bytes32 = value.ToBytes32(true);
            return new Element(bytes32, true);
        }

    }
}
