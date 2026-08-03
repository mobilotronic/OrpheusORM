using OrpheusDemoApi.Models;
using OrpheusInterfaces.Core;

namespace OrpheusDemoApi.Data;

/// <summary>
/// Puts a handful of customers and products in the database so the sample has something to show on
/// first run.
/// </summary>
/// <remarks>
/// Orpheus can attach seed data directly to a schema table via <c>ISchemaTable.SetData&lt;T&gt;()</c>,
/// which is the right tool for immutable lookup tables. This sample deliberately does not use it:
/// seed data attached that way is re-inserted whenever the schema object is altered, not only when
/// it is first created, so the moment you add a property to a model and restart — the exact thing
/// the schema-evolution demo asks you to try — the seed rows would replay and violate the primary
/// key. Guarding on a row count instead keeps both features working, and shows off
/// <c>GetTableCount&lt;T&gt;()</c> along the way.
/// </remarks>
public sealed class DemoDataSeeder(ILogger<DemoDataSeeder> logger)
{
    public static readonly Guid AcmeCustomerId = new("1B7E4E52-9D0A-4C1E-8F3A-6C5D2E9B4A71");
    public static readonly Guid GlobexCustomerId = new("2C8F5F63-AE1B-4D2F-9A4B-7D6E3F0C5B82");
    public static readonly Guid InitechCustomerId = new("3D906074-BF2C-4E30-AB5C-8E7F401D6C93");

    public static readonly Guid WidgetProductId = new("4EA17185-C03D-4F41-BC6D-9F80512E7DA4");
    public static readonly Guid GadgetProductId = new("5FB28296-D14E-4052-CD7E-A091623F8EB5");
    public static readonly Guid SprocketProductId = new("60C393A7-E25F-4163-DE8F-B1A27340FFC6");

    public async Task SeedAsync(IOrpheusDatabase database, CancellationToken cancellationToken = default)
    {
        if (database.GetTableCount<Customer>() > 0)
        {
            logger.LogInformation("Demo data already present, skipping seed.");
            return;
        }

        logger.LogInformation("Seeding demo customers and products.");

        using (var customers = database.CreateTable<Customer>())
        {
            customers.Add(
            [
                new Customer { CustomerId = AcmeCustomerId, Code = "ACME", Name = "Acme Corporation", Email = "orders@acme.example" },
                new Customer { CustomerId = GlobexCustomerId, Code = "GLOBEX", Name = "Globex Inc.", Email = "purchasing@globex.example" },
                new Customer { CustomerId = InitechCustomerId, Code = "INITECH", Name = "Initech LLC", Email = "ap@initech.example" },
            ]);
            await customers.SaveAsync(cancellationToken);
        }

        using (var products = database.CreateTable<Product>())
        {
            products.Add(
            [
                new Product { ProductId = WidgetProductId, Code = "WID-1", Description = "Widget", Price = 9.99 },
                new Product { ProductId = GadgetProductId, Code = "GAD-1", Description = "Gadget", Price = 24.50 },
                new Product { ProductId = SprocketProductId, Code = "SPR-1", Description = "Sprocket", Price = 4.75 },
            ]);
            await products.SaveAsync(cancellationToken);
        }
    }
}
