using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Contract;

namespace PiroPiro
{
    /// <summary>
    /// The type of an element is invalid for the attempted operation
    /// </summary>
    public class WrongElementTypeException : Exception
    {
        public Element Element { get; private set; }

        public WrongElementTypeException(string message, Element element)
            : base(message)
        {
            this.Element = element;
        }
    }
}
