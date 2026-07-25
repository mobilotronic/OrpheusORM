# OrpheusORM v2

OrpheusORM is a module-oriented ORM with built-in schema evolution for .NET — supporting SQL Server, MySQL, and PostgreSQL with full async API, connection pooling, and aggregate-level persistence via the unique Module system.

[![CI Build](https://github.com/mobilotronic/OrpheusORM/actions/workflows/buildWorkFlow.yml/badge.svg)](https://github.com/mobilotronic/OrpheusORM/actions/workflows/buildWorkFlow.yml)

## v2.0.0 Highlights

- **Async API** — full async support across all I/O methods with `CancellationToken`
- **Connection Pooling** — native ADO.NET connection pooling via `IOrpheusConnectionFactory`
- **PostgreSQL Support** — new DDL helper backed by [Npgsql](https://www.npgsql.org/)
- **Modern DI** — constructor injection replaces the static `ServiceManager` pattern
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

## Documentation
To get started, visit [Orpheus documentation](https://mobilotronic.github.io/OrpheusORM/).

## NuGet Packages
* [OrpheusORM](https://www.nuget.org/packages/OrpheusORM/)
* [Orpheus SQL Server DDL Helper](https://www.nuget.org/packages/OrpheusORMSQLServerDDLHelper/)
* [Orpheus MySQL Server DDL Helper](https://www.nuget.org/packages/OrpheusORMMySQLServerDDLHelper/)
* [Orpheus PostgreSQL Server DDL Helper](https://www.nuget.org/packages/OrpheusORMPostgreSQLServerHelper/)

Read more about DDL helpers [here](https://mobilotronic.github.io/OrpheusORM/documentation/orpheus_ddl_helper.html). You can also implement your own DDL helper for any ADO.NET-compatible database engine.
