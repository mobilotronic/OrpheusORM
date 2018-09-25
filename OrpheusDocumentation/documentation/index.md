# What is Orpheus
Orpheus is a hybrid ORM. Somewhere between a micro-orm like Dapper or PetaPoco and a full scale ORM like EntityFramework.
It supports decorated models and has a code-first schema generation engine, but that's optional. You don't have to use that, to use Orpheus.
You can drop it on a project and use it as you would any of the other micro-orms.

It's easy to use and very flexible on configuring, as all of its configuration is in a file.

Here is a quick example on how to create an entity.
```csharp
var usersTable = this.Database.CreateTable<TestModelUser>();
usersTable.Add(new TestModelUser()
{
    UserId = Guid.NewGuid(),
    UserName = "Admin",
    PasswordHash = "!@##%$#%$%#DFSDasdf43w3re",
    PasswordSalt = "$%TG*(sdfsfr687",
    Email = admin@test.com,
    Active = 1,
    UserProfileId = Guid.Parse("3C9EA0CB-885E-476F-A919-6E97484CE633"),
    UserGroupId = Guid.Parse("ABA227B9-1E82-4FFB-9A50-94AED2D41869")
});
usersTable.Save();
```
or deleting an entity
```csharp
var usersTable = this.Database.CreateTable<TestModelUser>();
usersTable.Delete(new TestModelUser()
{
    UserId = Guid.Parse("86AFD459-ABCB-4623-B375-AA82F8B36590"),
});
usersTable.Save();
```

# Orpheus Documentation
Here you can find information regarding the key/core class of OrpheusORM, understand their
purpose and how to use them.

* [Orpheus Table](orpheus_table.md)
* [Orpheus Module](orpheus_module.md)
* [Orpheus DI Configuration](orpheus_and_di.md)
* [Orpheus DDL Helper](orpheus_ddl_helper.md)