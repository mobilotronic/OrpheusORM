# Creating your schema
Orpheus has a built-in schema generator engine, that you can use to create the database schema for your application.

It's not mandatory to use it, in order to use Orpheus, but if you are starting a new project, you might find it useful.

### Schema Creation Options
In the ORM frameworks world, there are a few different options. Here are a couple of the main ones.

* Database First
  * Where the ORM reads your database and generates the models.
* Code First
  * Where you write your models and the ORM creates the schema for you.

Orpheus supports the second option, where you write up your models and decorate them accordingly based on your needs.

### Decorating your classes with schema attributes
There are plenty of attributes that you can use to decorate your classes and/or properties. Have a look at [Orpheus Attributes](../api/OrpheusAttributes.yml)

But let's highlight some common usage scenarios.

#### Foreign Key
```csharp
/// <summary>
/// Foreign key attribute constructor.
/// </summary>
/// <param name="referenceTable">The referenced table name</param>
/// <param name="referenceField">The referenced field name</param>
/// <param name="onDeleteCascade">Delete cascade flag</param>
/// <param name="onUpdateCascade">Update cascade flag</param>
public ForeignKey(string referenceTable, string referenceField,bool onDeleteCascade = false, bool onUpdateCascade = false)
```
Here is an invoice model, that depends on multiple different models. Shipping type, payment method etc.
```csharp
/// <summary>
/// A class that represents an "Invoice" entity.
/// </summary>
public class Invoice
{
    /// <summary>
    /// If the invoice created is a transformation of another invoice.
    /// This will help traceability.
    /// </summary>
    [ForeignKey("Invoice","Id")]
    public Guid? OriginalInvoice { get; set; }

    /// <summary>
    /// Invoice type id.
    /// </summary>
    [ForeignKey("InvoiceType","Id")]        
    public Guid InvoiceTypeId { get; set; }

    /// <summary>
    /// Invoice's transactor.
    /// </summary>
    [ForeignKey("Transactor","Id")]
    public Guid TransactorId { get; set; }

    /// <summary>
    /// Fiscal year where the invoice belongs to.
    /// </summary>
    [ForeignKey("FiscalYear","Id")]
    public Guid FiscalYearId { get; set; }

    /// <summary>
    /// Invoice's shipping method.
    /// </summary>
    [ForeignKey("ShippingMethod","Id")]
    public Guid ShippingMethodId { get; set; }

    /// <summary>
    /// Invoice's payment method.
    /// </summary>
    [ForeignKey("PaymentMethod","Id")]
    public Guid PaymentMethodId { get; set; }

    /// <summary>
    /// Invoice's date and time.
    /// </summary>
    public DateTime InvoiceDate { get; set; }

    ....
```

#### Composite Primary/Unique Key
A composite primary/unique key is a key that is comprised from more than one fields.
```csharp
/// <summary>
/// Unique composite key attribute, to decorate models that have primary or unique keys that are comprised from than one field.
/// </summary>
public class UniqueCompositeKey : OrpheusCompositeKeyBaseAttribute
{
    /// <summary>
    /// Primary composite key.
    /// </summary>
    /// <param name="fields">Fields that are part of the key</param>
    /// <param name="sort">Sort direction</param>
    public UniqueCompositeKey(string[] fields,string sort = null) : base(fields) { }
}
```
In the following example, an attribute can only be associated with only one attribute group.
```csharp
/// <summary>
/// Class that represents an attribute. An attribute can be associated with only one attribute group.
/// </summary>
[UniqueCompositeKey(new string[] {"Id", "AttributeGroupId" })]
public class Attribute 
{
    [ForeignKey("AttributeGroup","Id")]
    public Guid AttributeGroupId { get; set; }
}
```

### Creating an Orpheus Schema
You can use ```IOrpheusDatabase``` to create an ```ISchema``` object.
```csharp
/// <summary>
/// Creates a schema object and sets it's database.
/// </summary>
/// <param name="id">Schema id</param>
/// <param name="description">Schema description</param>
/// <param name="version">Schema version</param>
/// <returns>An ISchema instance</returns>
ISchema CreateSchema(Guid id, string description, double version);
```

```ISchema``` is the object were you have to register your models, that will eventually be your database schema.
There are a couple of different ways to register a model into a schema, but the most straight forward one, is to
register it via its type.
```csharp
/// <summary>
/// Creates a schema table and initializes table-name, dependencies and generating fields from a model, if provided.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="dependencies"></param>
/// <returns></returns>
ISchemaTable AddSchemaTable<T>(List<ISchemaObject> dependencies = null) where T : class;
```
Here is an example
```csharp
Schema.AddSchemaTable<MercuryTransactor>();
```
There is built in support for dependencies between your models. So if your model has an "Id" reference to another, this
will be translated into a foreign key constraint. In order for the Orpheus schema to be aware of that dependency you
have to register it.

So if your invoice table depends on the invoice type table, the code would like this.
```csharp
Schema.AddSchemaTable<InvoiceType>();

var invoice = this.schema.AddSchemaTable<Invoice>();
invoice.AddDependency<MercuryInvoiceType>();

```

After you have registered all your models, you need only to execute the schema.
```csharp
Schema.Execute();
```
This will iterate through the registered models, resolve the dependencies and create the database schema.

### Updating an existing schema
Updating a schema, particularly a complex one, is always tricky. Orpheus's schema builder provides
build-in support, for reconciling differences between your models and the corresponding tables.

The preferred option would be, when you have a schema change, to instantiate a new ```ISchema``` and register
only the models that have changes. This is definitely the most performant way to update your schema, using Orpheus, since only
the models with the changes will be executed.

An added benefit, is that you can keep a historic record of your schema changes.

Alternatively you can always use the same ```ISchema``` object and just change your models
that are already registered and Orpheus will reconcile field and constraint differences.