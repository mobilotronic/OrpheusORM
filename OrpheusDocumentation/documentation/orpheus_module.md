# Orpheus Module
OrpheusModule class represents a logical division and grouping of a set of tables.

For example you can have an `OrdersModule`, which will be comprised from many different tables.
Orders,Customers,OrderLines etc. 

When you Save from the module level, all pending records in tables that belong to the module
will be saved as well. 

All master-detail relationships and keys will be updated automatically.

## Whats does the above mean exactly?
You can use an OrpheusModule to logically group/divide your business logic.

Using the example above, having an `OrdersModule`, you can have your BL for adding, updating
and deleting orders in one place.

The OrpheusModule will manage for you, the master-detail relationships in your module.

For example an OrderLine must always have an OrderId. 
If you were to use separate tables
to achieve the same functionality, you would have to enter and synchronize the master-detail
key values manually.

## A quick example
Let's assume that you have the following model:
#### Transactor model (Customer, Supplier)
```csharp
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
#### Item model, the order item.
```csharp
    public class TestModelItem
    {
        [PrimaryKey(false)]
        public Guid ItemId { get; set; }

        [Length(30)]
        public string Code { get; set; }

        [Length(120)]
        public string Description { get; set; }

        [DefaultValue(0)]
        public double Price { get; set; }
    }
```
#### Order models.
```csharp
    public class TestModelOrder
    {
        [PrimaryKey]
        public Guid OrderId { get; set; }

        [ForeignKey("TestModelTransactor", "TransactorId")]
        public Guid TransactorId { get; set; }

        public DateTime OrderDateTime { get; set; }
    }

    public class TestModelOrderLine
    {
        [PrimaryKey]
        public Guid OrderLineId { get; set; }

        [ForeignKey("TestModelOrder","OrderId")]
        public Guid OrderId { get; set; }

        [ForeignKey("TestModelItem", "ItemId")]
        public Guid ItemId { get; set; }

        [DefaultValue(0)]
        public double Quantity { get; set; }

        public double Price { get; set; }

        public double TotalPrice { get; set; }
    }
```

#### Creating an OrpheusModule
There are a couple of ways to instantiate an OrpheusModule, the most straight forward
way is to use an ```IOrpheusModuleDefinition```.

##### Creating the definition object.
```csharp
IOrpheusDatabase db = OrpheusCore.ServiceProvider.Provider.Resolve<IOrpheusDatabase>();
var moduleDefinition = db.CreateModuleDefinition();
```
##### Configuring the definition object.
```csharp
moduleDefinition.MainTableOptions = moduleDefinition.CreateTableOptions("TestModelOrder",typeof(TestModelOrder));
moduleDefinition.ReferenceTableOptions.Add(moduleDefinition.CreateTableOptions("TestModelTransactor", typeof(TestModelTransactor)));
moduleDefinition.ReferenceTableOptions.Add(moduleDefinition.CreateTableOptions("TestModelItem", typeof(TestModelItem)));

var detailTableOptions = moduleDefinition.CreateTableOptions("TestModelOrderLine", typeof(TestModelOrderLine));
detailTableOptions.MasterTableName = "TestModelOrder";
detailTableOptions.AddMasterKeyField("OrderId");
moduleDefinition.DetailTableOptions.Add(detailTableOptions);
```
##### Creating the module.
```csharp
var module = this.Database.CreateModule(moduleDefinition);
```

##### Getting references to module tables.
```csharp
var transactors = module.GetReferenceTable<TestModelTransactor>("TestModelTransactor");
var items = module.GetReferenceTable<TestModelItem>("TestModelItem");
var orderLines = module.GetTable<TestModelOrderLine>("TestModelOrderLine");
var order = module.GetTable<TestModelOrder>("TestModelOrder");
```

##### Populating auxiliary data.
```csharp
//populating auxiliary data.
transactors.Add(TestDatabase.GetTransactors());
items.Add(TestDatabase.GetItems());
using (var tr = this.Database.BeginTransaction())
{
    transactors.ExecuteInserts(tr);
    items.ExecuteInserts(tr);
    try
    {
        tr.Commit();
    }
    catch
    {
        throw;
    }
}

transactors.Load();
items.Load();
```

##### Entering a new order.
```csharp
order.Add(new TestModelOrder()
{
    OrderId = Guid.NewGuid(),
    OrderDateTime = DateTime.Now,
    TransactorId = transactors.Data.First().TransactorId
});
//OrderId will be set automatically.
orderLines.Add(new TestModelOrderLine()
{
    ItemId = items.Data.First().ItemId,
    OrderLineId = Guid.NewGuid(),
    Price = 5,
    Quantity = 10,
    TotalPrice = 50
});

module.Save();
```
