mkdir lib\net46
@echo Nuget folder created.
xcopy ..\bin\Release\OrpheusAttributes.dll lib\net46 /Y /Q
xcopy ..\bin\Release\OrpheusCore.dll lib\net46 /Y /Q
xcopy ..\bin\Release\OrpheusInterfaces.dll lib\net46 /Y /Q
xcopy ..\..\OrpheusTests\bin\Release\OrpheusLogger.dll lib\net46 /Y /Q
xcopy ..\..\OrpheusTests\bin\Release\OrpheusMySQLDDLHelper.dll lib\net46 /Y /Q
xcopy ..\..\OrpheusTests\bin\Release\OrpheusSQLServerDDLHelper.dll lib\net46 /Y /Q
@echo assemblies copied.
nuget.exe pack OrpheusORM.nuspec
@echo new nupkg file created.
pause