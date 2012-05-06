using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Contract;

namespace PiroPiro
{
    /// <summary>
    /// Extension methods to handle query focus, to find elements within an specified context element
    /// </summary>
    public static class WithinExt
    {
        private static Stack<Element> WithinElementStack(Browser browser)
        {
            Stack<Element> withinStack = null;
            if (browser.BagHas("WithinStack"))
            {
                withinStack = browser.Bag.WithinStack as Stack<Element>;
            }

            if (withinStack == null)
            {
                withinStack = new Stack<Element>();
                browser.Bag.WithinStack = withinStack;
            }
            return withinStack;
        }

        private static void WithinElementPush(Browser browser, Element element)
        {
            Stack<Element> withinStack = WithinElementStack(browser);
            Element current = null;
            if (browser.BagHas("WithinElement"))
            {
                current = browser.Bag.WithinElement;
            }

            if (current != null)
            {
                withinStack.Push(current);
            }
            browser.Bag.WithinElement = element;
        }

        private static Element WithinElementPop(Browser browser)
        {
            Stack<Element> withinStack = WithinElementStack(browser);
            Element current = null;
            if (browser.BagHas("WithinElement"))
            {
                current = browser.Bag.WithinElement;
            }

            if (withinStack.Count == 0)
            {
                browser.Bag.WithinElement = null;
            }
            else
            {
                browser.Bag.WithinElement = withinStack.Pop();
            }
            return current;
        }

        /// <summary>
        /// Executes inner action within a context element (on inner action, queries search inside this container)
        /// </summary>
        /// <param name="container"></param>
        /// <param name="element"></param>
        /// <param name="action"></param>
        public static void Within(this Element container, Element element, Action<Element> action)
        {
            WithinElementPush(container.Browser, element);
            try
            {
                action(element);
            }
            finally
            {
                WithinElementPop(container.Browser);
            }
        }

        /// <summary>
        /// Gets the current context element
        /// </summary>
        /// <param name="browser"></param>
        /// <returns></returns>
        public static Element CurrentWithinElement(this Browser browser)
        {
            if (browser.BagHas("WithinElement"))
            {
                return browser.Bag.WithinElement;
            }
            return null;
        }

        /// <summary>
        /// Begin using a context element (call WithinClear to leave context)
        /// </summary>
        /// <param name="container"></param>
        /// <param name="element"></param>
        public static void WithinBegin(this Element container, Element element)
        {
            WithinElementPush(container.Browser, element);
        }

        /// <summary>
        /// Resets context, queries will search on whole page
        /// </summary>
        /// <param name="browser"></param>
        public static void WithinClear(this Browser browser)
        {
            browser.Bag.WithinStack = null;
            browser.Bag.WithinElement = null;
        }

        /// <summary>
        /// Executes inner action using a table row (tr) as context element
        /// </summary>
        /// <param name="container"></param>
        /// <param name="textOrSelector"></param>
        /// <param name="action"></param>
        public static void WithinTableRow(this Element container, string textOrSelector, Action<Element> action)
        {
            container.Within(container.TableRow(textOrSelector), action);
        }

        /// <summary>
        /// Executes inner action using a fieldset as context element
        /// </summary>
        /// <param name="container"></param>
        /// <param name="legendOrSelector"></param>
        /// <param name="action"></param>
        public static void WithinFieldSet(this Element container, string legendOrSelector, Action<Element> action)
        {
            container.Within(container.FieldSet(legendOrSelector), action);
        }

        /// <summary>
        /// Executes inner action using the first displayed iframe content as context
        /// </summary>
        /// <param name="container"></param>
        /// <param name="action"></param>
        public static void WithinIFrame(this Element container, Action<Element> action)
        {
            // perform implicit wait if necessary
            container.Browser.ImplicitWait(() =>
            {
                Element iframe;
                try
                {
                    iframe = container.Query("iframe").First(i => i.Displayed).IFrameContent();
                    container.Within(iframe, action);

                    // ensure leaving iframe
                    container.LeaveIFrameContent();
                }
                catch (InvalidOperationException ex)
                {
                    throw new ElementNotFoundException("no displayed iframe has been found: " + ex.Message);
                }
                return iframe;
            });
        }

        /// <summary>
        /// Executes inner action using a displayed iframe content as context
        /// </summary>
        /// <param name="container"></param>
        /// <param name="index"></param>
        /// <param name="action"></param>
        public static void WithinIFrame(this Element container, int index, Action<Element> action)
        {
            // perform implicit wait if necessary
            container.Browser.ImplicitWait(() =>
            {
                Element iframe;
                try
                {
                    iframe = container.Query("iframe").Where(i => i.Displayed).ElementAt(index).IFrameContent();
                    container.Within(iframe, action);
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new ElementNotFoundException("no displayed iframe has been found with index: " + index);
                }
                return iframe;
            });
        }

    }
}
