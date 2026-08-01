# Connecting to a database
With Orpheus you can have multiple connections to a different (or the same) database, at the
same time.

You can configure multiple database connections in the configuration file. Consider having the
following configuration:

```json
{
  "OrpheusConfiguration": {
    "DatabaseConnections": [
      {
        "ConfigurationName": "Database1",
        "Server": "Server1",
        "DatabaseName": "Database1",
        "UseIntegratedSecurity": false,
        "UseIntegratedSecurityForServiceConnection": false,
        "ServiceUserName": "[yourusername]",
        "ServicePassword": "[yourpassword]"
      },
      {
        "ConfigurationName": "Database2",
        "Server": "Server2",
        "DatabaseName": "Database2",
        "UseIntegratedSecurity": false,
        "UseIntegratedSecurityForServiceConnection": false,
        "ServiceUserName": "[yourusername]",
        "ServicePassword": "[yourpassword]",
        "UserName": "[yourusername]",
        "Password": "[yourpassword]"
      }
    ]
  }
}
```

First, let's load the configuration:
```csharp
var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.SetBasePath("YourPathHere");
configurationBuilder.AddJsonFile("OrpheusConfig.json", optional: false, reloadOnChange: true);
var configuration = configurationBuilder.Build();

var orpheusConfig = new OrpheusConfiguration();
configuration.GetSection("OrpheusConfiguration").Bind(orpheusConfig);
```

Since a normal DI registration (`AddOrpheusSqlServer(...)`, see [DI Configuration](orpheus_and_di.md))
registers one `IOrpheusDatabase` per container, connecting to two databases *simultaneously* is
most straightforward using each engine's static factory — each call produces its own independent,
fully-configured database, so there's no registration to collide:

```csharp
var db1Config = orpheusConfig.DatabaseConnections.First(c => c.ConfigurationName == "Database1");
var database1 = OrpheusSQLServerDatabase.CreateDatabase(db1Config);
database1.Connect();

var db2Config = orpheusConfig.DatabaseConnections.First(c => c.ConfigurationName == "Database2");
var database2 = OrpheusSQLServerDatabase.CreateDatabase(db2Config);
database2.Connect();
```

If you only need a single connection and you're already using a DI container, prefer
`services.AddOrpheusSqlServer(dbConfig)` instead (see [DI Configuration](orpheus_and_di.md)) —
it registers a pooled connection factory, DDL helper, and `IOrpheusDatabase` together, resolved
from your container like any other service.

### Connection pooling
Every connection Orpheus opens — the main connection and the auxiliary connections the DDL
helper uses for schema/administrative operations — goes through the same pooled ADO.NET
connection string, built from `IDatabaseConnectionConfiguration`'s pooling settings:

| Setting | Meaning | Default |
|---|---|---|
| `Pooling` | Whether ADO.NET connection pooling is enabled | `true` |
| `MinPoolSize` | Minimum number of connections maintained in the pool | `0` |
| `MaxPoolSize` | Maximum number of connections allowed in the pool | `100` |
| `ConnectionIdleTimeout` | Seconds a connection can remain idle in the pool before being removed | `300` |

`OrpheusDatabase.Connect()`/`Disconnect()` lease and return one connection per `IOrpheusDatabase`
instance from the pool — this is not per-operation connection leasing, so a long-lived
`IOrpheusDatabase` instance holds one pooled connection for its lifetime, same as before pooling
was added.

For more details on each configuration option, see
[Database Connection Configuration](../api/OrpheusCore.Configuration.Models.DatabaseConnectionConfiguration.yml).
