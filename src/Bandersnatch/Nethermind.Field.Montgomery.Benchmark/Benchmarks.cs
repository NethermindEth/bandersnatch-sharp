// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0. For full terms, see LICENSE in the project root.

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MathGmp.Native;
using Nethermind.Field.Montgomery.ElementFactory;
using Nethermind.Field.Montgomery.FpEElement;
using Nethermind.Int256;

namespace Nethermind.Field.Montgomery.Benchmark
{
    using TestElement = FpE;

    public class BenchmarkBase
    {

        protected static UInt256 _uMod = FpE._modulus.Value;
        protected static BigInteger _bMod = (BigInteger)_uMod;
        protected static mpz_t _gMod = ImportDataToGmp(_uMod.ToBigEndian());
        private static IEnumerable<BigInteger> Values => new[]
        {
            Numbers._uInt256Max
        }.Concat(UnaryOps.RandomUnsigned(1));
        public IEnumerable<UInt256> ValuesUint256 => Values.Select(x => (UInt256)x);
        public IEnumerable<TestElement> ValuesElement => Values.Select(x => (TestElement)x);
        public IEnumerable<mpz_t> ValuesGmp => Values.Select(x => ImportDataToGmp(x.ToByteArray(isBigEndian:true)));
        public IEnumerable<(BigInteger, UInt256, TestElement, mpz_t)> ValuesTuple => Values.Select(x => (x, (UInt256)x, (TestElement)x, ImportDataToGmp(x.ToByteArray(isBigEndian:true))));

        public IEnumerable<int> ValuesInt => UnaryOps.RandomInt(3);

        private static mpz_t ImportDataToGmp(byte[] data)
        {
            mpz_t result = new();
            gmp_lib.mpz_init(result);
            ulong memorySize = ulong.Parse(data.Length.ToString());
            using void_ptr memoryChunk = gmp_lib.allocate(memorySize);

            Marshal.Copy(data, 0, memoryChunk.ToIntPtr(), data.Length);
            gmp_lib.mpz_import(result, ulong.Parse(data.Length.ToString()), 1, 1, 1, 0, memoryChunk);

            return result;
        }
    }

    public class IntTwoParamBenchmarkBase : BenchmarkBase
    {
        [ParamsSource(nameof(ValuesTuple))]
        protected (BigInteger, UInt256, TestElement, mpz_t) _a;

        [ParamsSource(nameof(ValuesInt))]
        protected int _d;
    }

    public class TwoParamBenchmarkBase : BenchmarkBase
    {
        [ParamsSource(nameof(ValuesTuple))]
        public (BigInteger, UInt256, TestElement, mpz_t) _a;

        [ParamsSource(nameof(ValuesTuple))]
        public (BigInteger, UInt256, TestElement, mpz_t) _b;
    }

