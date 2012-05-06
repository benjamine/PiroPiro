using System;
using System.Data.SqlClient;
using PiroPiro.Config;

namespace PiroPiro.DbCleanup.SqlServer
{
    /// <summary>
    /// Performs a SQL RESTORE from a backup file
    /// </summary>
    public class SqlRestore : SqlServerStrategy
    {
        public string BackupFile
        {
            get { return Configuration.Get("PiroPiro.DbCleanup.BackupFile"); }
        }

        /// <summary>
        /// Restores a SQL database from a backup file.
        /// Warning: closes all existing connections
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="backUpFile">Backup File accesible from the SQL Server</param>
        /// <param name="serverName">SQL server instance</param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public static void Restore(string databaseName, string backUpFile, string serverName, string userName = null, string password = null)
        {
            try
            {
                Console.WriteLine("Restoring database " + databaseName + " at " + serverName + " ...");

                string connString = "Data Source=" + serverName + ";Initial Catalog=master;";

                if (string.IsNullOrEmpty(userName))
                {
                    connString += "Integrated Security=True;";
                }
                else
                {
                    connString += string.Format("User ID={0};Password={1};", userName, password);
                }

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    SqlCommand command = conn.CreateCommand();
                    command = conn.CreateCommand();
                    command.CommandTimeout = 3 * 60;
                    command.CommandType = System.Data.CommandType.Text;

                    // check if db exists
                    command.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = '" + databaseName + "'";

                    conn.Open();

                    if (command.ExecuteScalar() as int? == 1)
                    {
                        // db exists, set single user
                        command.CommandText = "ALTER DATABASE [" + databaseName + "] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE";
                        command.ExecuteNonQuery();
                    }

                    command.CommandText = "RESTORE DATABASE [" + databaseName + "] FROM  DISK = N'" +
                        backUpFile + "' WITH FILE = 1,  NOUNLOAD, REPLACE, STATS = 10";
                    command.ExecuteNonQuery();

                    conn.Close();
                }
                Console.WriteLine("Database restored successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database restore failed: " + ex.Message);
                throw;
            }
        }

        public SqlRestore(Configuration configuration = null)
            : base(configuration)
        {
        }

        protected override void DoExecute()
        {
            Restore(Database, BackupFile, Server);
        }

    }
}
