using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiroPiro
{
    /// <summary>
    /// A test file found could not be found
    /// </summary>
    public class TestFileNotFoundException : Exception
    {
        public string Path { get; private set; }

        public TestFileNotFoundException(string path)
            : base("Test file not found: " + path)
        {
            Path = path;
        }

        public TestFileNotFoundException(string path, Exception innerException)
            : base("Test file not found: " + path, innerException)
        {
            Path = path;
        }
    }
}