    public class ThreeParamBenchmarkBase : TwoParamBenchmarkBase
    {
        [ParamsSource(nameof(ValuesTuple))]
        public (BigInteger, UInt256, TestElement, mpz_t) _c;
    }

    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class AddMod : TwoParamBenchmarkBase
    {
        [Benchmark(Baseline = true)]
        public BigInteger AddMod_BigInteger()
        {
            return (_a.Item1 + _b.Item1) % _bMod;
        }

        // [Benchmark]
        // public BigInteger AddMod_BigInteger_New()
        // {
        //     return BigInteger.Remainder(_a.Item1 + _b.Item1, _bMod);
        // }

        [Benchmark]
        public UInt256 AddMod_UInt256()
        {
            UInt256.AddMod(_a.Item2, _b.Item2, _uMod, out UInt256 res);
            return res;
        }

        [Benchmark]
        public TestElement AddMod_Element()
        {
            TestElement.AddMod(_a.Item3, _b.Item3, out TestElement res);
            return res;
        }

        [Benchmark]
        public mpz_t AddMod_gmp()
        {
            using mpz_t res = new();
            gmp_lib.mpz_init(res);
            gmp_lib.mpz_add(res, _a.Item4, _b.Item4);
            gmp_lib.mpz_mod(res, res, _gMod);
            return res;
        }
    }

    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class SubtractMod : TwoParamBenchmarkBase
    {
        [Benchmark(Baseline = true)]
        public BigInteger SubtractMod_BigInteger()
        {
            return (_a.Item1 - _b.Item1) % _bMod;
        }

        [Benchmark]
        public UInt256 SubtractMod_UInt256()
        {
            UInt256.SubtractMod(_a.Item2, _b.Item2, _uMod, out UInt256 res);
            return res;
        }

        [Benchmark]
        public TestElement SubtractMod_Element()
        {
            TestElement.SubtractMod(_a.Item3, _b.Item3, out TestElement res);
            return res;
        }
        [Benchmark]
        public mpz_t SubtractMod_gmp()
        {
            using mpz_t res = new();
            gmp_lib.mpz_init(res);
            gmp_lib.mpz_sub(res, _a.Item4, _b.Item4);
            gmp_lib.mpz_mod(res, res, _gMod);
            return res;
        }
    }


    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class MultiplyMod : TwoParamBenchmarkBase
    {
        [Benchmark(Baseline = true)]
        public BigInteger MultiplyMod_BigInteger()
        {
            return _a.Item1 * _b.Item1 % _bMod;
        }

        [Benchmark]
        public UInt256 MultiplyMod_UInt256()
        {
            UInt256.MultiplyMod(_a.Item2, _b.Item2, _uMod, out UInt256 res);
            return res;
        }

        [Benchmark]
        public TestElement MultiplyMod_Element()
        {
            TestElement.MultiplyMod(_a.Item3, _b.Item3, out TestElement res);
            return res;
        }
        [Benchmark]
        public mpz_t MultiplyMod_gmp()
        {
            using mpz_t res = new();
            gmp_lib.mpz_init(res);
            gmp_lib.mpz_mul(res, _a.Item4, _b.Item4);
            gmp_lib.mpz_mod(res, res, _gMod);
            return res;
        }
    }

    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class DivideMod : TwoParamBenchmarkBase
    {
        [Benchmark(Baseline = true)]
        public BigInteger Divide_BigInteger()
        {
            return _a.Item1 / _b.Item1;
        }

        [Benchmark]
        public UInt256 Divide_UInt256()
        {
            UInt256.Divide(_a.Item2, _b.Item2, out UInt256 res);
            return res;
        }

        [Benchmark]
        public TestElement Divide_Element()
        {
            TestElement.Divide(_a.Item3, _b.Item3, out TestElement res);
            return res;
        }
        [Benchmark]
        public mpz_t Divide_gmp()
        {
            using mpz_t res = new();
            gmp_lib.mpz_init(res);
            gmp_lib.mpz_invert(res, _b.Item4, _gMod);
            gmp_lib.mpz_mul(res, _a.Item4, res);
            gmp_lib.mpz_mod(res, res, _gMod);
            return res;
        }
    }

    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class ExpMod : TwoParamBenchmarkBase
    {
        [Benchmark(Baseline = true)]
        public BigInteger ExpMod_BigInteger()
        {
            return BigInteger.ModPow(_a.Item1, _b.Item1, _bMod);
        }

        [Benchmark]
        public UInt256 ExpMod_UInt256()
        {
            UInt256.ExpMod(_a.Item2, _b.Item2, _uMod, out UInt256 res);
            return res;
        }

        [Benchmark]
        public TestElement ExpMod_Element()
        {
            TestElement.Exp(_a.Item3, _b.Item2, out TestElement res);
            return res;
        }
        [Benchmark]
        public mpz_t Exp_gmp()
        {
            using mpz_t res = new();
            gmp_lib.mpz_init(res);
            gmp_lib.mpz_powm(res, _a.Item4, _b.Item4, _gMod);
            return res;
        }
    }

    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class LeftShift : IntTwoParamBenchmarkBase
    {
        [Benchmark(Baseline = true)]
        public BigInteger LeftShift_BigInteger()
        {
            return (_a.Item1 << _d) % Numbers._twoTo256;
        }

        [Benchmark]
        public UInt256 LeftShift_UInt256()
        {
            _a.Item2.LeftShift(_d, out UInt256 res);
            return res;
        }

        [Benchmark]
        public TestElement LeftShift_Element()
        {
            _a.Item3.LeftShift(_d, out TestElement res);
            return res;
        }
    }

    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class RightShift : IntTwoParamBenchmarkBase
    {
        [Benchmark(Baseline = true)]
        public BigInteger RightShift_BigInteger()
        {
            return (_a.Item1 >> _d) % Numbers._twoTo256;
        }

        [Benchmark]
        public UInt256 RightShift_UInt256()
        {
            _a.Item2.RightShift(_d, out UInt256 res);
            return res;
        }

        [Benchmark]
        public TestElement RightShift_Element()
        {
            _a.Item3.RightShift(_d, out TestElement res);
            return res;
        }
    }

    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class Inverse : TwoParamBenchmarkBase
    {
        [Benchmark(Baseline = true)]
        public BigInteger Inverse_BigInteger()
        {
            return BigInteger.ModPow(_a.Item1, _bMod - 2, _bMod);
        }

        [Benchmark]
        public UInt256 Inverse_UInt256()
        {
            UInt256.ExpMod(_a.Item2, _uMod - 2, _uMod, out UInt256 res);
            return res;
        }

        [Benchmark]
        public TestElement Inverse_Element()
        {
            TestElement.Inverse(_a.Item3, out TestElement res);
            return res;
        }
        [Benchmark]
        public Element Inverse_gmp()
        {
            using mpz_t res = new();
            gmp_lib.mpz_init(res);
            gmp_lib.mpz_invert(res, _a.Item4, _gMod);
            byte[] result = new byte[32];
            Marshal.Copy(res.ToIntPtr(), result, 0, 32);
            return new Element(result);
        }
    }

    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class Sqrt : TwoParamBenchmarkBase
    {
        [Benchmark(Baseline = true)]
        public BigInteger Sqrt_BigInteger()
        {
            return BigInteger.ModPow(_a.Item1, _b.Item1, _bMod);
        }

        [Benchmark]
        public UInt256? Sqrt_UInt256()
        {
            return FieldMethods.ModSqrt(_a.Item2, _uMod);
        }

        [Benchmark]
        public TestElement? Sqrt_Element()
        {
            return TestElement.Sqrt(_a.Item3, out FpE res) ? res : null;
        }
    }
}
