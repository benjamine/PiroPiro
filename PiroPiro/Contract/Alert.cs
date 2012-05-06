using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiroPiro.Contract
{
    /// <summary>
    /// Represents a javascript dialog open in the browser
    /// </summary>
    public abstract class Alert
    {
        public Browser Browser { get; private set; }

        /// <summary>
        /// Message being displayed on this dialog
        /// </summary>
        public abstract string Text { get; }

        /// <summary>
        /// Accept dialog, closing it
        /// </summary>
        public abstract void Accept();

        /// <summary>
        /// Dismiss dialog, closing it
        /// </summary>
        public abstract void Dismiss();

        public Alert(Browser browser)
        {
            this.Browser = browser;
        }
    }
}
