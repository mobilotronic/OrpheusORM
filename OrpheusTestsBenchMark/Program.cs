using BenchmarkDotNet.Running;
using System;

namespace OrpheusTestsBenchMark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<InsertDataBenchMark>();
            BenchmarkRunner.Run<LoadBenchMark>();
            BenchmarkRunner.Run<UpdateBenchMark>();
            BenchmarkRunner.Run<DeleteBenchMark>();
            Console.ReadKey();
        }
    }
}
