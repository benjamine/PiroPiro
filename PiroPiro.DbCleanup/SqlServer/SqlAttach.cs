using System;
using System.Data.SqlClient;
using System.IO;
using System.Security.AccessControl;
using PiroPiro.Config;

namespace PiroPiro.DbCleanup.SqlServer
{
    /// <summary>
    /// Restores a SQL database by detaching, overwriting .mdf file and attaching again. (usually faster than a full SQL Restore)
    /// </summary>
    public class SqlAttach : SqlServerStrategy
    {
        public string BackupFile
        {
            get { return Configuration.Get("PiroPiro.DbCleanup.BackupFile"); }
        }

        public bool AsyncCopy
        {
            get { return Configuration.GetFlag("PiroPiro.DbCleanup.AsyncCopy", false) ?? true; }
        }

        public string SourceMdfFileSuffix
        {
            get
            {
                string suffix = (Configuration.Get("PiroPiro.DbCleanup.SourceMdfFileSuffix", false) ?? "").Trim();
                if (string.IsNullOrEmpty(suffix))
                {
                    return suffix;
                }
                else
                {
                    return ".bak";
                }
            }
        }

        /// <summary>
        /// Detachs a database, overwrites its .mdf file, and attachs it again (a faster replacement for a SQL Restore)
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="serverName"></param>
        /// <param name="useAsyncCopy"> </param>
        /// <param name="sourceMdfFileSuffix">a suffix to add to mdf target filename to obtain the source filename (by default .testData)</param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public static void DetachOverwriteAttach(string databaseName, string serverName, bool useAsyncCopy = true,
            string sourceMdfFileSuffix = ".testData", string userName = null, string password = null)
        {

            string mdfFile = null, logFile = null;

            if (DbExists(databaseName, serverName, userName, password))
            {
                GetDatabaseFiles(databaseName, serverName, out mdfFile, out logFile, userName, password);
                Detach(databaseName, serverName, userName, password);
            }
            else
            {
                Console.WriteLine("Database " + databaseName + " at " + serverName + " could not be found");
                throw new Exception("Database " + databaseName + " at " + serverName + " could not be found");
            }
            OverwriteMdfFile(serverName, mdfFile, logFile, useAsyncCopy, sourceMdfFileSuffix);
            Attach(databaseName, serverName, userName, password, mdfFile);
        }

        private static void GetDatabaseFiles(string databaseName, string serverName,
            out string mdfFile, out string logFile, string userName = null, string password = null)
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

        private static void OverwriteMdfFile(string serverName, string mdfFile, string logFile,
            bool useAsyncCopy = true, string sourceMdfFileSuffix = ".testData")
        {
            try
            {

                // get UNC path for mdf & log files

                string mdfFileUnc = LocalPathToUnc(serverName, mdfFile);
                string logFileUnc = LocalPathToUnc(serverName, logFile);
                try
                {
                    // try to gran full control on log file
                    GrantFullControl(logFileUnc, "Everyone");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot grant full control on log file: " + ex.Message);
                }
                // delete log file
                File.Delete(logFileUnc);

                // add permissions to mdf file to allow overwrite
                // this permissions will be overridden after attaching db
                GrantFullControl(mdfFileUnc, "Everyone");

                if (string.IsNullOrWhiteSpace(sourceMdfFileSuffix))
                {
                    sourceMdfFileSuffix = ".testData";
                }

                // overwrite mdf file
                if (File.Exists(mdfFileUnc + sourceMdfFileSuffix + ".copy"))
                {
                    Console.WriteLine("Using previously generated mdf copy...");
                    // replace using a previously generated copy
                    File.Delete(mdfFileUnc);
                    File.Move(mdfFileUnc + sourceMdfFileSuffix + ".copy", mdfFileUnc);
                    // add full permissions on mdf file
                    // this permissions are overriden on attach
                    GrantFullControl(mdfFileUnc, "Everyone");
                }
                else
                {
                    Console.WriteLine("Copying mdf file...");
                    // overwrite making a copy
                    File.Copy(mdfFileUnc + sourceMdfFileSuffix, mdfFileUnc, true);
                }

                Console.WriteLine("Database mdf file overwritten");

                // prepare a copy in the background, to accelerate next executions
                PrepareNextCopyAsync(LocalPathToUnc(serverName, mdfFile + sourceMdfFileSuffix));
            }
            catch (Exception ex)
            {
                Console.WriteLine("mdf overwrite failed, attach database manually before trying again: " + ex.Message);
                throw;
            }

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

        public static void PrepareNextCopyAsync(string filename)
        {
            Console.WriteLine("Preparing mdf copy for next executions...");
            new Action(() =>
            {
                try
                {
                    if (!File.Exists(filename + ".copy") && !File.Exists(filename + ".copyTemp"))
                    {
                        File.Copy(filename, filename + ".copyTemp");
                        File.Move(filename + ".copyTemp", filename + ".copy");
                        GrantFullControl(filename + ".copy", "Everyone");
                        Console.WriteLine("    >> Completed mdf copy for next executions");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error preparing mdf copy: " + ex.Message);
                    if (File.Exists(filename + ".copy"))
                    {
                        File.Delete(filename + ".copy");
                    }
                }
                finally
                {
                    // cleanup
                    if (File.Exists(filename + ".copyTemp"))
                    {
                        File.Delete(filename + ".copyTemp");
                    }
                }
            }).BeginInvoke(null, null);
        }

        public SqlAttach(Configuration configuration = null)
            : base(configuration)
        {
        }

        protected override void DoExecute()
        {
            // detach, overwrite db files, and attach again
            DetachOverwriteAttach(Database, Server, AsyncCopy, SourceMdfFileSuffix);
        }
    }
}
