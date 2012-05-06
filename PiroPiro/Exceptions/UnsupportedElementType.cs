using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Contract;

namespace PiroPiro
{
    /// <summary>
    /// The detected element type is currently not supported
    /// </summary>
    public class UnsupportedElementType : Exception
    {
        public Element Element { get; private set; }

        public UnsupportedElementType(string message, Element element)
            : base(message)
        {
            this.Element = element;
        }
    }
}
