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
            // Must match the engine the benchmark classes below actually run against
            // (they don't set DatabaseEngine themselves, so they default to dbSQLServer).
            // This was previously set to dbPostgreSQL, which recreated the wrong database
            // and left the SQL Server TestModelTransactor table growing unbounded across
            // every benchmark run (445,470 stale rows accumulated before this fix).
            baseBenchMark.DatabaseEngine = DbEngine.dbSQLServer;
            Console.WriteLine("========== Recreating schema ==========");
            baseBenchMark.ReCreateSchema();
            Console.WriteLine("========== Schema recreated ==========");
            //BenchmarkRunner.Run<DapperInsertBenchMark>();
            //BenchmarkRunner.Run<AsyncInsertBenchMark>();
            //BenchmarkRunner.Run<InsertDataBenchMark>();
            //BenchmarkRunner.Run<LoadBenchMark>();
            //BenchmarkRunner.Run<UpdateBenchMark>();
            //BenchmarkRunner.Run<DeleteBenchMark>();

            // Orpheus vs Dapper vs EF Core, at RowCount = 10/100/1000, with per-operation
            // table resets (see ComparisonBenchMarkBase) so results stay comparable run to run.
            BenchmarkRunner.Run<InsertComparisonBenchMark>();
            BenchmarkRunner.Run<LoadComparisonBenchMark>();
            BenchmarkRunner.Run<UpdateComparisonBenchMark>();
            BenchmarkRunner.Run<DeleteComparisonBenchMark>();
        }
    }
}
