// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nethermind.Int256;
using Nethermind.Verkle.Fields;
using Nethermind.Verkle.Fields.FpEElement;

namespace Nethermind.Verkle.Bench
{
    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class ArithmeticOps : TwoParamBenchmarkBase
    {
        [Benchmark]
        public FpE Add()
        {
            FpE.AddMod(_a, _b, out FpE res);
            return res;
        }

        [Benchmark]
        public FpE Subtract()
        {
            FpE.SubtractMod(_a, _b, out FpE res);
            return res;
        }

        [Benchmark]
        public FpE Multiply()
        {
            FpE.MultiplyMod(_a, _b, out FpE res);
            return res;
        }

        [Benchmark]
        public FpE Divide()
        {
            FpE.Divide(_a, _b, out FpE res);
            return res;
        }

        [Benchmark]
        public FpE Exp()
        {
            FpE.Exp(_a, FpE._bLegendreExponentElement.Value, out FpE res);
            return res;
        }

        [Benchmark]
        public FpE Inverse()
        {
            FpE.InverseOld(_a, out FpE res);
            return res;
        }

        [Benchmark]
        public FpE InverseNew()
        {
            FpE.Inverse(_a, out FpE res);
            return res;
        }

        [Benchmark]
        public FpE? Sqrt()
        {
            return FpE.Sqrt(_a, out FpE res) ? res : null;
        }
    }
}
