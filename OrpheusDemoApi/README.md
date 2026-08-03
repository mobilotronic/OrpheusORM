# OrpheusORM Demo API

A small ASP.NET Core Minimal API that shows what using [OrpheusORM](../ReadMe.md) actually looks
like: DI registration, schema generated from model classes, the Module system saving a master-detail
aggregate in one call, async table CRUD, batched inserts, and raw SQL projections.

Runs on PostgreSQL. It creates its own database and tables on first start, so the only prerequisite
is a running PostgreSQL.

## Run it

```bash
# from the repository root
docker compose -f dockerDevDB.yml up -d postgres
dotnet run --project OrpheusDemoApi
```

Then open <http://localhost:5266/scalar/v1> for the interactive API reference, or work through
[`OrpheusDemoApi.http`](OrpheusDemoApi.http).

Connection settings live in [`appsettings.json`](appsettings.json) under `OrpheusConfiguration`, and
match the `postgres` service in `dockerDevDB.yml` (127.0.0.1:5433, user `postgres`). The demo uses
its own database, `orpheusDemoDB`, so it never touches the test suite's `orpheusTestDB`.

Nothing needs to exist beforehand — connecting creates the database if it is missing, and start-up
then creates the tables and seeds three customers and three products.

## What to read, in order

| # | File | What it shows |
|---|------|---------------|
| 1 | [`Program.cs`](Program.cs) | Registering Orpheus. `AddOrpheusPostgreSql(config, "PostgreSQL")` really is the whole thing — no global state to initialise. |
| 2 | [`Models/SalesModels.cs`](Models/SalesModels.cs) | The entire mapping layer. Four classes, a handful of attributes, no context class and no migrations. |
| 3 | [`Data/OrpheusSession.cs`](Data/OrpheusSession.cs) | How a transient `IOrpheusDatabase` fits a request-scoped web app. The one piece of glue you have to write yourself. |
| 4 | [`Schema/DemoSchema.cs`](Schema/DemoSchema.cs) | Declaring the schema from the models, and the dependencies that order table creation. |
| 5 | [`Endpoints/SalesOrderEndpoints.cs`](Endpoints/SalesOrderEndpoints.cs) | **The interesting one.** The Module system: one `Save()` writes an order and its lines together and wires up the foreign keys. |
| 6 | [`Endpoints/CustomerEndpoints.cs`](Endpoints/CustomerEndpoints.cs) | Ordinary async CRUD over a single table. |
| 7 | [`Endpoints/ProductEndpoints.cs`](Endpoints/ProductEndpoints.cs) | `BatchSize` and bulk inserts. |
| 8 | [`Endpoints/ReportEndpoints.cs`](Endpoints/ReportEndpoints.cs) | Dropping to SQL and materialising the result into a typed projection. |

## The bit worth trying yourself

Post an order whose lines carry **no** order id:

```bash
curl -X POST http://localhost:5266/api/orders \
  -H "Content-Type: application/json" \
  -d '{
        "customerId": "1b7e4e52-9d0a-4c1e-8f3a-6c5d2e9b4a71",
        "orderNumber": "SO-1001",
        "lines": [
          { "productId": "4ea17185-c03d-4f41-bc6d-9f80512e7da4", "quantity": 3, "price": 9.99 },
          { "productId": "5fb28296-d14e-4052-cd7e-a091623f8eb5", "quantity": 2, "price": 24.50 }
        ]
      }'
```

Every line in the response comes back with `salesOrderId` set to the order's generated key. The
application never assigned it — the order's own key did not exist until `module.Save()` ran, and the
module propagated it into the detail rows inside the same transaction. `GET /api/orders/{id}` then
brings the whole aggregate back with a single `module.Load()`, no joins written by hand.

Then compare `POST /api/products/bulk?count=2000&batchSize=1` against
`?count=2000&batchSize=500` to see what `IOrpheusTable.BatchSize` is doing.

## Known rough edges

These are limitations of the current library, found while building this sample. They are recorded
here so the code above does not look like it is doing something odd for no reason.

- **Schema evolution does not currently work on PostgreSQL.** Re-running the schema is a safe no-op
  when nothing has changed — that part works, and start-up does it every time. But *adding* a
  property to a model and restarting fails: Orpheus emits `CREATE TABLE Customer` unquoted, which
  PostgreSQL folds to `customer`, then emits `ALTER TABLE "Customer"` quoted, which does not match
  and fails with `42P01: relation "Customer" does not exist`. The mismatch is between
  `SchemaObject.createDDLString` (unquoted) and `SafeFormatAlterTableAddColumn`/`DropColumn` in
  `OrpheusPostgreSQLDDLHelper` (quoted). SQL Server and MySQL are unaffected because their
  identifiers are case-insensitive. Until that is fixed, drop the tables and let start-up recreate
  them (`DELETE /api/schema` is not exposed for this reason — do it in psql).
- **Modules are synchronous.** `IOrpheusTable` has a full async surface; `IOrpheusModule` does not,
  so the order endpoints await the connection and then call `Save()`/`Load()` synchronously.

Three earlier entries are gone as of **2.1.0**, which fixed them in the library: the static
`ServiceManager.ServiceProvider` and `ConfigurationManager` initialisations `Program.cs` used to
need — both types have since been deleted outright — and the spurious `[PrimaryKey]` that read-only
projections like `CustomerTotal` had to declare.

## Keys

Primary keys are **UUIDv7** (`Guid.CreateVersion7()`), not random v4 — the column type is still
`uuid` and the model properties are still `Guid`, but the values carry a millisecond timestamp in
their leading bytes and so sort in creation order:

```sql
SELECT "SalesOrderId" FROM salesorder ORDER BY "SalesOrderId";  -- creation order
```

Swap the scheme with `services.AddOrpheusKeyGenerator<RandomGuidKeyGenerator>()`, or implement
`IOrpheusKeyGenerator` yourself. Note that the ordering benefit shows up in PostgreSQL and MySQL
indexes but not SQL Server's, whose `uniqueidentifier` comparison does not read bytes left to right.

## A note on naming

The order aggregate is `SalesOrder`, not `Order`, because Orpheus emits table names unquoted and
`ORDER` is a reserved word in PostgreSQL — a model class called `Order` produces
`CREATE TABLE Order (...)` and fails to parse. Columns are emitted delimited, so they keep their
PascalCase while table names arrive in the database lower-cased. That asymmetry is why the raw SQL in
`ReportEndpoints.cs` reads `FROM customer c ... c."Code"`.
