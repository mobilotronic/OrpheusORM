# Orpheus DDL Helper
OrpheusORM is database engine type agnostic. This means that it does not include
any code targeting a specific database engine.

Despite the fact, that SQL based database engines use the SQL language, there are differences
between them.

Here is where the ```IOrpheusDDLHelper``` comes into play.

Every ```IOrpheusDatabase``` requires a ```IOrpheusDDLHelper```. Orpheus
provides out of the box, helpers for MS SQL and MySQL database engines.

### So what do these helpers do?
As mentioned above, they reconcile differences between the different database engines.

For example MS SQL natively supports the ```UID``` type while MySQL does not.
The helper will give this kind of information to the ```IOrpheusDatabase```.

You can implement your own ```IOrpheusDDLHelper``` and register it in OrpheusORM.


#### Implementing a custom ```IOrpheusDDLHelper```
Implementing your own DDLHelper is not a requirement.

However if Orpheus does not provide one for you, for your database engine, it's quite easy to implement your own.

All you have to do is to create a class that implements ```IOrpheusDDLHelper```,implement your logic and register it in Orpheus.
[Here](orpheus_and_di.md) is how you can register a custom DDL helper.