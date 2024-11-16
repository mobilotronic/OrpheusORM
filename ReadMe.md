# Orpheus
Orpheus is a lightweight flexible ORM, that gives the developer real flexibility, on how to create schemas, load/save data configure complex constraints and relationships between models.

[![CI Build](https://github.com/mobilotronic/OrpheusORM/actions/workflows/buildWorkFlow.yml/badge.svg)](https://github.com/mobilotronic/OrpheusORM/actions/workflows/buildWorkFlow.yml)
## Overview

### Schema creation
OrpheusORM has a built-in schema engine, which you can ,optionally, use to create and/or update your schema, based on your model classes.

### Model binding
By default Orpheus assumes that your table names will match your model class names. 

But you can override this assumption, by decorating your model classes with the ```TableName``` attribute and essentially map your model to the database table.

### Nested data
Using an OrpheusModule you can save nested data (master-detail-subdetail) with just one Save. 

All master-detail relationships and keys will be updated automatically.

## Documentation
To get started go to Orpheus's [documentation](https://mobilotronic.github.io/OrpheusORM/), where besides a fully documented API, you can also find examples on how to use Orpheus.

## Nuget packages
There are three available nuget packages.
* [OrpheusORM](https://www.nuget.org/packages/OrpheusORM/)
* [Orpheus SQL Server DDL Helper](https://www.nuget.org/packages/OrpheusORMSQLServerDDLHelper/)
* [Orpheus MySQL Server DDL Helper](https://www.nuget.org/packages/OrpheusORMMySQLServerDDLHelper/)

As mentioned in the documentation, Orpheus is database engine agnostic, but between different database engines there are differences.

That's the role of the DDL helpers, to reconcile the differences between the underlying database engines.
Read more about DDL helpers [here](https://mobilotronic.github.io/OrpheusORM/documentation/orpheus_ddl_helper.html)

There are already two DDL helpers available for SQL and MySQL, but you can implement your own and use it.
**Note:** MySQL DDL helper has a dependency on [MySQL.Data](https://dev.mysql.com/downloads/) 
