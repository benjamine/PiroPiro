using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using PiroPiro.Config;

namespace PiroPiro.Contract
{
    /// <summary>
    /// Represents a web browser instance
    /// </summary>
    public abstract class Browser : Element, IDisposable
    {
        private static readonly List<Browser> _Instances = new List<Browser>();

        /// <summary>
        /// All created (and not disposed) instances of this class
        /// </summary>
        public static IEnumerable<Browser> Instances
        {
            get
            {
                return _Instances;
            }
        }

        /// <summary>
        /// Indicates if this is "headless" browser (instead of real user accesible browser)
        /// </summary>
        public abstract bool IsHeadless { get; }

        /// <summary>
        /// Disposes all (not disposed) instances of this class
        /// </summary>
        public static void DisposeAll()
        {
            foreach (var browser in _Instances)
            {
                browser.Dispose(false);
            }
            _Instances.Clear();
        }

        /// <summary>
        /// Base Url used when calling <see cref="Visit"/> with a relative url
        /// </summary>
        public virtual string Site { get; set; }

        /// <summary>
        /// Indicates if this browser is running on the local machine
        /// </summary>
        public abstract bool IsLocal { get; }

        /// <summary>
        /// Gets or sets the timeout for implicit waits, or null to deactivate them
        /// </summary>
        public virtual TimeSpan? ImplicitWaitTimeout { get; set; }

        /// <summary>
        /// Global browser configuration settings
        /// </summary>
        public virtual Configuration Configuration { get; protected set; }

        /// <summary>
        /// Navigates to another page
        /// </summary>
        /// <param name="url">target url, if relative <see cref="Site"/> base url is used</param>
        /// <param name="ifNotThere">if true and already at target url no action is performed</param>
        public void Visit(string url, bool ifNotThere = false)
        {

            Uri uri = new Uri(url, UriKind.RelativeOrAbsolute);
            bool onSite = false;
            if (!uri.IsAbsoluteUri)
            {
                // relative to Site base url
                uri = new Uri(new Uri(Site), url);
                onSite = true;
            }
            if (ifNotThere && uri.AbsoluteUri == Url)
            {
                return; // already there
            }
            DoVisit(uri, onSite);
        }

        protected abstract void DoVisit(Uri uri, bool onSite);

        /// <summary>
        /// Navigate back (press back button)
        /// </summary>
        public void Back()
        {
            DoBack();
        }

        protected abstract void DoBack();

        /// <summary>
        /// Navigate forward (press forward button)
        /// </summary>
        public void Forward()
        {
            DoForward();
        }

        protected abstract void DoForward();

        /// <summary>
        /// Refresh current page (press refresh button)
        /// </summary>
        public void Refresh()
        {
            DoRefresh();
        }

        protected abstract void DoRefresh();

        /// <summary>
        /// Title of the page currently loaded
        /// </summary>
        public virtual string PageTitle { get { throw new NotImplementedException(); } }

        /// <summary>
        /// Url of the current page
        /// </summary>
        public virtual string Url { get { throw new NotImplementedException(); } }

        protected override string DoGetTagName()
        {
            throw new InvalidOperationException("Not valid for Browser element");
        }

        public override string DoGetAttribute(string name)
        {
            throw new InvalidOperationException("Not valid for Browser element");
        }

        public override bool Displayed
        {
            get { throw new InvalidOperationException("Not valid for Browser element"); }
        }

        public override bool Enabled
        {
            get { throw new InvalidOperationException("Not valid for Browser element"); }
        }

        protected override void DoClear()
        {
            throw new InvalidOperationException("Not valid for Browser element");
        }

        /// <summary>
        /// Alert dialog currently showing on this browser
        /// </summary>
        public virtual Alert Alert { get { throw new NotImplementedException(); } }

        /// <summary>
        /// Browser http cookies
        /// </summary>
        public virtual Cookies Cookies { get { throw new NotImplementedException(); } }

        internal Element Wait(Func<Element> elementGet, TimeSpan wait)
        {
            return DoWaitElement(elementGet, wait);
        }

        protected virtual Element DoWaitElement(Func<Element> elementGet, TimeSpan wait)
        {
            // explicit wait default implementation

            Element element = Retrier.Get(() => elementGet(),
                retryIfDefaultValue: true,
                timeout: wait,
                retryOnExceptions: new[] { typeof(ElementNotFoundException) });
            if (element == null)
            {
                throw new ElementNotFoundException("Element not found");
            }
            return element;
        }

        internal Element ImplicitWait(Func<Element> elementGet)
        {
            return DoImplicitWaitElement(elementGet);
        }

        protected virtual Element DoImplicitWaitElement(Func<Element> elementGet)
        {
            // implicit wait default implementation

            Element element = Retrier.Get(() => elementGet(),
                retryIfDefaultValue: true,
                timeout: ImplicitWaitTimeout,
                retryOnExceptions: new[] { typeof(ElementNotFoundException) });
            if (element == null)
            {
                throw new ElementNotFoundException("Element not found");
            }
            return element;
        }

        /// <summary>
        /// Execute javascript code synchronously on the current browser context
        /// </summary>
        /// <param name="js"></param>
        public void ExecuteJS(string js)
        {
            DoExecuteJS(js);
        }

        protected abstract void DoExecuteJS(string js);


        /// <summary>
        /// Execute javascript code asynchronously on the current browser context
        /// </summary>
        /// <param name="js"></param>
        public void ExecuteJSAsync(string js)
        {
            DoExecuteJSAsync(js);
        }

        protected abstract void DoExecuteJSAsync(string js);

        /// <summary>
        /// Evaluates a javascript expression on the current browser context
        /// </summary>
        /// <param name="js"></param>
        /// <returns></returns>
        public string EvaluateJS(string js)
        {
            return DoEvaluateJS(js);
        }

        protected abstract string DoEvaluateJS(string js);

        /// <summary>
        /// Custom data for this browser instance
        /// </summary>
        public dynamic Bag { get; private set; }

        public bool BagHas(string name)
        {
            return ((IDictionary<string, object>)Bag).ContainsKey(name);
        }

        /// <summary>
        /// Indicates if this instance has already been disposed
        /// </summary>
        public bool Disposed { get; private set; }

        public override bool IsBrowser
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Creates a new browser instance
        /// </summary>
        /// <param name="cfg">source for configuration settings</param>
        public Browser(Configuration cfg = null)
            : base(null)
        {
            _Instances.Add(this);

            Configuration = cfg ?? Configuration.Default;
            Site = cfg.Get("Site");

            int implicitWaitMs = Configuration.Get<int>("PiroPiro.ImplicitWait.Timeout", false);
            if (implicitWaitMs > 0)
            {
                this.ImplicitWaitTimeout = TimeSpan.FromMilliseconds(implicitWaitMs);
            }

            Browser = this;

            Bag = new ExpandoObject();

        }

        /// <summary>
        /// Dispose this browser
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose this browser
        /// </summary>
        /// <param name="removeFromInstances"></param>
        public void Dispose(bool removeFromInstances)
        {
            if (!Disposed)
            {
                DoDispose();
                Disposed = true;
            }
            if (removeFromInstances)
            {
                _Instances.Remove(this);
            }
        }

        protected abstract void DoDispose();
    }
}
