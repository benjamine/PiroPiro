using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Contract;
using System.IO;
using System.Reflection;
using System.Resources;

namespace PiroPiro
{
    /// <summary>
    /// Extension methods to handle test files
    /// </summary>
    public static class TestFilesExt
    {
        /// <summary>
        /// Extracts a file from an embedded resource
        /// </summary>
        /// <param name="targetAssembly"></param>
        /// <param name="resourceName"></param>
        /// <param name="filepath"></param>
        private static void WriteResourceToFile(Assembly targetAssembly, string resourceName, string filepath)
        {
            using (Stream stream = targetAssembly.GetManifestResourceStream(targetAssembly.GetName().Name + "." + resourceName))
            {
                if (stream == null)
                {
                    throw new Exception("Cannot find embedded resource '" + resourceName + "'");
                }
                using (BinaryWriter sw = new BinaryWriter(File.Open(filepath, FileMode.Create)))
                {
                    byte[] buffer = new byte[8 * 1024];
                    int bytes = 0;
                    while ((bytes = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        sw.Write(buffer, 0, bytes);
                    }
                    sw.Flush();
                    sw.Close();
                    stream.Close();
                }
            }
        }

        /// <summary>
        /// Gets the path of a test file available from the Browser instance machine (eg. for file uploading). If browser is remote a shared folder will be used
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="relativePath">relative path to the test files folder</param>
        /// <returns></returns>
        public static string GetTestFilePath(this Browser browser, string relativePath)
        {
            if (Path.IsPathRooted(relativePath))
            {
                if (!File.Exists(relativePath))
                {
                    throw new TestFileNotFoundException(relativePath);
                }
                return relativePath;
            }

            Assembly assembly = Assembly.GetCallingAssembly();
            string resourceName = relativePath.Trim(new[] { '\\', '/', ' ' }).Replace("\\", ".").Replace("/", ".");

            string folder = browser.IsLocal ?
                Path.Combine(Path.GetDirectoryName(assembly.Location), "TestFiles") :
                Path.Combine(browser.Configuration.Get("PiroPiro.SharedTempPath", true), "TestFiles");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            string file = Path.Combine(folder, relativePath);

            if (File.Exists(file) && File.GetCreationTime(file) > File.GetLastWriteTime(assembly.Location))
            {
                // file is already extracted and updated
                return file;
            }
            else
            {
                try
                {
                    // extract embedded resource to target folder
                    WriteResourceToFile(assembly, "TestFiles." + resourceName, file);
                    return file;
                }
                catch (Exception ex)
                {
                    throw new TestFileNotFoundException("Test file not found: " + relativePath, ex);
                }
            }
        }
    }
}
