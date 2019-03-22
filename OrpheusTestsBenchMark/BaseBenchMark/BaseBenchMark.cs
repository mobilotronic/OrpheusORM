using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using OrpheusTests;

namespace OrpheusTestsBenchMark
{
    [Config(typeof(Config))]
    public class BaseBenchMark : BaseTestClass
    {
        protected virtual void initializeBenchMark() { }

        public const int Iterations = 1;

        public BaseBenchMark()
        {
            this.Initialize();
            this.initializeBenchMark();
        }
    }

    public class Config : ManualConfig
    {
        public Config()
        {
            Add(MemoryDiagnoser.Default);
            Add(Job.Default.With(Jit.RyuJit).With(Runtime.Core).With(CsProjCoreToolchain.NetCoreApp22)
                //.WithUnrollFactor(BaseBenchMark.Iterations)
                //.WithIterationTime(new TimeInterval(500, TimeUnit.Millisecond))
                .WithLaunchCount(1)
                .WithIterationCount(15)
                .WithWarmupCount(15)
                //.WithOutlierMode(BenchmarkDotNet.Mathematics.OutlierMode.All)
                .WithUnrollFactor(BaseBenchMark.Iterations)
            );
        }
    }
}
