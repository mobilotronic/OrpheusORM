using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrpheusCore;
using OrpheusInterfaces;
using System.Reflection;
using System.IO;

namespace OrpheusTests
{
    internal class TestData
    {
        public static string[] UserNames = new string[] { "Admin","Root","User1","User2","User3" };
        public static string[] PasswordHashes = new string[] { "asd1df!@#", "1#$#*(*&", "!@#54$%", "hash", "%$^%(()123" };
        public static string[] PasswordSalts = new string[] { "fdsfa@#$)", "SAD#$FD$", "#$$%!%$)_+", "salt", "FDsfasdf*(*&$#%" };
        public static string[] Emails = new string[] { "admin@test.com", "sales@test.com", "info@test.com", "support@test.com","webadmin@test.com" };
    }

    public enum DbEngine
    {
        dbSQLServer,
        dbMySQL
    }

    public class TestDatabase
    {
        //public static string ConnectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\orpheusTestDB.mdf;Initial Catalog=orpheusTestDB;Integrated Security=True";
        private static string sqlServerConnectionString = @"Data Source=[your-server];Initial Catalog=orpheusTestDB;Integrated Security=True";
        public static string mySQLConnectionString = @"Server=[your-server];Database=orpheusTestDB;Uid=[user-name];Pwd=[password]";
        private static IOrpheusDatabase db;

        private static string assemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static string ConnectionString
        {
            get
            {
                switch (TestDatabase.DatabaseEngine)
                {
                    case DbEngine.dbSQLServer:
                        {
                            return sqlServerConnectionString;
                        }
                    case DbEngine.dbMySQL:
                        {
                            return mySQLConnectionString;
                        }
                    default: return null;
                }
            }
        }

        public static IOrpheusDatabase DB
        {
            get
            {
                if(TestDatabase.db == null)
                {
                    switch (TestDatabase.DatabaseEngine)
                    {
                        case DbEngine.dbSQLServer:
                            {
                                OrpheusCore.Orpheus.InitializeConfiguration(assemblyDirectory + @"\" + "OrpheusSQLServer.config");
                                break;
                            }
                        case DbEngine.dbMySQL:
                            {
                                OrpheusCore.Orpheus.InitializeConfiguration(assemblyDirectory + @"\" + "OrpheusMySQL.config");
                                break;
                            }
                    }
                    TestDatabase.db = OrpheusIocContainer.Resolve<IOrpheusDatabase>();
                }
                return TestDatabase.db;
            }
        }

        public static DbEngine DatabaseEngine { get; set; }

        public static List<TestModelUser> GetRandomUsersForTesting(int count = 1000)
        {
            var result = new List<TestModelUser>();
            Random rnd = new Random();
            for(var i = 0; i <= count - 1; i++)
            {
                var rndIdx = rnd.Next(0, 5);
                result.Add(new TestModelUser()
                {
                    UserId = Guid.NewGuid(),
                    UserName = TestData.UserNames[rndIdx] + (i.ToString()),
                    PasswordHash = TestData.PasswordHashes[rndIdx],
                    PasswordSalt = TestData.PasswordSalts[rndIdx],
                    Email = TestData.Emails[rndIdx],
                    Active = 1,
                    UserProfileId = TestSchemaConstants.AdminUserProfileId,
                    UserGroupId = TestSchemaConstants.AdminUserGroupId
                });
            }
            return result;
        }

        public static List<TestModelTransactor> GetTransactors(int count = 1000,TestModelTransactorType transactorType = TestModelTransactorType.ttCustomer)
        {
            var result = new List<TestModelTransactor>();
            for (var i = 0; i <= count - 1; i++)
            {
                result.Add(
                        new TestModelTransactor()
                        {
                            TransactorId = Guid.NewGuid(),
                            Code = "TR" + i.ToString(),
                            Description = "Transactor" + i.ToString(),
                            Address = "Address" + i.ToString(),
                            Email = "Email" + i.ToString(),
                            Type= transactorType
                        }
                    );
            }
            return result;
        }

        public static List<TestModelItem> GetItems(int count = 1000, TestModelTransactorType transactorType = TestModelTransactorType.ttCustomer)
        {
            var result = new List<TestModelItem>();
            for (var i = 0; i <= count - 1; i++)
            {
                result.Add(
                        new TestModelItem()
                        {
                            ItemId = Guid.NewGuid(),
                            Code = "TR" + i.ToString(),
                            Description = "Transactor" + i.ToString(),
                            Price = i
                        }
                    );
            }
            return result;
        }
    }
}
