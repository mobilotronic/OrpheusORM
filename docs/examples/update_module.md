# Using an OrpheusModule

```javascript
var sqlServerConnectionString = @"Data Source=[yourserver];Initial Catalog=orpheusTestDB;Integrated Security=True";
var db = OrpheusIocContainer.Resolve<IOrpheusDatabase>();
//Connect to the database.
db.Connect(sqlServerConnectionString);

var module = OrpheusIocContainer.Resolve<IOrpheusModule>(new ResolverOverride[] {
    new ParameterOverride("database",db)
});

module.ReferenceTables.Add(this.Database.CreateTable<TestModelTransactor>("TestModelTransactor"));
module.ReferenceTables.Add(this.Database.CreateTable<TestModelItem>("TestModelItem"));


module.Tables.Add(this.Database.CreateTable<TestModelOrder>("TestModelOrder"));
var order = module.GetTable<TestModelOrder>("TestModelOrder");


var orderLineOptions = OrpheusIocContainer.Resolve<IOrpheusTableOptions>();
orderLineOptions.TableName = "TestModelOrderLine";
orderLineOptions.MasterTableKeyFields = new List<IOrpheusTableKeyField>();
orderLineOptions.Database = this.Database;


var orderMasterKeyField = OrpheusIocContainer.Resolve<IOrpheusTableKeyField>();
orderMasterKeyField.Name = "OrderId";
orderLineOptions.MasterTableKeyFields.Add(orderMasterKeyField);
orderLineOptions.MasterTableName = "TestModelOrder";
module.Tables.Add(this.Database.CreateTable<TestModelOrderLine>(orderLineOptions));
            

var transactors = module.GetReferenceTable<TestModelTransactor>("TestModelTransactor");
var items = module.GetReferenceTable<TestModelItem>("TestModelItem");

            
var orderLines = module.GetTable<TestModelOrderLine>("TestModelOrderLine");
orderLines.MasterTable = order;

//populating auxiliary data.
transactors.Add(GetTransactors());
items.Add(GetItems());
using(var tr = db.BeginTransaction())
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
//loading auxiliary data in memory.
transactors.Load();
items.Load();

order.Add(new TestModelOrder() {
    OrderId = Guid.NewGuid(),
    OrderDateTime = DateTime.Now,
    TransactorId = transactors.Data.First().TransactorId
});

orderLines.Add(new TestModelOrderLine() {
    ItemId = items.Data.First().ItemId,
    OrderLineId = Guid.NewGuid(),
    Price = 5,
    Quantity = 10,
    TotalPrice = 50
});

//saving the module will save both _Order_ and *OrderLine* tables
module.Save();

```