using System;
using System.Data.SqlClient;
using System.IO;
using System.Security.AccessControl;
using PiroPiro.Config;

namespace PiroPiro.DbCleanup.SqlServer
{
    /// <summary>
    /// Base class for strategies that clean up a SQL database
    /// </summary>
    public abstract class SqlServerStrategy : DbCleanupStrategy
    {
        public string Database
        {
            get { return Configuration.Get("PiroPiro.DbCleanup.Database"); }
        }

        public string Server
        {
            get { return Configuration.Get("PiroPiro.DbCleanup.Server"); }
        }

        public string TouchFile
        {
            get { return Configuration.Get("PiroPiro.DbCleanup.TouchFile", false); }
        }

        private static void GetDatabaseFiles(string databaseName, string serverName, out string mdfFile, out string logFile, string userName = null, string password = null)
        {
            try
            {
                Console.WriteLine("Locating mdf file for database " + databaseName + " at " + serverName + " ...");

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
                    command.CommandTimeout = 60;
                    command.CommandType = System.Data.CommandType.Text;

                    // GET DATA FILE
                    command.CommandText = "SELECT [filename] from [" + databaseName + "].sys.[sysfiles] WHERE [filename] like '%.mdf'";

                    conn.Open();

                    mdfFile = (string)command.ExecuteScalar();

                    // GET LOG FILE
                    command.CommandText = "SELECT [filename] from [" + databaseName + "].sys.[sysfiles] WHERE [filename] like '%.ldf'";
                    logFile = (string)command.ExecuteScalar();

                    conn.Close();
                }
                Console.WriteLine("mdf file located successfully");

            }
            catch (Exception ex)
            {
                Console.WriteLine("error locating Database mdf file: " + ex.Message);
                throw;
            }

        }

        private static void Detach(string databaseName, string serverName, string userName = null, string password = null)
        {
            try
            {
                Console.WriteLine("Detaching database " + databaseName + " at " + serverName + " ...");

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
                    command.CommandTimeout = 60;
                    command.CommandType = System.Data.CommandType.Text;

                    // SET SINGLE USE (drop connections)
                    command.CommandText = "ALTER DATABASE [" + databaseName + "] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE";

                    conn.Open();

                    command.ExecuteNonQuery();

                    // DETACH
                    command.CommandText = "EXEC master.dbo.sp_detach_db @dbname = N'" + databaseName + "', @keepfulltextindexfile=N'true'";

                    command.ExecuteNonQuery();

                    conn.Close();
                }
                Console.WriteLine("Database detached successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database detach failed: " + ex.Message);
                throw;
            }
        }

        private static string LocalPathToUnc(string serverName, string fullpath)
        {
            string machine = serverName.Split('\\')[0];
            if (string.IsNullOrEmpty(machine) || machine == "." || machine == "127.0.0.1" || machine.Equals("localhost", StringComparison.InvariantCultureIgnoreCase))
            {
                return fullpath;
            }
            DriveInfo drive = new DriveInfo(fullpath);
            string driveLetter = fullpath.Substring(0, 1);
            return string.Format(@"\\{0}\{1}$\{2}", machine, driveLetter,
                fullpath.Substring(drive.RootDirectory.FullName.Length));
        }

        private static void GrantFullControl(string filename, string user)
        {
            var accessRules = File.GetAccessControl(filename);
            accessRules.AddAccessRule(new FileSystemAccessRule(user,
                 FileSystemRights.FullControl,
                 AccessControlType.Allow));
            File.SetAccessControl(filename, accessRules);
        }

        private static void Attach(string databaseName, string serverName, string userName, string password, string mdfFile)
        {
            try
            {
                Console.WriteLine("Attaching database " + databaseName + " at " + serverName + " ...");

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
                    command.CommandTimeout = 60;
                    command.CommandType = System.Data.CommandType.Text;

                    // ATTACH
                    command.CommandText = "CREATE DATABASE [" + databaseName + "] ON (FILENAME=N'" + mdfFile + "') FOR ATTACH";

                    conn.Open();

                    command.ExecuteNonQuery();

                    conn.Close();
                }
                Console.WriteLine("Database attached successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database attach failed: " + ex.Message);
                throw;
            }
        }

        public static bool DbExists(string databaseName, string serverName, string userName = null, string password = null)
        {

            bool exists = false;
            try
            {
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
                    command.CommandTimeout = 60;
                    command.CommandType = System.Data.CommandType.Text;

                    command.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = '" + databaseName + "'";

                    conn.Open();

                    exists = command.ExecuteScalar() as int? == 1;

                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error finding " + databaseName + " at " + serverName + ": " + ex.Message);
                throw;
            }

            return exists;
        }

        public SqlServerStrategy(Configuration configuration = null)
            : base(configuration)
        {
            Executed += SqlServerStrategy_Executed;
        }

        void SqlServerStrategy_Executed(object sender, EventArgs e)
        {
            // touch a file, eg: restart app by touching web.config
            // this is useful after a sql restore or attach, as all connection from app pools get closed
            string touchFile = TouchFile;
            if (!string.IsNullOrEmpty(touchFile))
            {
                File.SetLastWriteTimeUtc(touchFile, DateTime.UtcNow);
                Console.WriteLine("Touched file : " + touchFile);
            }
        }

    }
}
