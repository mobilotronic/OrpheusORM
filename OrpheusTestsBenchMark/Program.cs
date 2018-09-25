using BenchmarkDotNet.Running;
using System;

namespace OrpheusTestsBenchMark
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseBenchMark = new InsertDataBenchMark();
            Console.WriteLine("========== Recreating schema ==========");
            baseBenchMark.ReCreateSchema();
            Console.WriteLine("========== Schema recreated ==========");
            Console.WriteLine($"========== Logging level: {OrpheusCore.Configuration.ConfigurationManager.Configuration.Logging.Level} ==========");
            Console.WriteLine($"========== Logging file: {OrpheusCore.Configuration.ConfigurationManager.Configuration.Logging.FilePath} ==========");
            BenchmarkRunner.Run<InsertDataBenchMark>();
            BenchmarkRunner.Run<LoadBenchMark>();
            BenchmarkRunner.Run<UpdateBenchMark>();
            BenchmarkRunner.Run<DeleteBenchMark>();
            Console.ReadKey();
        }
    }
}
