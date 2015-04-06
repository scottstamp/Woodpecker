using System;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Data;
using System.Collections.Generic;

using Woodpecker.Storage;

namespace Woodpecker.Core
{
    /// <summary>
    /// This static class keeps values of various configuration settings.
    /// </summary>
    public static class Configuration
    {
        #region Fields
        /// <summary>
        /// Keeps configuration keys and values.
        /// </summary>
        private static Dictionary<string, string> configurationValues = new Dictionary<string, string>();
        public static Encoding charTable;
        #region Menu configuration
        public static bool blacklistBastards;
        #endregion

        #region DLL Imports
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Reads the credentials for the database from db.config and creates the connection string.
        /// </summary>
        public static void createConnectionString()
        {
            string dbHost = readConfigurationValueFromFile("host");
            string dbName = readConfigurationValueFromFile("name");
            string dbUsername = readConfigurationValueFromFile("username");
            string dbPassword = readConfigurationValueFromFile("password");

            int dbMinPoolSize = 0;
            int dbMaxPoolSize = 0;
            if(!int.TryParse(Configuration.readConfigurationValueFromFile("minpoolsize"),out dbMinPoolSize))
                dbMinPoolSize = 25;
            if (!int.TryParse(Configuration.readConfigurationValueFromFile("maxpoolsize"), out dbMaxPoolSize))
                dbMaxPoolSize = 500;

            Database.connectionString =
                "Server=" + dbHost + ";" +
                "Database=" + dbName + ";" +
                "Uid=" + dbUsername + ";" +
                "Pwd=" + dbPassword + ";" +
                "Pooling=true;" +
                "Min pool size=" + dbMinPoolSize + ";" +
                "Max pool size=" + dbMaxPoolSize + ";";

            Logging.Log("Loaded database credentials and created connection string.");
        }
        /// <summary>
        /// Initializes the configuration from the 'configuration' table.
        /// </summary>
        public static void loadConfiguration()
        {
            Logging.Log("Loading configuration from `configuration` table...");
            charTable = System.Text.Encoding.GetEncoding("iso-8859-1");

            Database Database = new Database(true, true);
            if (Database.Ready)
            {
                DataTable Table = Database.getTable("SELECT configkey,configvalue FROM configuration");
                foreach (DataRow Row in Table.Rows)
                {
                    string Key = (string)Row["configkey"];
                    if (!configurationValues.ContainsKey(Key))
                    {
                        string Value = (string)Row["configvalue"];
                        configurationValues.Add(Key, Value);
                        //Logging.Log("   " + Key + " = \"" + Value + "\"");
                    }
                }

                Game.Items.carryItemHelper.setDefaultHandItemTypes();
                Logging.Log("Loaded configuration (" + Table.Rows.Count + " entries)");
            }
            else
            {
                Logging.Log("Failed to load configuration, because the database wasn't contactable!", Logging.logType.commonWarning);
                Engine.Program.Stop("failed to load configuration");
            }      
        }
        /// <summary>
        /// Sets configuation variable, then adds updates the local cache to reflect such changes.
        /// </summary>
        /// <param name="Key">The key of the configuration entry.</param>
        /// <param name="Value">The value of the configuration entry.</param>
        public static void setConfigurationValue(string Key, string Value)
        {
            // Do we really need to sanitize Value? only the emu has access to it...
            Logging.Log("Assigning value '" + Value + "' to key '" + Key + "' in `configuration` table...");
            charTable = System.Text.Encoding.GetEncoding("iso-8859-1");

            Database Database = new Database(true, true);
            if (Database.Ready)
            {
                Database.runQuery("UPDATE `configuration` SET `configvalue`='" + Value + "' WHERE (`configkey`='" + Key + "')");
                configurationValues[Key] = Value;

                Logging.Log("Configuration value for " + Key + " has been successfully updated to reflect '" + configurationValues[Key] + "'.");
            }
            else
            {
                Logging.Log("Failed to update configuration, because the database wasn't contactable!", Logging.logType.commonWarning);
            }      

        }
        /// <summary>
        /// Checks if there is a value for a certain configuration key. If so, then the value is returned. Otherwise, the key is returned.
        /// </summary>
        /// <param name="Key">The key of the configuration entry.</param>
        public static string getConfigurationValue(string Key)
        {
            if (configurationValues.ContainsKey(Key))
                return configurationValues[Key];
            else
                return Key;
        }
        /// <summary>
        /// Checks if there is a value for a certain configuration key. If so, then the value is parsed to an integer and returned. Otherwise, zero (0) is returned.
        /// </summary>
        /// <param name="Key">The key of the configuration entry.</param>
        public static int getNumericConfigurationValue(string Key)
        {
            try { return int.Parse(configurationValues[Key]); }
            catch { return 0; }
        }
        /// <summary>
        /// True if the configuration file 'db.config' is found in the same directory as the executable.
        /// </summary>
        public static bool configFileExists
        {
            get { return File.Exists(Directory.GetCurrentDirectory() + "\\db.config"); }
        }
        public static string readConfigurationValueFromFile(string Key)
        {
            StringBuilder sb = new StringBuilder(255);
            try { int i = GetPrivateProfileString("db", Key, "", sb, 255, Directory.GetCurrentDirectory() + "\\db.config"); }
            catch { }
            return sb.ToString();
        }
        #endregion
    }
}
