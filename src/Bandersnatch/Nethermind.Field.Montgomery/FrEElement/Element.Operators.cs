using System.Numerics;
using Nethermind.Int256;

namespace Nethermind.Field.Montgomery.FrEElement
{
    public readonly partial struct FrE
    {

        public static FrE operator +(in FrE a, in FrE b)
        {
            AddMod(in a, in b, out FrE res);
            return res;
        }

        public static FrE operator -(in FrE a, in FrE b)
        {
            SubtractMod(in a, in b, out FrE c);
            return c;
        }

        public static FrE operator *(in FrE a, in FrE b)
        {
            MultiplyMod(a, b, out FrE x);
            return x;
        }


        public static FrE operator /(in FrE a, in FrE b)
        {
            Divide(in a, in b, out FrE c);
            return c;
        }

        public static FrE operator >> (in FrE a, int n)
        {
            a.RightShift(n, out FrE res);
            return res;
        }
        public static FrE operator <<(in FrE a, int n)
        {
            a.LeftShift(n, out FrE res);
            return res;
        }

        public static bool operator ==(in FrE a, int b)
        {
            return a.Equals(b);
        }
        public static bool operator ==(int a, in FrE b)
        {
            return b.Equals(a);
        }
        public static bool operator ==(in FrE a, uint b)
        {
            return a.Equals(b);
        }
        public static bool operator ==(uint a, in FrE b)
        {
            return b.Equals(a);
        }
        public static bool operator ==(in FrE a, long b)
        {
            return a.Equals(b);
        }
        public static bool operator ==(long a, in FrE b)
        {
            return b.Equals(a);
        }
        public static bool operator ==(in FrE a, ulong b)
        {
            return a.Equals(b);
        }
        public static bool operator ==(ulong a, in FrE b)
        {
            return b.Equals(a);
        }
        public static bool operator !=(in FrE a, int b)
        {
            return !a.Equals(b);
        }
        public static bool operator !=(int a, in FrE b)
        {
            return !b.Equals(a);
        }
        public static bool operator !=(in FrE a, uint b)
        {
            return !a.Equals(b);
        }
        public static bool operator !=(uint a, in FrE b)
        {
            return !b.Equals(a);
        }
        public static bool operator !=(in FrE a, long b)
        {
            return !a.Equals(b);
        }
        public static bool operator !=(long a, in FrE b)
        {
            return !b.Equals(a);
        }
        public static bool operator !=(in FrE a, ulong b)
        {
            return !a.Equals(b);
        }
        public static bool operator !=(ulong a, in FrE b)
        {
            return !b.Equals(a);
        }

        public static bool operator <(in FrE a, in FrE b)
        {
            return LessThan(in a, in b);
        }
        public static bool operator <(in FrE a, int b)
        {
            return LessThan(in a, b);
        }
        public static bool operator <(int a, in FrE b)
        {
            return LessThan(a, in b);
        }
        public static bool operator <(in FrE a, uint b)
        {
            return LessThan(in a, b);
        }
        public static bool operator <(uint a, in FrE b)
        {
            return LessThan(a, in b);
        }
        public static bool operator <(in FrE a, long b)
        {
            return LessThan(in a, b);
        }
        public static bool operator <(long a, in FrE b)
        {
            return LessThan(a, in b);
        }
        public static bool operator <(in FrE a, ulong b)
        {
            return LessThan(in a, b);
        }
        public static bool operator <(ulong a, in FrE b)
        {
            return LessThan(a, in b);
        }
        public static bool operator <=(in FrE a, in FrE b)
        {
            return !LessThan(in b, in a);
        }
        public static bool operator <=(in FrE a, int b)
        {
            return !LessThan(b, in a);
        }
        public static bool operator <=(int a, in FrE b)
        {
            return !LessThan(in b, a);
        }
        public static bool operator <=(in FrE a, uint b)
        {
            return !LessThan(b, in a);
        }
        public static bool operator <=(uint a, in FrE b)
        {
            return !LessThan(in b, a);
        }
        public static bool operator <=(in FrE a, long b)
        {
            return !LessThan(b, in a);
        }
        public static bool operator <=(long a, in FrE b)
        {
            return !LessThan(in b, a);
        }
        public static bool operator <=(in FrE a, ulong b)
        {
            return !LessThan(b, in a);
        }
        public static bool operator <=(ulong a, FrE b)
        {
            return !LessThan(in b, a);
        }
        public static bool operator >(in FrE a, in FrE b)
        {
            return LessThan(in b, in a);
        }
        public static bool operator >(in FrE a, int b)
        {
            return LessThan(b, in a);
        }
        public static bool operator >(int a, in FrE b)
        {
            return LessThan(in b, a);
        }
        public static bool operator >(in FrE a, uint b)
        {
            return LessThan(b, in a);
        }
        public static bool operator >(uint a, in FrE b)
        {
            return LessThan(in b, a);
        }
        public static bool operator >(in FrE a, long b)
        {
            return LessThan(b, in a);
        }
        public static bool operator >(long a, in FrE b)
        {
            return LessThan(in b, a);
        }
        public static bool operator >(in FrE a, ulong b)
        {
            return LessThan(b, in a);
        }
        public static bool operator >(ulong a, in FrE b)
        {
            return LessThan(in b, a);
        }
        public static bool operator >=(in FrE a, in FrE b)
        {
            return !LessThan(in a, in b);
        }
        public static bool operator >=(in FrE a, int b)
        {
            return !LessThan(in a, b);
        }
        public static bool operator >=(int a, in FrE b)
        {
            return !LessThan(a, in b);
        }
        public static bool operator >=(in FrE a, uint b)
        {
            return !LessThan(in a, b);
        }
        public static bool operator >=(uint a, in FrE b)
        {
            return !LessThan(a, in b);
        }
        public static bool operator >=(in FrE a, long b)
        {
            return !LessThan(in a, b);
        }
        public static bool operator >=(long a, in FrE b)
        {
            return !LessThan(a, in b);
        }
        public static bool operator >=(in FrE a, ulong b)
        {
            return !LessThan(in a, b);
        }
        public static bool operator >=(ulong a, in FrE b)
        {
            return !LessThan(a, in b);
        }

        public static implicit operator FrE(ulong value)
        {
            return new FrE(value);
        }
        public static implicit operator FrE(ulong[] value)
        {
            return new FrE(value[0], value[1], value[2], value[3]);
        }

        public static explicit operator FrE(in BigInteger value)
        {
            byte[] bytes32 = value.ToBytes32(true);
            return new FrE(bytes32, true);
        }

    }
}
