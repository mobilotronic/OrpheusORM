# Connecting to a database
With Orpheus you can multiple connections to a different (or the same) database, at the same time.

You can configure multiple database connections, in the configuration file. Consider having the following configuration

```json
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
  ],
```
First let's load the configuration
```csharp
    //creating an IConfiguration to pass it on to Orpheus.
    var configurationBuilder = new ConfigurationBuilder();
    configurationBuilder.SetBasePath("YourPathHere");
    configurationBuilder.AddJsonFile("appSettings.json", optional: false, reloadOnChange: true);
    this.configuration = configurationBuilder.Build();
    OrpheusCore.Configuration.ConfigurationManager.InitializeConfiguration(this.configuration);

    //creating a database to connect to server1/database1
    var database1 = OrpheusCore.ServiceProvider.OrpheusServiceProvider.Resolve<IOrpheusDatabase>();
    var dbConfiguration = OrpheusCore.Configuration.ConfigurationManager.Configuration.DatabaseConnections.FirstOrDefault(c => c.ConfigurationName.ToLower() == "database1");
    database1.Connect(dbConfiguration);

    //creating a database to connect to server2/database2
    var database2 = OrpheusCore.ServiceProvider.OrpheusServiceProvider.Resolve<IOrpheusDatabase>();
    var dbConfiguration = OrpheusCore.Configuration.ConfigurationManager.Configuration.DatabaseConnections.FirstOrDefault(c => c.ConfigurationName.ToLower() == "database2");
    database2.Connect(dbConfiguration);
```
For more details on each configuration option go to [Database Connection Configuration](../api/OrpheusCore.Configuration.Models.DatabaseConnectionConfiguration.yml)