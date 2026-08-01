# Orpheus Table
Orpheus table is the core class of OrpheusORM.
It is responsible for the actual executing of the
* Delete
* Update
* Insert

commands to modify data.
It's also responsible for loading data, with or without criteria. 
So you can load all the data of the underlying database table or a subset of it.

It is model agnostic and you can declaratively define the model for the table. The model for the table
is/should be basically a representation of the database table fields.

## When to use it
There is no limitation per se, for when to use the OrpheusTable class. 
From a logical separation perspective, it would make more sense, if you were saving data to 
a table that has no detail tables. [Orpheus Module](orpheus_module.md) is the class to use,
when you have multiple tables, with dependencies to each other.

## A quick example
Let's assume you have the following model
```csharp
    public enum TestModelTransactorType
    {
        ttCustomer,
        ttSupplier
    }
    public class TestModelTransactor
    {
        [PrimaryKey]
        public Guid TransactorId { get; set; }

        [Length(30)]
        public string Code { get; set; }

        [Length(120)]
        public string Description { get; set; }

        [Length(120)]
        public string Address { get; set; }

        [Length(250)]
        public string Email { get; set; }

        public TestModelTransactorType Type { get; set; }
    }
```
You can declare the table in your code
```csharp
public class TransactorsTable:OrpheusTable<TestModelTransactor>
{
}
var transactorsTable = new TransactorsTable();
```
or create an instance of the table using the OrpheusDatabase (see [DI Configuration](orpheus_and_di.md)
for how to get an `IOrpheusDatabase` instance)
```csharp
var transactorsTable = db.CreateTable<TestModelTransactor>();
```
**Note: The database does not keep a reference for the created table.**

After you have a table instance, you can add, update and delete data from your table.
```csharp
var transactorsTable = db.CreateTable<TestModelTransactor>();

var transactor = new TestModelTransactor(){
TransactorId = Guid.NewGuid(),
Code = '001',
Description = 'Transactor1'
};
transactorsTable.Add(transactor);
transactorsTable.Save();
```
**Note:The table save will be executed within a transaction, so in case of any error, changes will be rolled back.**

## Async
Every I/O method on `IOrpheusTable<T>` has an async counterpart — `LoadAsync`/`SaveAsync` — backed
by real `DbConnection`/`DbCommand`/`DbDataReader` async I/O, not a synchronous call wrapped in
`Task.Run`. The sync methods remain fully supported; prefer the async ones for scalability in
server-side code.
```csharp
await transactorsTable.LoadAsync();

transactorsTable.Add(transactor);
await transactorsTable.SaveAsync(cancellationToken);
```
`LoadAsync` has the same overloads as `Load`, including loading from a raw SQL string or an
`IDbCommand`:
```csharp
await transactorsTable.LoadAsync("SELECT * FROM TestModelTransactor WHERE Type = 0");
```

## Batching
When you `Save()` (or `SaveAsync()`) a table with many pending Add/Update/Delete operations,
Orpheus doesn't execute one round trip per row. Instead it chunks the pending rows into batches
and sends one multi-row command per batch:
* Inserts (for tables without a DB-generated key) use a multi-row `INSERT ... VALUES (...),(...),...`.
* Deletes use a single `WHERE key IN (...)` / OR-composite command.
* Updates send multiple parameterized `UPDATE` statements in one batch (the same technique EF Core
  uses internally).
* Tables with a DB-generated key are also batched, using an engine-specific technique to retrieve
  the generated keys for the whole batch in one round trip (see [Orpheus DDL Helper](orpheus_ddl_helper.md)).

The batch size defaults to 100 rows and can be changed per table:
```csharp
transactorsTable.BatchSize = 250;
```
