using System;
using System.Collections.Generic;
using System.Text;

namespace OrpheusTests
{
    public class TestDatabaseConnectionStrings
    {
        public static string SQLServer = @"Data Source=[your-server];Initial Catalog=orpheusTestDB;Integrated Security=True";
        public static string MySQL = @"Server=[your-server];Database=orpheusTestDB;Uid=[user-name];Pwd=[password]";
    }
}
