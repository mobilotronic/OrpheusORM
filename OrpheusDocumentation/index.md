# **Orpheus**.
The lightweight ORM.

Flexibility on creating schema, load/save data and configure complex constraints and relationships between models.
## Schema Creation:
OrpheusORM has a built-in schema engine, which you can ,optionally, use to create and/or update your schema, based on your model classes.

## Model Binding
By default Orpheus assumes that your table names will match your model class names. But you can override this assumption, by decorating your model classes with the [TableName] attribute and essentially map your model to the database table.

## Nested Data
Using an OrpheusModule you can save nested data (master-detail-subdetail) with just one Save. All master-detail relationships and keys will be updated automatically.

## Multiple Database Engines
Orpheus supports SQL Server, MySQL, and PostgreSQL out of the box, each via its own DDL helper
package. See [Orpheus DDL Helper](documentation/orpheus_ddl_helper.md).

## Async
Every I/O method has an async counterpart (`LoadAsync`/`SaveAsync`) backed by real async ADO.NET
I/O. See [Orpheus Table](documentation/orpheus_table.md).

## Batching
Save() chunks pending Add/Update/Delete operations into multi-row commands instead of one round
trip per row. See [Orpheus Table](documentation/orpheus_table.md).

## Connection Pooling
Connections are leased from the ADO.NET connection pool, with pool size and idle timeout
configurable per connection. See [Connecting to a database](documentation/orpheus_connecting_to_db.md).