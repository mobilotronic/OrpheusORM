# Configuring NLog
Here is an example of how to configure NLog with Orpheus.

Prerequisite is to add [NLog](https://github.com/NLog/NLog.Extensions.Logging) to your project.

```csharp
	LogManager.LoadConfiguration("nlog.config");
	var logger = LogManager.GetCurrentClassLogger();
	try
	{
		var configurationBuilder = new ConfigurationBuilder();
		configurationBuilder.AddJsonFile("appSettings.json", optional: false, reloadOnChange: true);
		var configuration = configurationBuilder.Build();

		IServiceCollection serviceCollection = new ServiceCollection();
		serviceCollection.AddLogging((builder) =>
		{
			builder.ClearProviders();
			//setting the MEL minimum level to trace, will essentially cancel whatever logging settings might present in the appsettings.json file
			//the logging level would be controlled solely from the NLog configuration file.
			builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
			builder.AddNLog(configuration);
		});

		// AddOrpheusSqlServer (or AddOrpheusMySql/AddOrpheusPostgreSql) internally calls
		// AddOrpheusServices(), which only falls back to console logging if nothing has
		// registered ILoggerFactory yet — so register logging (as above) before this call.
		var connectionConfig = new OrpheusCore.Configuration.Models.DatabaseConnectionConfiguration();
		configuration.GetSection("OrpheusConfiguration:DatabaseConnections:0").Bind(connectionConfig);
		serviceCollection.AddOrpheusSqlServer(connectionConfig);

		var provider = serviceCollection.BuildServiceProvider();
		var db = provider.GetRequiredService<IOrpheusDatabase>();
	}
	catch (Exception e)
	{
		logger.Log(NLog.LogLevel.Error, e);
	}
```


Other logging frameworks, might require a similar approach.