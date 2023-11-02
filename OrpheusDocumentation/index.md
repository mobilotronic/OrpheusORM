# **Orpheus**.
The lightweight ORM.

Flexibility on creating schema, load/save data and configure complex constraints and relationships between models.
## Schema Creation:
OrpheusORM has a built-in schema engine, which you can ,optionally, use to create and/or update your schema, based on your model classes.

## Model Binding
By default Orpheus assumes that your table names will match your model class names. But you can override this assumption, by decorating your model classes with the [TableName] attribute and essentially map your model to the database table.

## Nested Data
Using an OrpheusModule you can save nested data (master-detail-subdetail) with just one Save. All master-detail relationships and keys will be updated automatically.