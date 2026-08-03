using OrpheusDemoApi.Schema;

namespace OrpheusDemoApi.Data;

/// <summary>
/// Brings the database up to date at start-up: creates it if missing, creates or evolves the schema,
/// then seeds demo data.
/// </summary>
/// <remarks>
/// Worth knowing: nothing here creates the <c>orpheusDemoDB</c> database, and nothing needs to.
/// <c>ConnectAsync</c> asks the DDL helper to create the database first if it is not there, so a
/// clean PostgreSQL container plus <c>dotnet run</c> is the whole setup story.
/// </remarks>
public sealed class SchemaInitializer(
    IServiceProvider serviceProvider,
    ILogger<SchemaInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // The database and session are scoped/transient services, so take a scope rather than
        // resolving them from the root provider.
        using var scope = serviceProvider.CreateScope();
        var session = scope.ServiceProvider.GetRequiredService<OrpheusSession>();
        var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();

        logger.LogInformation("Connecting to the demo database (creating it if it does not exist).");
        var database = await session.ConnectAsync(cancellationToken);

        logger.LogInformation("Applying schema.");
        new DemoSchema(database).Execute();

        await seeder.SeedAsync(database, cancellationToken);

        logger.LogInformation("Demo database is ready.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
