using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using PiroPiro.Contract;

namespace PiroPiro.SpecFlow.Web
{
    public static class SpecFlowExt
    {
        /// <summary>
        /// context Browser instance
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Browser Browser(this ScenarioContext context)
        {
            Browser browser;
            if (!context.TryGetValue("Browser", out browser))
            {
                // try to reuse the last created browser
                browser = PiroPiro.Contract.Browser.Instances.LastOrDefault();
                if (browser == null)
                {
                    // use factory to create a new browser
                    BrowserFactory factory;
                    string browserFactoryName = null;
                    if (context.TryGetValue("BrowserFactoryName", out browserFactoryName))
                    {
                        factory = BrowserFactory.GetByName(browserFactoryName);
                    }
                    else
                    {
                        factory = BrowserFactory.Default;
                    }

                    if (factory == null)
                    {
                        throw new Exception(string.Format("Cannot create Browser, no BrowserFactory has been registered{0}.",
                            string.IsNullOrEmpty(browserFactoryName) ? "" : " with name '" + browserFactoryName + "'"));
                    }
                    browser = factory.Create();
                }
                context["Browser"] = browser;
            }
            return browser;
        }

        /// <summary>
        /// Dispose Browser instance in this context
        /// </summary>
        /// <param name="context"></param>
        public static void DisposeBrowser(this ScenarioContext context)
        {
            Browser browser;
            if (context.TryGetValue("Browser", out browser))
            {
                browser.Dispose();
                context.Remove("Browser");
            }
        }

        /// <summary>
        /// Returns the currently focused element on the Page (<see cref="Element.Within"/>) if exists, otherwise returns the Browser.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Element Page(this ScenarioContext context)
        {
            Browser browser = context.Browser();
            return browser.CurrentWithinElement() ?? browser;
        }

        /// <summary>
        /// Converts a Table object into a Dictionary using 'Field' column as key, and 'Value' column as value.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static IDictionary<string, string> ToFieldValuesDictionary(this Table table)
        {
            if (!table.Header.Contains("Field"))
            {
                throw new Exception("Column named 'Field' not found");
            }
            if (!table.Header.Contains("Value"))
            {
                throw new Exception("Column named 'Value' not found");
            }
            return table.Rows.ToDictionary(r => r["Field"], r => r["Value"]);
        }

        /// <summary>
        /// Fills a set of input fields using a Table object.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="table"> </param>
        public static void FillFields(this Element element, Table table)
        {
            element.FillFields(table.ToFieldValuesDictionary());
        }


    }
}
