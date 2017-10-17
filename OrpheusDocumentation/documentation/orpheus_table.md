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
or create an instance of the table using the OrpheusDatabase
```csharp
IOrpheusDatabase db = OrpheusCore.ServiceProvider.Provider.Resolve<IOrpheusDatabase>();
var transactorsTable = db.CreateTable<TestModelTransactor>();
```
**Note: The database does not keep a reference for the created table.**

After you have a table instance, you can add, update and delete data from your table.
```csharp
IOrpheusDatabase db = OrpheusCore.ServiceProvider.Provider.Resolve<IOrpheusDatabase>();
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
