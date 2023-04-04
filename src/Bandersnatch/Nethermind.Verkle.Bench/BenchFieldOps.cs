// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nethermind.Field;
using Nethermind.Field.Bench;
using Nethermind.Int256;
using Nethermind.Verkle.Fields.FpEElement;

namespace Nethermind.Verkle.Bench
{
    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class AddMod : TwoParamBenchmarkBase
    {
        [Benchmark(Baseline = true)]
        public BigInteger AddMod_BigInteger()
        {
            return (_a.Item1 + _b.Item1) % _bMod;
        }

        [Benchmark]
        public BigInteger AddMod_BigInteger_New()
        {
            return BigInteger.Remainder(_a.Item1 + _b.Item1, _bMod);
        }

        [Benchmark]
        public UInt256 AddMod_UInt256()
        {
            UInt256.AddMod(_a.Item2, _b.Item2, _uMod, out UInt256 res);
            return res;
        }

        [Benchmark]
        public FpE AddMod_Element()
        {
            FpE.AddMod(_a.Item3, _b.Item3, out FpE res);
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
        public FpE SubtractMod_Element()
        {
            FpE.SubtractMod(_a.Item3, _b.Item3, out FpE res);
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
        public FpE MultiplyMod_Element()
        {
            FpE.MultiplyMod(_a.Item3, _b.Item3, out FpE res);
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
            return BigInteger.Remainder(BigInteger.Divide(_a.Item1, _b.Item1), _bMod);
        }

        [Benchmark]
        public UInt256 Divide_UInt256()
        {
            UInt256.Divide(_a.Item2, _b.Item2, out UInt256 res);
            res.Mod(_uMod, out res);
            return res;
        }

        [Benchmark]
        public FpE Divide_Element()
        {
            FpE.Divide(_a.Item3, _b.Item3, out FpE res);
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
        public FpE ExpMod_Element()
        {
            FpE.Exp(_a.Item3, _b.Item2, out FpE res);
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
        public FpE Inverse_Element()
        {
            FpE.Inverse(_a.Item3, out FpE res);
            return res;
        }
    }

    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class Sqrt : TwoParamBenchmarkBase
    {
        [Benchmark(Baseline = true)]
        public BigInteger Sqrt_BigInteger()
        {
            return (BigInteger)Math.Exp(BigInteger.Log(_a.Item1) / 2);
        }

        [Benchmark]
        public UInt256? Sqrt_UInt256()
        {
            return FieldMethods.ModSqrt(_a.Item2, _uMod);
        }

        [Benchmark]
        public FpE? Sqrt_Element()
        {
            return FpE.Sqrt(_a.Item3, out FpE res) ? res : null;
        }
    }
}
