using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
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
                .WithToolchain(CsProjCoreToolchain.NetCoreApp90)
                // Monitoring is BenchmarkDotNet's strategy for I/O-bound benchmarks where each
                // iteration is one real, "expensive" operation (DB/network calls) rather than a
                // tight in-memory loop — it skips the pilot/overhead-estimation stages built for
                // batching many cheap invocations per iteration, which don't apply here since
                // InvocationCount is pinned to 1 anyway (see below).
                .WithStrategy(RunStrategy.Monitoring)
                .WithLaunchCount(1)
                // Bimodal iteration-time distributions on these benchmarks were traced to
                // warm-up residue (JIT tiering, SQL Server query-plan compilation) bleeding into
                // measured iterations. More warmup lets both stabilize before anything is
                // measured; more iterations tightens the confidence interval and reduces the
                // influence of any remaining outliers.
                .WithWarmupCount(30)
                .WithIterationCount(30)
                .WithUnrollFactor(BaseBenchMark.Iterations)
                // Without an explicit InvocationCount, BenchmarkDotNet's pilot stage batches
                // many calls to the [Benchmark] method into a single measured iteration
                // (it can climb into the hundreds), and [IterationSetup]/[IterationCleanup]
                // only wrap the whole batch, not each call. For benchmarks whose setup seeds
                // or clears rows for exactly one operation (insert/update/delete), that lets
                // later calls within a batch run against already-mutated data. Pinning
                // InvocationCount to 1 forces exactly one [Benchmark] call per
                // IterationSetup/IterationCleanup cycle.
                .WithInvocationCount(BaseBenchMark.Iterations)
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
