# Defining database engine

OrpheusORM uses [Unity](https://www.nuget.org/packages/Unity/) in order to have the same code base for all supported database engines.
To configure which database engine to use, you need only to update the unity configuration section with the appropriate types.

### SQL Server
```XML
  <unity xmlns="http://schemas.microsoft.com/practices/2010/unityx">
    <assembly name="System.Data,4.0.0.0 ,Culture=neutral,PublicKeyToken=b77a5c561934e089"/>
    
    <alias alias="IDbConnection" type="System.Data.IDbConnection, System.Data"/>
    <alias alias="SqlConnection" type="System.Data.SqlClient.SqlConnection, System.Data"/>

      <register type="IDbConnection" mapTo="SqlConnection">
        <constructor/>
      </register>
```
### MySQL
```XML
  <unity xmlns="http://schemas.microsoft.com/practices/2010/unityx">
    <assembly name="System.Data,4.0.0.0 ,Culture=neutral,PublicKeyToken=b77a5c561934e089"/>
    
    <alias alias="IDbConnection" type="System.Data.IDbConnection, System.Data"/>
    <alias alias="MySqlConnection" type="MySql.Data.MySqlClient.MySqlConnection, MySql.Data"/>

      <register type="IDbConnection" mapTo="MySqlConnection">
        <constructor/>
      </register>
```