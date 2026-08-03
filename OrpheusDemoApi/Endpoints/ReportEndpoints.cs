using OrpheusDemoApi.Contracts;
using OrpheusDemoApi.Data;
using OrpheusDemoApi.Schema;

namespace OrpheusDemoApi.Endpoints;

/// <summary>
/// Dropping to SQL, and inspecting the schema.
/// </summary>
public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder routes)
    {
        var reports = routes.MapGroup("/api/reports").WithTags("Reports");

        reports.MapGet("/customer-totals", async (OrpheusSession session, CancellationToken ct) =>
        {
            var db = await session.ConnectAsync(ct);

            // Orpheus never gets in the way of SQL you want to write yourself. SQLAsync<T> runs the
            // statement and materialises the result into T by matching column names to properties —
            // T needs no attributes and no corresponding table.
            //
            // A note on casing, because it bites everyone once on PostgreSQL: Orpheus emits column
            // names delimited (so they keep their PascalCase) but table names bare (so PostgreSQL
            // folds them to lower case). Hence `customer`, but `c."Code"`. Result aliases are quoted
            // so they come back matching CustomerTotal's property names.
            const string sql = """
                SELECT  c."Code"                                    AS "Code",
                        c."Name"                                    AS "Name",
                        CAST(COUNT(DISTINCT o."SalesOrderId") AS INTEGER) AS "OrderCount",
                        COALESCE(SUM(l."TotalPrice"), 0)            AS "TotalValue"
                FROM    customer c
                        LEFT JOIN salesorder o ON o."CustomerId" = c."CustomerId"
                        LEFT JOIN salesorderline l ON l."SalesOrderId" = o."SalesOrderId"
                GROUP BY c."Code", c."Name"
                ORDER BY "TotalValue" DESC
                """;

            var totals = await db.SQLAsync<CustomerTotal>(sql, cancellationToken: ct);
            return Results.Ok(totals);
        })
        .WithSummary("Order totals per customer")
        .WithDescription("Raw SQL materialised into a typed projection via IOrpheusDatabase.SQLAsync<T>.");

        var schema = routes.MapGroup("/api/schema").WithTags("Schema");

        schema.MapGet("/", async (OrpheusSession session, CancellationToken ct) =>
        {
            var db = await session.ConnectAsync(ct);
            var (description, version, tables) = new DemoSchema(db).Describe();
            return Results.Ok(new SchemaResponse(description, version, tables));
        })
        .WithSummary("Describe the demo schema");

        schema.MapPost("/apply", async (OrpheusSession session, CancellationToken ct) =>
        {
            var db = await session.ConnectAsync(ct);

            // The same call the application makes at start-up, and a no-op when no model has
            // changed. Applying an actual difference currently fails on PostgreSQL — see the remarks
            // on DemoSchema.Execute.
            new DemoSchema(db).Execute();

            return Results.Ok(new { Applied = true });
        })
        .WithSummary("Re-apply the schema")
        .WithDescription("Idempotent — does nothing when the models match the database.");

        return routes;
    }
}
