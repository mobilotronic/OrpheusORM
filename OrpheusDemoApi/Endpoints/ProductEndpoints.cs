using System.Diagnostics;
using OrpheusDemoApi.Contracts;
using OrpheusDemoApi.Data;
using OrpheusDemoApi.Models;

namespace OrpheusDemoApi.Endpoints;

/// <summary>
/// Product CRUD, plus a bulk-insert endpoint that exposes Orpheus's batching knob.
/// </summary>
public static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", async (OrpheusSession session, CancellationToken ct) =>
        {
            var db = await session.ConnectAsync(ct);
            using var products = db.CreateTable<Product>();
            await products.LoadAsync(ct);
            return Results.Ok(products.Data);
        })
        .WithSummary("List all products");

        group.MapGet("/{id:guid}", async (Guid id, OrpheusSession session, CancellationToken ct) =>
        {
            var db = await session.ConnectAsync(ct);
            using var products = db.CreateTable<Product>();
            await products.LoadAsync([id], ct);

            var product = products.Data.FirstOrDefault();
            return product is null ? Results.NotFound() : Results.Ok(product);
        })
        .WithSummary("Get a product by id");

        group.MapPost("/", async (ProductRequest request, OrpheusSession session, CancellationToken ct) =>
        {
            var db = await session.ConnectAsync(ct);
            using var products = db.CreateTable<Product>();

            var product = new Product
            {
                Code = request.Code,
                Description = request.Description,
                Price = request.Price,
            };

            products.Add(product);
            await products.SaveAsync(ct);

            return Results.Created($"/api/products/{product.ProductId}", product);
        })
        .WithSummary("Create a product");

        group.MapDelete("/{id:guid}", async (Guid id, OrpheusSession session, CancellationToken ct) =>
        {
            var db = await session.ConnectAsync(ct);
            using var products = db.CreateTable<Product>();
            await products.LoadAsync([id], ct);

            var product = products.Data.FirstOrDefault();
            if (product is null)
            {
                return Results.NotFound();
            }

            products.Delete(product);
            await products.SaveAsync(ct);

            return Results.NoContent();
        })
        .WithSummary("Delete a product");

        group.MapPost("/bulk", async (
            OrpheusSession session,
            CancellationToken ct,
            int count = 1000,
            int batchSize = 100) =>
        {
            if (count is < 1 or > 100_000)
            {
                return Results.BadRequest("count must be between 1 and 100000.");
            }

            if (batchSize < 1)
            {
                return Results.BadRequest("batchSize must be at least 1.");
            }

            var db = await session.ConnectAsync(ct);
            using var products = db.CreateTable<Product>();

            // Save() does not issue one round trip per row. It groups queued inserts into multi-row
            // statements of at most BatchSize rows (default 100). Raising this trades memory and
            // statement size for far fewer round trips — try 1 vs 500 and compare the timings.
            products.BatchSize = batchSize;

            var suffix = DateTime.UtcNow.Ticks;
            products.Add(Enumerable.Range(0, count).Select(i => new Product
            {
                Code = $"BULK-{suffix}-{i}",
                Description = $"Bulk generated product {i}",
                Price = Math.Round(i % 100 + 0.99, 2),
            }).ToList());

            var stopwatch = Stopwatch.StartNew();
            await products.SaveAsync(ct);
            stopwatch.Stop();

            return Results.Ok(new BulkProductResult(count, batchSize, stopwatch.ElapsedMilliseconds));
        })
        .WithSummary("Bulk-insert products")
        .WithDescription("Demonstrates IOrpheusTable.BatchSize. Vary batchSize to see the effect on round trips.");

        return group;
    }
}
