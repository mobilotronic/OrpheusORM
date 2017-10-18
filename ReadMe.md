# OrpheusORM
OrpheusORM is a lightweight flexible ORM, that gives the developer real flexibility, on how to create schemas, load/save data configure complex constraints and relationships between models.

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
To get started go to Orpheus's [documentation](https://redacted-user.github.io/OrpheusORM/), where besides a fully documented API, you can also find examples on how to use Orpheus.
