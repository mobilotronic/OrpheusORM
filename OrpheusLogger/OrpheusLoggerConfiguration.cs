using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrpheusLogger
{
    public class OrpheusLoggerConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("level", DefaultValue = "Error", IsDefaultCollection = false, IsKey = false, IsRequired = true)]
        public string Level
        {
            get { return (String)this["level"]; }
            set { this["level"] = value; }
        }

        [ConfigurationProperty("filePath", DefaultValue = null, IsDefaultCollection = false, IsKey = false, IsRequired = false)]
        public string FilePath
        {
            get { return (String)this["filePath"]; }
            set { this["filePath"] = value; }
        }

        [ConfigurationProperty("maxFileSize", DefaultValue = 10, IsDefaultCollection = false, IsKey = false, IsRequired = false)]
        public int MaxFileSize
        {
            get { return (int)this["maxFileSize"]; }
            set { this["maxFileSize"] = value; }
        }

        public override bool IsReadOnly()
        {
            return false;
        }

        public static string LoggingSectionName = "orpheusLogging";

        public static OrpheusLoggerConfiguration Configuration
        {
            get
            {
                return (OrpheusLoggerConfiguration)System.Configuration.ConfigurationManager.GetSection(LoggingSectionName);
            }
        }
    }
}
