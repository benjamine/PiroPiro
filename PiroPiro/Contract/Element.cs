using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiroPiro.Contract
{
    /// <summary>
    /// Represents a DOM Element on a Browser current page
    /// </summary>
    public abstract class Element
    {
        /// <summary>
        /// Browser containing this Element
        /// </summary>
        public Browser Browser { get; protected set; }

        /// <summary>
        /// returns a collection of Elements matching specified css selector
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public IEnumerable<Element> Query(string selector)
        {
            return DoQuery(selector);
        }

        /// <summary>
        /// returns a collection of Elements matching specified css selector
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        protected abstract IEnumerable<Element> DoQuery(string selector);

        /// <summary>
        /// returns an Element matching specified css selector (performs an implicit wait if necessary)
        /// </summary>        
        /// <param name="selector"></param>
        /// <returns></returns>
        public Element QuerySingle(string selector)
        {
            try
            {
                return Browser.ImplicitWait(() => DoQuerySingle(selector));
            }
            catch (InvalidOperationException)
            {
                throw new AmbiguousQueryException(string.Format("More than one element has been obtained for: '{0}'", selector));
            }
            catch (ElementNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // catch other exceptions, as Selenium.ElementNotFound
                throw new ElementNotFoundException(string.Format("Element '{0}' not found on the page", selector), ex);
            }
        }

        /// <summary>
        /// returns an Element matching specified css selector.
        /// </summary>
        /// <param name="selector"></param>
        /// <returns>if the element is not present return null or throw</returns>
        protected virtual Element DoQuerySingle(string selector)
        {
            try
            {
                return Query(selector).SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw new AmbiguousQueryException(string.Format("More than one element has been obtained for: '{0}'", selector));
            }
        }

        /// <summary>
        /// Element tag name, lower case
        /// </summary>
        public string TagName
        {
            get
            {
                if (_TagName == null)
                {
                    _TagName = DoGetTagName().ToLowerInvariant() ?? "";
                }
                return _TagName;
            }
        }

        private string _TagName;

        protected abstract string DoGetTagName();

        public abstract string Text { get; }

        public abstract string Html { get; }

        /// <summary>
        /// Defines attributes that can be cached (ie. asked only once to the underlying driver)
        /// </summary>
        protected string[] CacheableAttributes = new[] { "type" };

        private readonly Dictionary<string, string> _AttributeCache = new Dictionary<string, string>();

        /// <summary>
        /// Get an attribute value by name
        /// </summary>
        /// <param name="name">attribute name</param>
        /// <returns></returns>
        public string GetAttribute(string name)
        {
            string attrName = name.ToLowerInvariant(), value;
            if (CacheableAttributes.Contains(attrName))
            {
                if (!_AttributeCache.TryGetValue(attrName, out value))
                {
                    _AttributeCache[name] = value = DoGetAttribute(name);
                }
                return value;
            }
            else
            {
                return DoGetAttribute(attrName);
            }
        }

        public abstract string DoGetAttribute(string name);

        /// <summary>
        /// Indicates if the element is visible to the user
        /// </summary>
        public abstract bool Displayed { get; }

        /// <summary>
        /// Indicates if the element is enabled
        /// </summary>
        public abstract bool Enabled { get; }

        #region UserActions

        /// <summary>
        /// Simulate a mouse click on this element
        /// </summary>
        public void Click()
        {
            DoClick();
        }

        protected abstract void DoClick();

        /// <summary>
        /// Type keys focusing on this element (eg. fill textbox)
        /// </summary>
        /// <param name="keys"></param>
        public void SendKeys(string keys)
        {
            DoSendKeys(keys);
        }

        protected abstract void DoSendKeys(string keys);

        /// <summary>
        /// Clear element content (textboxes and textareas)
        /// </summary>
        public void Clear()
        {
            DoClear();
        }

        protected abstract void DoClear();

        #endregion

        /// <summary>
        /// Indicates if this is the root Browser element
        /// </summary>
        public virtual bool IsBrowser
        {
            get
            {
                return false;
            }
        }

        public Element(Browser browser)
        {
            this.Browser = browser;
        }

        /// <summary>
        /// id attribute
        /// </summary>
        public virtual string Id
        {
            get
            {
                return GetAttribute("id");
            }
        }

        /// <summary>
        /// class attribute
        /// </summary>
        public virtual string Class
        {
            get
            {
                return (GetAttribute("class") ?? "").Trim();
            }
        }

        /// <summary>
        /// Css classes aplied to this element
        /// </summary>
        public virtual string[] Classes
        {
            get
            {
                return Class
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim()).Distinct().ToArray();
            }
        }

        /// <summary>
        /// Returns the input type for this element (lower case), or null if this is not a input element.
        /// select, textarea, button tag names are returned as input types.
        /// </summary>
        public virtual string InputType
        {
            get
            {
                switch (TagName)
                {
                    case "select":
                        return "select";
                    case "a":
                        return "link";
                    case "textarea":
                        return "textarea";
                    case "button":
                        return "button";
                    case "input":
                        string typeAttr = GetAttribute("type");
                        if (string.IsNullOrEmpty("typeattr"))
                        {
                            typeAttr = "text";
                        }
                        else
                        {
                            typeAttr = typeAttr.ToLowerInvariant();
                        }
                        return typeAttr;
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Selects an option on this element (only valid for select fields)
        /// </summary>
        /// <param name="text"></param>
        public virtual void SelectOptionByText(string text)
        {
            if (InputType != "select")
            {
                throw new WrongElementTypeException("options can only be found on select input elements", this);
            }

            var selectedOptions = Query("option:selected"); // sizzle selector
            if (selectedOptions.Any(o => o.Text == text))
            {
                // already selected
                return;
            }
            else
            {
                var option = Query("option").FirstOrDefault(o => o.Text == text);
                if (option == null)
                {
                    string[] available = Query("option").Select(o => '"' + (o.Text ?? "") + '"').ToArray();
                    throw new SelectOptionNotFoundException(string.Format("Option \"{0}\" not found on this select, available options are: {1}",
                        text, available.Length < 1 ? "<empty>" : string.Join(", ", available)), this);
                }
                option.Click();
            }
        }

        /// <summary>
        /// Gets the text of the currently selected option (only valid for select fields)
        /// </summary>
        /// <returns></returns>
        public virtual string SelectedOptionsText()
        {
            if (InputType != "select")
            {
                throw new WrongElementTypeException("options can only be found on select input elements", this);
            }

            return string.Join(", ", Query("option:selected") // sizzle selector
                .Select(o => o.Text));
        }

        /// <summary>
        /// Sets the path for a input file element (only valid for file fields)
        /// </summary>
        /// <param name="path"></param>
        public virtual void SetFile(string path)
        {
            if (InputType != "file")
            {
                throw new WrongElementTypeException("a file path can only be set on an input[type=file] element", this);
            }
            DoSetFile(path);
        }

        protected virtual void DoSetFile(string path)
        {
            // by default handle input[type=file] as a regular textbox
            Clear();
            SendKeys(path);
        }

        /// <summary>
        /// If this elements is an iframe returns the inner html element (only valid for iframe elements)
        /// </summary>
        /// <returns></returns>
        public Element IFrameContent()
        {
            if (this.TagName != "iframe")
            {
                throw new WrongElementTypeException("Error getting iframe content, this is not an iframe element", this);
            }
            return DoGetIFrameContent();
        }

        /// <summary>
        /// Ensures queries are executed against main window frame
        /// </summary>
        public void LeaveIFrameContent()
        {
            DoLeaveIFrameContent();
        }

        protected virtual Element DoGetIFrameContent()
        {
            throw new NotImplementedException();
        }

        protected virtual void DoLeaveIFrameContent()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Wait for an element to appear
        /// </summary>
        /// <param name="elementGet"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public Element Wait(Func<Element, Element> elementGet, int seconds)
        {
            return Wait(elementGet, TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// Wait for an element to appear
        /// </summary>
        /// <param name="elementGet"></param>
        /// <param name="wait"></param>
        /// <returns></returns>
        public Element Wait(Func<Element, Element> elementGet, TimeSpan wait)
        {
            return Browser.Wait(browser => elementGet(this), wait);
        }

    }
}
