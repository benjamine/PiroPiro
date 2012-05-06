using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiroPiro
{
    /// <summary>
    /// An element could not be found on the current page
    /// </summary>
    public class ElementNotFoundException : Exception
    {
        public ElementNotFoundException(string message)
            : base(message)
        {
        }

        public ElementNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
