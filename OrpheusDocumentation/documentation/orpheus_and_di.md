# Orpheus and DI
One of the first things that need to happen in an OrpheusORM application
is registering Orpheus's services — and, for whichever database engine you're
targeting, wiring up a connection factory and DDL helper for it.

### Why use DI?
The reason for using an IoC container is configurability and extensibility. Orpheus doesn't
depend on, or include, any code that targets a specific database engine — the engine is
selected entirely by which services you register.

Read about Microsoft's DI [here](https://msdn.microsoft.com/en-us/magazine/mt707534.aspx)

### The easy way: per-engine registration
Each database engine ships its own DDL helper package (`OrpheusSQLServerDDLHelper`,
`OrpheusMySQLDDLHelper`, `OrpheusPostgreSQLDDLHelper`), and each one provides a single
`IServiceCollection` extension method — mirroring the `UseSqlServer()`/`UseNpgsql()` pattern
from EF Core — that registers everything Orpheus needs for that engine in one call: a pooled
connection factory, the engine's DDL helper, and `IOrpheusDatabase` itself, already configured.

```csharp
var services = new ServiceCollection();

services.AddOrpheusSqlServer(config =>
{
    config.Server = "YourServer";
    config.DatabaseName = "YourDatabase";
    config.UseIntegratedSecurity = true;
});

var provider = services.BuildServiceProvider();
var db = provider.GetRequiredService<IOrpheusDatabase>(); // already fully configured
db.Connect();
```

The MySQL and PostgreSQL equivalents are `AddOrpheusMySql(...)` (in `OrpheusMySQLDDLHelper`) and
`AddOrpheusPostgreSql(...)` (in `OrpheusPostgreSQLDDLHelper`) — same shape, same configuration
options. You can also pass an already-built `IDatabaseConnectionConfiguration` instead of a
configuration lambda, which is convenient when the configuration is coming from a file (see
below).

Internally, each of these calls `AddOrpheusServices()` to register Orpheus's own internal
services (table options, module/schema types, and a console logging fallback if nothing else has
registered `ILoggerFactory`) — you don't need to call `AddOrpheusServices()` yourself unless
you're wiring up a database engine manually (see "Registering things yourself" below).

### Configuration by file
The configuration lambda above can just as easily be filled from an `IConfiguration` section
instead of hardcoded values. A typical `OrpheusConfig.json` looks like this:

```json
{
  "OrpheusConfiguration": {
    "DatabaseConnections": [
      {
        "ConfigurationName": "Database1",
        "Server": "YourServer",
        "DatabaseName": "YourDatabase",
        "UseIntegratedSecurity": false,
        "UseIntegratedSecurityForServiceConnection": false,
        "UserName": "[yourusername]",
        "Password": "[yourpassword]",
        "ServiceUserName": "[yourusername]",
        "ServicePassword": "[yourpassword]",
        "Pooling": true,
        "MinPoolSize": 0,
        "MaxPoolSize": 100,
        "ConnectionIdleTimeout": 300
      }
    ]
  }
}
```

```csharp
var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.SetBasePath("YourPathHere");
configurationBuilder.AddJsonFile("OrpheusConfig.json", optional: false, reloadOnChange: true);
var configuration = configurationBuilder.Build();

var services = new ServiceCollection();
services.AddOrpheusSqlServer(configuration, "Database1");
```

`AddOrpheusSqlServer(configuration, connectionName)` binds the `"OrpheusConfiguration"` section and
looks up the connection whose `ConfigurationName` matches `connectionName` for you. If your settings
live under a different section, pass it as a third argument:
`services.AddOrpheusSqlServer(configuration, "Database1", "MyApp:Orpheus")`.

For details on every connection option — including the pooling settings (`Pooling`,
`MinPoolSize`, `MaxPoolSize`, `ConnectionIdleTimeout`) that get applied to the underlying ADO.NET
connection pool — see [Connecting to a database](orpheus_connecting_to_db.md) and
[Database Connection Configuration](../api/OrpheusCore.Configuration.Models.DatabaseConnectionConfiguration.yml).

### Without a DI container
If your application isn't using an `IServiceCollection` at all (a console app or script, for
example), each engine's DDL helper package also ships a static factory that builds a fully
working, pooling-aware `IOrpheusDatabase` on its own:

```csharp
var db = OrpheusSQLServerDatabase.CreateDatabase(config => {
    config.Server = "YourServer";
    config.DatabaseName = "YourDatabase";
    config.UseIntegratedSecurity = true;
});
db.Connect();
```

`OrpheusMySQLServerDatabase.CreateDatabase(...)` and `OrpheusPostgreSQLServerDatabase.CreateDatabase(...)`
work the same way. Internally these build their own small, private service container, so the
result is identical to what you'd get by registering with `AddOrpheusSqlServer(...)` and
resolving — it's just self-contained.

### Registering things yourself
The per-engine extension methods cover the common case, but they're a thin convenience layer —
nothing stops you from registering `IOrpheusConnectionFactory`, `IOrpheusDDLHelper`, and
`IOrpheusDatabase` directly, for example if you need a custom `IOrpheusDDLHelper` for a database
engine Orpheus doesn't ship support for (see [Orpheus DDL Helper](orpheus_ddl_helper.md)):

```csharp
services.AddOrpheusServices();
services.AddTransient<IOrpheusConnectionFactory, MyCustomConnectionFactory>();
services.AddTransient<IOrpheusDDLHelper, MyCustomDDLHelper>();
services.AddTransient<IOrpheusDatabase, OrpheusDatabase>();
```

**Note:** The static `ServiceManager.ServiceProvider` property and `ServiceManager.Resolve<T>()`
helpers are retained for backward compatibility but are obsolete — prefer resolving
`IOrpheusDatabase` from your `IServiceProvider` (constructor injection, or
`provider.GetRequiredService<IOrpheusDatabase>()`) instead.
