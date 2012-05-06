using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Contract;
using System.Text.RegularExpressions;

namespace PiroPiro
{
    /// <summary>
    /// Extension methods to find diferent type of elements
    /// </summary>
    public static class FindExt
    {

        private static Regex LikeCssSelectorRegex = new Regex(@"^[a-z]*[.#\[\:].+$", RegexOptions.Compiled);

        /// <summary>
        /// Detects if a string could be a css selector (eg. starts with anyof(.#[:) or sometagname+anyof(.#[:))
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static bool LooksLikeACssSelector(string selector)
        {
            return LikeCssSelectorRegex.IsMatch(selector);
        }

        /// <summary>
        /// Finds a link (a element) by text or css selector
        /// </summary>
        /// <param name="element"></param>
        /// <param name="textOrSelector">text or css selector</param>
        /// <returns></returns>
        public static Element Link(this Element element, string textOrSelector)
        {
            // perform implicit wait if necessary
            return element.Browser.ImplicitWait(() =>
            {
                Element link = null;

                if (LooksLikeACssSelector(textOrSelector))
                {
                    // find by css selector
                    link = element.Query(textOrSelector).SingleOrDefault(e => e.TagName == "a");
                    if (link != null)
                    {
                        return link;
                    }
                }

                // find by text
                link = element.Query(string.Format("a:contains('{0}')", textOrSelector)).SingleOrDefault();
                if (link == null)
                {
                    // no link found, try to detect bad semantics
                    var elem = element.Query(":contains('{0}'):last").SingleOrDefault();
                    if (elem != null)
                    {
                        throw new ElementNotFoundException(string.Format("text '{0}' was found in a {0} element, use a elements for links", textOrSelector, elem.TagName));
                    }
                }

                if (link == null)
                {
                    throw new ElementNotFoundException(string.Format("Link '{0}' not found", textOrSelector));
                }
                return link;
            });
        }

        /// <summary>
        /// Finds table rows (tr element) by inner text or css selector
        /// </summary>
        /// <param name="element"></param>
        /// <param name="textOrSelector">text or css selector</param>
        /// <returns></returns>
        public static IEnumerable<Element> TableRows(this Element element, string textOrSelector = null)
        {
            string tableRowClass = element.Browser.Configuration.Get("TableRowClass", false);
            if (string.IsNullOrWhiteSpace(tableRowClass))
            {
                tableRowClass = null;
            }
            else
            {
                tableRowClass = tableRowClass.Trim();
            }

            if (string.IsNullOrWhiteSpace(textOrSelector))
            {
                // all rows
                return element.Query(string.Format(
                    (tableRowClass == null ? "tr" : "." + tableRowClass)
                    , textOrSelector));
            }

            if (LooksLikeACssSelector(textOrSelector))
            {
                // find by css selector
                return element.Query(textOrSelector).Where(e => (tableRowClass == null) ?
                    e.TagName == "tr" : e.Classes.Contains(tableRowClass));
            }

            // find by text
            return element.Query(string.Format(
                (tableRowClass == null ? "tr" : "." + tableRowClass)
                + ":contains('{0}')", textOrSelector));
        }

        /// <summary>
        /// Finds a table row (tr element) by inner text or css selector
        /// </summary>
        /// <param name="element"></param>
        /// <param name="textOrSelector">text or css selector</param>
        /// <returns></returns>
        public static Element TableRow(this Element element, string textOrSelector)
        {
            // perform implicit wait if necessary
            return element.Browser.ImplicitWait(() =>
            {

                Element tr = null;

                string tableRowClass = element.Browser.Configuration.Get("TableRowClass", false);
                if (string.IsNullOrWhiteSpace(tableRowClass))
                {
                    tableRowClass = null;
                }
                else
                {
                    tableRowClass = tableRowClass.Trim();
                }

                if (LooksLikeACssSelector(textOrSelector))
                {
                    // find by css selector
                    tr = element.Query(textOrSelector).SingleOrDefault(e => (tableRowClass == null) ?
                        e.TagName == "tr" : e.Classes.Contains(tableRowClass));
                    if (tr != null)
                    {
                        return tr;
                    }
                }

                // find by text
                tr = element.Query(string.Format(
                    (tableRowClass == null ? "tr" : "." + tableRowClass)
                    + ":contains('{0}')", textOrSelector)).SingleOrDefault();
                if (tr == null)
                {
                    // no link found, try to detect bad semantics
                    var elem = element.Query(":contains('{0}'):last").SingleOrDefault();
                    if (elem != null)
                    {
                        throw new ElementNotFoundException(string.Format("text '{0}' was found in a {0} element, use tr elements for table rows", textOrSelector, elem.TagName));
                    }
                }

                if (tr == null)
                {
                    throw new ElementNotFoundException(string.Format("Table row '{0}' not found", textOrSelector));
                }
                return tr;

            });
        }

        /// <summary>
        /// Finds an image (img element) by alt text, title (tooltip), or css selector
        /// </summary>
        /// <param name="element"></param>
        /// <param name="altTitleOrSelector">alt text, title or css selector</param>
        /// <returns></returns>
        public static Element Image(this Element element, string altTitleOrSelector)
        {
            // perform implicit wait if necessary
            return element.Browser.ImplicitWait(() =>
            {

                Element img = null;

                if (LooksLikeACssSelector(altTitleOrSelector))
                {
                    // find by css selector
                    img = element.Query(altTitleOrSelector).SingleOrDefault(e => e.TagName == "img");
                    if (img != null)
                    {
                        return img;
                    }
                }

                // find by alt text or title
                img = element.Query(string.Format("img[alt*='{0}'],img[title*='{0}']", altTitleOrSelector)).SingleOrDefault();
                if (img == null)
                {
                    // no img found, try to detect bad semantics
                    var elem = element.Query(":contains('{0}'):last").SingleOrDefault();
                    if (elem != null)
                    {
                        throw new ElementNotFoundException(string.Format("text '{0}' was found in a {0} element, use img elements for images", altTitleOrSelector, elem.TagName));
                    }
                }

                if (img == null)
                {
                    throw new ElementNotFoundException(string.Format("Image '{0}' not found", altTitleOrSelector));
                }
                return img;

            });
        }

        /// <summary>
        /// Finds the smaller displayed element containing a text
        /// </summary>
        /// <param name="element"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Element ElementWithText(this Element element, string text)
        {
            // perform implicit wait if necessary
            return element.Browser.ImplicitWait(() =>
            {
                var elem = element.Query(":contains('" + text + "')").LastOrDefault(e => e.Displayed);
                if (elem == null)
                {
                    throw new ElementNotFoundException(string.Format("Element with text '{0}' not found", text));
                }
                return elem;
            });
        }

        /// <summary>
        /// Throws a <see cref="ElementNotFoundException"/> if the element is not displayed to the user
        /// </summary>
        public static Element SeenByUser(this Element element)
        {
            // perform implicit wait if necessary
            return element.Browser.ImplicitWait(() =>
            {
                try
                {
                    if (!element.Displayed)
                    {
                        throw new ElementNotFoundException("element not displayed to the user");
                    }
                }
                catch (NotImplementedException)
                {
                }
                return element;
            });
        }
    }
}
