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
One easy way to configure Orpheus is by using a configuration file. The configuration file, is basically a XML file that has the DI configuration for Orpheus.

* To initialize the configuration you can use the Orpheus configuration object.
    ```csharp
    OrpheusCore.Configuration.ConfigurationManager.InitializeConfiguration();
    ```
    By default Orpheus will try to find file ```OrpheusCore.config``` in the executing folder.
    Alternatively you can define a file name.
    ```csharp
    OrpheusCore.Configuration.ConfigurationManager.InitializeConfiguration("MyPath\Orpheus.config");
    ```
    ##### Configuration Sample
    ```xml
    <?xml version="1.0"?>
    <OrpheusConfiguration xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
      <Services>
        <ServiceProviderItem>
          <Implementation>System.Data.SqlClient.SqlConnection, System.Data.SqlClient, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a</Implementation>
          <Service>System.Data.IDbConnection, System.Data.Common, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a</Service>
          <ServiceLifeTime>Transient</ServiceLifeTime>
        </ServiceProviderItem>
        <ServiceProviderItem>
          <Implementation>OrpheusCore.OrpheusDatabase, OrpheusCore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null</Implementation>
          <Service>OrpheusInterfaces.IOrpheusDatabase, OrpheusInterfaces, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null</Service>
          <ServiceLifeTime>Transient</ServiceLifeTime>
        </ServiceProviderItem>
        <ServiceProviderItem>
          <Implementation>OrpheusSQLDDLHelper.OrpheusSQLServerDDLHelper, OrpheusSQLServerDDLHelper, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null</Implementation>
          <Service>OrpheusInterfaces.IOrpheusDDLHelper, OrpheusInterfaces, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null</Service>
          <ServiceLifeTime>Transient</ServiceLifeTime>
        </ServiceProviderItem>
        <ServiceProviderItem>
          <Implementation>OrpheusLogger.OrpheusFileLogger, OrpheusLogger, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null</Implementation>
          <Service>Microsoft.Extensions.Logging.ILogger, Microsoft.Extensions.Logging.Abstractions, Version=2.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60</Service>
          <ServiceLifeTime>Singleton</ServiceLifeTime>
        </ServiceProviderItem>
      </Services>
      <Logging Level="Error" MaxFileSize="1" />
    </OrpheusConfiguration>
    ```

#### Configuration by code
If you don't want to have that configuration in a file or if you already have a configuration file, that has a different schema/structure
than the one that Orpheus supports, you can initialize configuration by code.

You simply create a ```OrpheusConfiguration``` class and populate it's properties.

Imagine this scenario, where you have an ASP.net Core application and you want to use Orpheus,
but you don't want to have a separate configuration file.
```csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();
    var orpheusConfig = new OrpheusCore.Configuration.OrpheusConfiguration();
    //here instead of having the services hardcoded, you can read them from your web.config or appsettings.json
    //and populate the Services list.
    orpheusConfig.Services = new List<OrpheusCore.Configuration.ServiceProviderItem>()
    {
        new OrpheusCore.Configuration.ServiceProviderItem()
        {
            Service = "System.Data.IDbConnection, System.Data.Common, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            Implementation = "System.Data.SqlClient.SqlConnection, System.Data.SqlClient, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            ServiceLifetime =  ServiceLifetime.Transient
        }
    };
    OrpheusCore.Configuration.ConfigurationManager.InitializeConfiguration(orpheusConfig);
    OrpheusCore.ServiceProvider.OrpheusServiceProvider.InitializeServiceCollection(services);
}
```