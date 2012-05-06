using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Contract;

namespace PiroPiro.SpecFlow.Web
{
    /// <summary>
    /// Base class for step definitions using PiroPiro SpecFlow integration.
    /// Provides shortcut properties to extensions methods in <see cref="IStepDefinitions"/>
    /// </summary>
    public abstract class StepDefinitions : IStepDefinitions
    {
        /// <summary>
        /// context Browser instance
        /// </summary>
        public Browser Browser
        {
            get
            {
                return this.Browser();
            }
        }

        /// <summary>
        /// Returns the currently focused element on the Page (<see cref="Element.Within"/>) if exists, otherwise returns the Browser.
        /// </summary>
        public Element Page
        {
            get
            {
                return this.Page();
            }
        }
    }
}
