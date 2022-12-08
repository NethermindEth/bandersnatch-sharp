using System.Diagnostics;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Nethermind.Field.Montgomery.Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IConfig config = Debugger.IsAttached ? new DebugInProcessConfig() : DefaultConfig.Instance;

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        }
    }
}
