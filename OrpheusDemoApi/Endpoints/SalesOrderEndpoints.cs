using OrpheusDemoApi.Contracts;
using OrpheusDemoApi.Data;
using OrpheusDemoApi.Models;
using OrpheusInterfaces.Core;

namespace OrpheusDemoApi.Endpoints;

/// <summary>
/// The order aggregate, handled through an Orpheus <see cref="IOrpheusModule"/>. This is the feature
/// that has no direct equivalent in other .NET ORMs, so it is the part of the sample worth reading
/// closely.
/// </summary>
/// <remarks>
/// A module is a group of tables that are saved and loaded as one unit:
/// <list type="bullet">
/// <item><b>Main table</b> — the aggregate root (<see cref="SalesOrder"/>).</item>
/// <item><b>Detail tables</b> — children of the main table (<see cref="SalesOrderLine"/>), declared
/// with the master table's name and the key field that links them.</item>
/// <item><b>Reference tables</b> — lookups the aggregate points at but does not own
/// (<see cref="Customer"/>, <see cref="Product"/>). They are loaded with the module but never
/// cascaded into by a save.</item>
/// </list>
/// One <c>Save()</c> writes the whole graph in a single transaction, generating the master key and
/// propagating it into every detail row on the way. One <c>Load()</c> brings the whole graph back.
/// <para>
/// Note that <see cref="IOrpheusModule"/> is synchronous — unlike <c>IOrpheusTable</c>, it has no
/// async surface today, so the handlers below await the connection and then call <c>Save</c>/
/// <c>Load</c> directly.
/// </para>
/// </remarks>
public static class SalesOrderEndpoints
{
    private const string SalesOrderTable = nameof(SalesOrder);
    private const string SalesOrderLineTable = nameof(SalesOrderLine);
    private const string CustomerTable = nameof(Customer);
    private const string ProductTable = nameof(Product);

    /// <summary>
    /// Builds the order module. This is the whole mapping — there is no configuration file and
    /// nothing is persisted about the module unless you ask for it via
    /// <c>IOrpheusModuleDefinition.SaveToDB()</c>.
    /// </summary>
    private static IOrpheusModule CreateOrderModule(IOrpheusDatabase database)
    {
        var definition = database.CreateModuleDefinition();

        definition.MainTableOptions = definition.CreateTableOptions(SalesOrderTable, typeof(SalesOrder));

        // Lookups the aggregate reads but does not own.
        definition.ReferenceTableOptions.Add(definition.CreateTableOptions(CustomerTable, typeof(Customer)));
        definition.ReferenceTableOptions.Add(definition.CreateTableOptions(ProductTable, typeof(Product)));

        // The master-detail link. AddMasterKeyField names the field on the detail model that Orpheus
        // fills in from the master record's key during Save.
        var lineOptions = definition.CreateTableOptions(SalesOrderLineTable, typeof(SalesOrderLine));
        lineOptions.MasterTableName = SalesOrderTable;
        lineOptions.AddMasterKeyField(nameof(SalesOrder.SalesOrderId));
        definition.DetailTableOptions.Add(lineOptions);

        return database.CreateModule(definition);
    }

    private static SalesOrderResponse ToResponse(SalesOrder order, IEnumerable<SalesOrderLine> lines) =>
        new(
            order.SalesOrderId,
            order.CustomerId,
            order.OrderNumber,
            order.OrderDate,
            lines
                .Where(l => l.SalesOrderId == order.SalesOrderId)
                .Select(l => new SalesOrderLineResponse(
                    l.SalesOrderLineId,
                    l.SalesOrderId,
                    l.ProductId,
                    l.Quantity,
                    l.Price,
                    l.TotalPrice))
                .ToList());

    public static RouteGroupBuilder MapSalesOrderEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/orders").WithTags("Orders (module)");

        group.MapPost("/", async (CreateSalesOrderRequest request, OrpheusSession session, CancellationToken ct) =>
        {
            if (request.Lines is null || request.Lines.Count == 0)
            {
                return Results.BadRequest("An order needs at least one line.");
            }

            var db = await session.ConnectAsync(ct);
            var module = CreateOrderModule(db);

            var orders = module.GetTable<SalesOrder>(SalesOrderTable);
            var lines = module.GetTable<SalesOrderLine>(SalesOrderLineTable);

            var order = new SalesOrder
            {
                CustomerId = request.CustomerId,
                OrderNumber = request.OrderNumber,
                OrderDate = DateTime.UtcNow,
            };
            orders.Add(order);

            // This is the point of the whole endpoint: SalesOrderId is never assigned here. The
            // order's own key does not exist yet either — it is generated during Save. Orpheus
            // generates the master key, then writes it into every detail row before inserting them.
            lines.Add(request.Lines.Select(line => new SalesOrderLine
            {
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                Price = line.Price,
                TotalPrice = line.Quantity * line.Price,
            }).ToList());

            // One call, one transaction, master and details together.
            module.Save();

            // The models were updated in place, so the generated keys can be read straight back.
            return Results.Created($"/api/orders/{order.SalesOrderId}", ToResponse(order, lines.Data));
        })
        .WithSummary("Create an order with its lines in a single Save()")
        .WithDescription("Lines are posted without an order id; the module assigns it. Check the response — every line carries the order's generated key.");

        group.MapGet("/{id:guid}", async (Guid id, OrpheusSession session, CancellationToken ct) =>
        {
            var db = await session.ConnectAsync(ct);
            var module = CreateOrderModule(db);

            // Loading from the module level loads the main table by key and then every detail table
            // that hangs off it. No joins to write, no include chains to remember.
            module.Load([id]);

            var order = module.GetTable<SalesOrder>(SalesOrderTable).Data.FirstOrDefault();
            if (order is null)
            {
                return Results.NotFound();
            }

            var lines = module.GetTable<SalesOrderLine>(SalesOrderLineTable).Data;
            return Results.Ok(ToResponse(order, lines));
        })
        .WithSummary("Load an order and its lines with a single Load()");

        group.MapGet("/", async (OrpheusSession session, CancellationToken ct) =>
        {
            var db = await session.ConnectAsync(ct);
            using var orders = db.CreateTable<SalesOrder>();
            await orders.LoadAsync(ct);
            return Results.Ok(orders.Data);
        })
        .WithSummary("List order headers")
        .WithDescription("Headers only — a plain table load, no module involved.");

        group.MapDelete("/{id:guid}", async (Guid id, OrpheusSession session, CancellationToken ct) =>
        {
            var db = await session.ConnectAsync(ct);
            var module = CreateOrderModule(db);
            module.Load([id]);

            var orderTable = module.GetTable<SalesOrder>(SalesOrderTable);
            var order = orderTable.Data.FirstOrDefault();
            if (order is null)
            {
                return Results.NotFound();
            }

            // Deleting the master queues the detail deletions too, so the foreign key is never
            // violated mid-transaction.
            var lineTable = module.GetTable<SalesOrderLine>(SalesOrderLineTable);
            lineTable.Delete(lineTable.Data.ToList());
            orderTable.Delete(order);
            module.Save();

            return Results.NoContent();
        })
        .WithSummary("Delete an order and its lines");

        return group;
    }
}
