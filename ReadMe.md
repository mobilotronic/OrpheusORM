# OrpheusORM

OrpheusORM is a module-oriented ORM with built-in schema evolution for .NET — supporting SQL Server, MySQL, and PostgreSQL with full async API, connection pooling, and aggregate-level persistence via the unique Module system.

[![CI Build](https://github.com/mobilotronic/OrpheusORM/actions/workflows/buildWorkFlow.yml/badge.svg)](https://github.com/mobilotronic/OrpheusORM/actions/workflows/buildWorkFlow.yml)

## Highlights

- **Async API** — full async support across all I/O methods with `CancellationToken`
- **Connection Pooling** — native ADO.NET connection pooling via `IOrpheusConnectionFactory`
- **PostgreSQL Support** — new DDL helper backed by [Npgsql](https://www.npgsql.org/)
- **Modern DI** — constructor injection throughout, with no global state: register with `AddOrpheusSqlServer`/`AddOrpheusMySql`/`AddOrpheusPostgreSql` and everything resolves from your own container
- **Time-ordered keys** — auto-generated Guid keys are UUIDv7 by default, and the generator is swappable via `IOrpheusKeyGenerator`
- **Batched Insert/Update/Delete** — `Save()` sends fewer, larger round trips instead of one per row (tune via `IOrpheusTable.BatchSize`)
- **Performance** — cached model metadata + `IDisposable` implementations throughout

## Overview

### Schema creation
OrpheusORM has a built-in schema engine, which you can optionally use to create and/or update your schema, based on your model classes.

### Model binding
By default Orpheus assumes that your table names will match your model class names. You can override this with the `[TableName]` attribute.

### Nested data
Using an OrpheusModule you can save nested data (master-detail-subdetail) with just one Save. All master-detail relationships and keys are updated automatically.

## Supported Databases

| Engine      | NuGet Package |
|-------------|--------------|
| SQL Server  | [OrpheusORMSQLServerDDLHelper](https://www.nuget.org/packages/OrpheusORMSQLServerDDLHelper/) |
| MySQL       | [OrpheusORMMySQLServerDDLHelper](https://www.nuget.org/packages/OrpheusORMMySQLServerDDLHelper/) |
| PostgreSQL  | [OrpheusORMPostgreSQLServerHelper](https://www.nuget.org/packages/OrpheusORMPostgreSQLServerHelper/) |

## Try it

[`OrpheusDemoApi`](OrpheusDemoApi/README.md) is a runnable ASP.NET Core Minimal API that exercises
the schema engine, the Module system, async CRUD and batching against PostgreSQL. It creates its own
database and tables on first start:

```bash
docker compose -f dockerDevDB.yml up -d postgres
dotnet run --project OrpheusDemoApi
```

## Documentation
To get started, visit [Orpheus documentation](https://mobilotronic.github.io/OrpheusORM/).

## NuGet Packages
* [OrpheusORM](https://www.nuget.org/packages/OrpheusORM/)
* [Orpheus SQL Server DDL Helper](https://www.nuget.org/packages/OrpheusORMSQLServerDDLHelper/)
* [Orpheus MySQL Server DDL Helper](https://www.nuget.org/packages/OrpheusORMMySQLServerDDLHelper/)
* [Orpheus PostgreSQL Server DDL Helper](https://www.nuget.org/packages/OrpheusORMPostgreSQLServerHelper/)

Read more about DDL helpers [here](https://mobilotronic.github.io/OrpheusORM/documentation/orpheus_ddl_helper.html). You can also implement your own DDL helper for any ADO.NET-compatible database engine.
