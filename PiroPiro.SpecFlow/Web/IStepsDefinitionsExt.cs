using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using PiroPiro.Contract;

namespace PiroPiro.SpecFlow.Web
{
    public static class IStepsDefinitionsExt
    {
        /// <summary>
        /// Gets current scenario context
        /// </summary>
        /// <param name="stepsDef"></param>
        /// <returns></returns>
        public static ScenarioContext Context(this IStepDefinitions stepsDef)
        {
            return ScenarioContext.Current;
        }

        /// <summary>
        /// context Browser instance
        /// </summary>
        /// <param name="stepsDef"></param>
        /// <returns></returns>
        public static Browser Browser(this IStepDefinitions stepsDef)
        {
            return stepsDef.Context().Browser();
        }

        /// <summary>
        /// Returns the currently focused element on the Page (<see cref="Element.Within"/>) if exists, otherwise returns the Browser.
        /// </summary>
        /// <param name="stepsDef"></param>
        /// <returns></returns>
        public static Element Page(this IStepDefinitions stepsDef)
        {
            return stepsDef.Context().Page();
        }

    }
}
