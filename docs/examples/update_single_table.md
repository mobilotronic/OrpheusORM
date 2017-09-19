Using a single OrpheusTable
=

```javascript
var sqlServerConnectionString = @"Data Source=[yourserver];Initial Catalog=orpheusTestDB;Integrated Security=True";
var db = OrpheusIocContainer.Resolve<IOrpheusDatabase>();
//Connect to the database.
db.Connect(sqlServerConnectionString);

//create a table options object and create the orpheus table.
var tableOptions = OrpheusIocContainer.Resolve<IOrpheusTableOptions>();
tableOptions.TableName = "TestModelUser";
tableOptions.KeyFields = new List<IOrpheusTableKeyField>();
var usersTable = this.Database.CreateTable<TestModelUser>(tableOptions);

//add one or more TestModelUser records
usersTable.Add(TestDatabase.GetRandomUsersForTesting(recordCount));

IDbTransaction trans = this.Database.BeginTransaction();
try
{
    usersTable.ExecuteInserts(trans);
    trans.Commit();
}
catch(Exception e)
{
    trans.Rollback();
    throw e;
}

```