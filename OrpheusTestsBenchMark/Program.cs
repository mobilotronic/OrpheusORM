using BenchmarkDotNet.Running;
using OrpheusTests;
using System;

namespace OrpheusTestsBenchMark
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseBenchMark = new InsertDataBenchMark();
            baseBenchMark.DatabaseEngine = DbEngine.dbPostgreSQL;
            Console.WriteLine("========== Recreating schema ==========");
            baseBenchMark.ReCreateSchema();
            Console.WriteLine("========== Schema recreated ==========");
            BenchmarkRunner.Run<AsyncInsertBenchMark>();
            //BenchmarkRunner.Run<InsertDataBenchMark>();
            //BenchmarkRunner.Run<LoadBenchMark>();
            //BenchmarkRunner.Run<UpdateBenchMark>();
            //BenchmarkRunner.Run<DeleteBenchMark>();
        }
    }
}
