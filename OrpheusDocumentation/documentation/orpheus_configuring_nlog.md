# Configuring NLog
Here is an example of how to configure NLog with Orpheus.

Prerequisite is to add [NLog](https://github.com/NLog/NLog.Extensions.Logging) to your project.

```csharp
	LogManager.LoadConfiguration("nlog.config");
	var logger = LogManager.GetCurrentClassLogger();
	try
	{
		IServiceCollection serviceCollection = new ServiceCollection();
		this.configuration = this.createConfiguration("appSettings.json");
		serviceCollection.AddLogging((builder) =>
		{
			builder.ClearProviders();
			//setting the MEL minimum level to trace, will essentially cancel whatever logging settings might present in the appsettings.json file
			//the logging level would be controlled solely from the NLog configuration file.
			builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
			builder.AddNLog(configuration);
		});
		OrpheusCore.Configuration.ConfigurationManager.InitializeConfiguration(this.configuration, serviceCollection);
	}
	catch (Exception e)
	{
		logger.Log(NLog.LogLevel.Error, e);
	}
```


Other logging frameworks, might require a similar approach.