using Microsoft.EntityFrameworkCore;
using OrpheusTestModels;

namespace OrpheusTestsBenchMark
{
    /// <summary>
    /// EF Core mapping onto the same TestModelTransactor table used by the
    /// Orpheus and Dapper comparison benchmarks, so all three frameworks
    /// operate on identical schema/data.
    /// </summary>
    public class BenchmarkDbContext : DbContext
    {
        public DbSet<TestModelTransactor> TestModelTransactors => Set<TestModelTransactor>();

        public BenchmarkDbContext(DbContextOptions<BenchmarkDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestModelTransactor>(e =>
            {
                e.ToTable("TestModelTransactor");
                e.HasKey(t => t.TransactorId);
                e.Property(t => t.Type).HasConversion<int>();
            });
        }
    }
}
