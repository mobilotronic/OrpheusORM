using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrpheusCore;
using OrpheusInterfaces.Core;
using OrpheusTestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrpheusTests
{
    /// <summary>
    /// Covers the 2.1.0 work that removed Orpheus's static service locator and configuration
    /// holder, and made key generation pluggable. Each of these reproduces something an application
    /// hit before the fix: the schema builder and module system demanding a globally-assigned
    /// service provider, schema generation demanding an initialised static configuration, and
    /// read-only projections being rejected for having no primary key.
    /// </summary>
    public class DependencyInjectionTests : BaseTestClass
    {
        /// <summary>
        /// The schema engine and the module system must work with nothing but the database's own
        /// service provider — no global state anywhere. Before 2.1.0 both paths reached for a static
        /// service locator and threw ArgumentNullException("provider") if an application had not
        /// assigned one; that locator no longer exists, and this covers the paths that used it.
        /// </summary>
        protected void TestSchemaAndModuleResolveFromTheDatabase()
        {
            this.Initialize();

            Assert.IsNotNull(this.Database.ServiceProvider,
                "Expected the database to carry the provider its collaborators resolve from.");

            // Schema creation: previously resolved ISchemaTable and its loggers statically.
            this.ReCreateSchema();

            // Module creation and save: previously resolved ILogger<IOrpheusTable<T>> statically.
            var module = this.Database.CreateModule();
            var transactors = this.Database.CreateTable<TestModelTransactor>();
            module.MainTable = transactors;

            var id = Guid.NewGuid();
            transactors.Add(new TestModelTransactor
            {
                TransactorId = id,
                Code = "NOSTATIC",
                Description = "Saved with no global state",
                Address = "Addr",
                Email = "nostatic@test.com",
                Type = TestModelTransactorType.ttCustomer
            });
            module.Save();

            module.ClearData();
            module.Load([id]);

            Assert.AreEqual(1, module.GetTable<TestModelTransactor>(0).Data.Count,
                "Expected the module to round-trip a record resolving only from the database.");

            this.DisconnectDatabase();
        }

        /// <summary>
        /// A model with an unsized string property must still generate DDL. Before 2.1.0 the schema
        /// builder read DefaultStringSize from a static holder and threw
        /// ArgumentNullException("configuration") when it had not been initialised.
        /// </summary>
        protected void TestUnsizedStringModelGeneratesSchema()
        {
            this.Initialize();
            this.ReCreateSchema();

            var schema = this.Database.CreateSchema(Guid.NewGuid(), "Unsized string schema", 1.0);
            var table = schema.AddSchemaTable<TestModelUnsizedString>();

            var descriptionField = table.Fields.FirstOrDefault(f => f.Name == nameof(TestModelUnsizedString.Description));
            Assert.IsNotNull(descriptionField, "Expected a schema field for the unsized string property.");
            Assert.IsFalse(string.IsNullOrEmpty(descriptionField.Size),
                "Expected the unsized string column to receive the configured default size.");

            this.DisconnectDatabase();
        }

        /// <summary>
        /// A model used purely to receive query results has no key and needs none.
        /// </summary>
        protected async Task TestKeylessProjectionLoads()
        {
            this.Initialize();
            this.ReCreateSchema();

            var transactors = this.Database.CreateTable<TestModelTransactor>();
            transactors.Add(new TestModelTransactor
            {
                TransactorId = Guid.NewGuid(),
                Code = "PROJ",
                Description = "Projection source",
                Address = "Addr",
                Email = "proj@test.com",
                Type = TestModelTransactorType.ttCustomer
            });
            await transactors.SaveAsync();

            var code = this.Database.DDLHelper.SafeFormatField("Code");
            var description = this.Database.DDLHelper.SafeFormatField("Description");
            var rowCount = this.Database.DDLHelper.SafeFormatField("RowCount");

            var results = await this.Database.SQLAsync<TestModelProjection>(
                $"SELECT {code}, {description}, COUNT(*) AS {rowCount} FROM TestModelTransactor " +
                $"WHERE {code} = 'PROJ' GROUP BY {code}, {description}");

            Assert.AreEqual(1, results.Count, "Expected the keyless projection to materialise one row.");
            Assert.AreEqual("PROJ", results.First().Code);

            this.DisconnectDatabase();
        }

        /// <summary>
        /// Loading a keyless model is fine; deleting one is not, and must say so clearly. The check
        /// moved from the constructor to the point of use, so this asserts it still fires.
        /// </summary>
        protected void TestKeylessModelCannotBeMutated()
        {
            this.Initialize();

            var table = this.Database.CreateTable<TestModelProjection>("TestModelTransactor");
            table.Delete(new TestModelProjection { Code = "X", Description = "Y" });

            var exception = Assert.ThrowsExactly<InvalidOperationException>(
                () => table.Save(),
                "Expected deleting a keyless model to be rejected.");
            StringAssert.Contains(exception.Message, nameof(TestModelProjection),
                "Expected the error to name the offending model.");

            this.DisconnectDatabase();
        }

        /// <summary>
        /// Auto-generated Guid keys must be UUIDv7 and must sort in creation order by their
        /// big-endian bytes, which is the representation PostgreSQL's uuid and MySQL's storage
        /// compare on.
        /// </summary>
        protected async Task TestAutoGeneratedKeysAreTimeOrdered()
        {
            this.Initialize();
            this.ReCreateSchema();

            var table = this.Database.CreateTable<TestModelTransactor>();
            var added = new List<TestModelTransactor>();
            for (var i = 0; i < 25; i++)
            {
                // TransactorId left at Guid.Empty so Orpheus generates it.
                var record = new TestModelTransactor
                {
                    Code = $"SEQ{i}",
                    Description = $"Sequential {i}",
                    Address = "Addr",
                    Email = "seq@test.com",
                    Type = TestModelTransactorType.ttCustomer
                };
                table.Add(record);
                added.Add(record);
                await Task.Delay(2);
            }
            await table.SaveAsync();

            foreach (var record in added)
            {
                Assert.AreNotEqual(Guid.Empty, record.TransactorId, "Expected Orpheus to generate the key.");
                var version = (record.TransactorId.ToByteArray(bigEndian: true)[6] & 0xF0) >> 4;
                Assert.AreEqual(7, version, $"Expected a UUIDv7 key, got version {version}.");
            }

            var generated = added.Select(r => r.TransactorId).ToList();
            var sorted = generated
                .OrderBy(g => Convert.ToHexString(g.ToByteArray(bigEndian: true)), StringComparer.Ordinal)
                .ToList();
            CollectionAssert.AreEqual(generated, sorted,
                "Expected generated keys to already be in ascending big-endian byte order.");

            this.DisconnectDatabase();
        }

        /// <summary>
        /// The generator is replaceable, and swapping it changes what gets stored.
        /// </summary>
        protected void TestKeyGeneratorIsPluggable()
        {
            this.Initialize();

            Assert.IsNotNull(this.Database.KeyGenerator, "Expected a key generator to always be available.");
            Assert.AreEqual(7, (this.Database.KeyGenerator.NewKey().ToByteArray(bigEndian: true)[6] & 0xF0) >> 4,
                "Expected the default generator to produce UUIDv7 values.");

            IOrpheusKeyGenerator random = new RandomGuidKeyGenerator();
            Assert.AreEqual(4, (random.NewKey().ToByteArray(bigEndian: true)[6] & 0xF0) >> 4,
                "Expected RandomGuidKeyGenerator to produce version 4 values.");

            this.DisconnectDatabase();
        }
    }
}
