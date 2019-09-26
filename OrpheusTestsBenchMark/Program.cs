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
            BenchmarkRunner.Run<InsertDataBenchMark>();
            BenchmarkRunner.Run<LoadBenchMark>();
            BenchmarkRunner.Run<UpdateBenchMark>();
            BenchmarkRunner.Run<DeleteBenchMark>();
            Console.ReadKey();
        }
    }
}
