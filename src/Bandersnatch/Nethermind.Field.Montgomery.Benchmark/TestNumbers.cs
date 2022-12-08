using System.Numerics;

namespace Nethermind.Field.Montgomery.Benchmark
{
    public static class TestNumbers
    {
        public static readonly BigInteger _twoTo64 = new BigInteger(ulong.MaxValue) + 1;
        public static readonly BigInteger _twoTo128 = _twoTo64 * _twoTo64;
        public static readonly BigInteger _uInt128Max = _twoTo128 - 1;
        public static readonly BigInteger _twoTo192 = _twoTo128 * _twoTo64;
        public static readonly BigInteger _uInt192Max = _twoTo192 - 1;
        public static readonly BigInteger _twoTo256 = _twoTo128 * _twoTo128;
        public static readonly BigInteger _uInt256Max = _twoTo256 - 1;

        public static readonly BigInteger _int256Max = (BigInteger.One << 255) - 1;
        public static readonly BigInteger _int256Min = -_int256Max;
    }
}
