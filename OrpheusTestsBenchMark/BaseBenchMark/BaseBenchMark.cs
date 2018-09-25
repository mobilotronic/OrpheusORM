using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
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
            Add(new MemoryDiagnoser());
            Add(Job.Default
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
