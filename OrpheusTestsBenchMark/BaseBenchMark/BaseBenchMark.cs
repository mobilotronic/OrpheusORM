using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using OrpheusTests;
using System;
using System.Collections.Generic;
using System.Text;

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
                .WithUnrollFactor(BaseBenchMark.Iterations)
                //.WithIterationTime(new TimeInterval(500, TimeUnit.Millisecond))
                .WithLaunchCount(1)
                .WithWarmupCount(0)
                .WithTargetCount(5)
                .WithRemoveOutliers(true)
            );
        }
    }
}
