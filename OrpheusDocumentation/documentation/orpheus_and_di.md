# Orpheus and DI
One of the first things that need to happen in an OrpheusORM application,
is to initialize Orpheus's configuration.

### Why use DI?
The reason for using an IoC container, is configurability and extensibility.
Having all the configuration in a file, the consumer can define the database engine of his
choosing as long as it implements ```System.Data.IDbConnection```, or a different logger component as long as it implements ```Microsoft.Extensions.Logging.ILogger```.

**Note:** At the moment only SQL Server and MySQL have been tested. Other DB engines might work, but there is no guarantee.

Read about Microsoft's DI [here](https://msdn.microsoft.com/en-us/magazine/mt707534.aspx)

### Agnostic Database Engine
By design Orpheus, does not depend nor include any code that targets specifically a database engine. 
This means that the consumer will need to somehow configure, which database engine will Orpheus target.

Here comes into play Orpheus's configuration. Either by file or by code, you can define the database engine for Orpheus.

#### Configuration by file
One easy way to configure Orpheus is by using a configuration file. The configuration file, is basically a JSON file that has the DI configuration for Orpheus.

* To initialize the configuration you can use the Orpheus configuration object.
    ```csharp
    OrpheusCore.Configuration.ConfigurationManager.InitializeConfiguration(IConfiguration configuration, IServiceCollection services = null);
    ```
    If no services are defined, then Orpheus will be in self-service mode. This means that it will create its own service collection and register all required services there.

    Alternatively you can define a file name. This will implicitly set Orpheus to self-service mode.
    ```csharp
    OrpheusCore.Configuration.ConfigurationManager.InitializeConfiguration("MyPath\appSettings.json");
    ```
    **Note:** You don't have to have a separate file for Orpheus's configuration. Its configuration can live inside your existing configuration file.

    ##### Configuration Sample
    ```javascript
    {
      "Services": [
        {
          "Implementation": "System.Data.SqlClient.SqlConnection, System.Data.SqlClient, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
          "Service": "System.Data.IDbConnection, System.Data.Common, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
          "ServiceLifeTime": "Transient"
        },
        {
          "Implementation": "OrpheusCore.OrpheusDatabase, OrpheusCore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
          "Service": "OrpheusInterfaces.Core.IOrpheusDatabase, OrpheusInterfaces, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
          "ServiceLifeTime": "Transient"
        },
        {
          "Implementation": "OrpheusSQLDDLHelper.OrpheusSQLServerDDLHelper, OrpheusSQLServerDDLHelper, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
          "Service": "OrpheusInterfaces.Core.IOrpheusDDLHelper, OrpheusInterfaces, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
          "ServiceLifeTime": "Transient"
        }
      ],
      "DatabaseConnection": 
        {
          "ConfigurationName": "ServiceConnection",
          "Server": "[YourServer]",
          "DatabaseName": "[YourDatabase]",
          "UseIntegratedSecurity": true
        }
    }
    ```
