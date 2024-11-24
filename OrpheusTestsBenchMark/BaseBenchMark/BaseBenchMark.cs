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
            AddDiagnoser(MemoryDiagnoser.Default);
            AddJob(
                Job.Default.WithJit(Jit.RyuJit)
                .WithToolchain(CsProjCoreToolchain.NetCoreApp80)
                .WithLaunchCount(1)
                .WithIterationCount(15)
                .WithWarmupCount(15)
                .WithUnrollFactor(BaseBenchMark.Iterations)
            );

            //Add(Job.Default.With(Jit.RyuJit).With(CsProjCoreToolchain.NetCoreApp70)
            //    //.WithUnrollFactor(BaseBenchMark.Iterations)
            //    //.WithIterationTime(new TimeInterval(500, TimeUnit.Millisecond))
            //    .WithLaunchCount(1)
            //    .WithIterationCount(15)
            //    .WithWarmupCount(15)
            //    //.WithOutlierMode(BenchmarkDotNet.Mathematics.OutlierMode.All)
            //    .WithUnrollFactor(BaseBenchMark.Iterations)
            //);
        }
    }
}
