using OrpheusDemoApi.Models;
using OrpheusInterfaces.Core;
using OrpheusInterfaces.Schema;

namespace OrpheusDemoApi.Schema;

/// <summary>
/// The demo's database schema, declared entirely from the model classes in
/// <see cref="OrpheusDemoApi.Models"/>.
/// </summary>
/// <remarks>
/// Orpheus reads the model's properties and attributes to work out columns, types, lengths, defaults
/// and constraints, so there is nothing to describe here beyond which tables exist and which depend
/// on which. Dependencies control creation order (and drop order, in reverse).
/// </remarks>
public sealed class DemoSchema(IOrpheusDatabase database)
{
    /// <summary>
    /// A stable id for this schema. Orpheus records it in its own bookkeeping tables so it can tell,
    /// on later runs, which objects it has already created. Keep it constant for the life of the
    /// application — generating a new one each run would orphan the previous schema's records.
    /// </summary>
    private static readonly Guid SchemaId = new("9F2C1A54-6E3B-4C7D-9A18-2B6D5E0F73C4");

    private ISchema Build()
    {
        var schema = database.CreateSchema(SchemaId, "Orpheus Demo Schema", 1.0);

        // No dependencies — these two are referenced by others, so they are created first.
        schema.AddSchemaTable<Customer>();
        schema.AddSchemaTable<Product>();

        var salesOrders = schema.AddSchemaTable<SalesOrder>();
        salesOrders.AddDependency<Customer>();

        var salesOrderLines = schema.AddSchemaTable<SalesOrderLine>();
        salesOrderLines.AddDependency<SalesOrder>();
        salesOrderLines.AddDependency<Product>();

        return schema;
    }

    /// <summary>
    /// Creates or updates the schema in the database.
    /// </summary>
    /// <remarks>
    /// Safe to call on every start-up. Orpheus checks whether each object already exists; if it
    /// does, the object is diffed against the model and only the differences are emitted as
    /// ALTER TABLE. When nothing has changed — the usual case — no DDL is executed at all.
    /// <para>
    /// Caveat on PostgreSQL: the no-change path works, but actually applying a difference does not.
    /// Tables are created with an unquoted name (which PostgreSQL lower-cases) and altered with a
    /// quoted one (which preserves case), so the ALTER fails with
    /// <c>42P01: relation "Customer" does not exist</c>. Adding a property to a model therefore
    /// needs the table dropped and recreated for now. See "Known rough edges" in the README.
    /// </para>
    /// </remarks>
    public void Execute() => this.Build().Execute();

    /// <summary>
    /// Drops every object in the schema. Used by the <c>DELETE /api/schema</c> endpoint so the demo
    /// can be reset without touching the database by hand.
    /// </summary>
    public void Drop() => this.Build().Drop();

    /// <summary>
    /// Describes the schema without touching the database — used by <c>GET /api/schema</c>.
    /// </summary>
    public (string Description, double Version, IReadOnlyList<string> Tables) Describe()
    {
        var schema = this.Build();
        return (schema.Description, schema.Version, schema.SchemaObjects.Select(o => o.SQLName).ToList());
    }
}
