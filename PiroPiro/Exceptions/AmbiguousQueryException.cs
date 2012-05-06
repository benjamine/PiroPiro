using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiroPiro
{
    /// <summary>
    /// More than one element has been found in a query for single element
    /// </summary>
    public class AmbiguousQueryException : Exception
    {
        public AmbiguousQueryException(string message)
            : base(message)
        {
        }
    }
}
