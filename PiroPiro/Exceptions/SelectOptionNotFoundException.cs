using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Contract;

namespace PiroPiro
{
    /// <summary>
    /// An option could not be found on select field
    /// </summary>
    public class SelectOptionNotFoundException : ElementNotFoundException
    {
        public Element SelectElement { get; private set; }

        public SelectOptionNotFoundException(string message, Element selectElement)
            : base(message)
        {
            SelectElement = selectElement;
        }
    }
}
