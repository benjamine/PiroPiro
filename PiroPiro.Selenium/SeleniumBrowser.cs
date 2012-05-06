using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Config;
using PiroPiro.Contract;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SizSelCsZzz;

namespace PiroPiro.Selenium
{
    public class SeleniumBrowser : Browser
    {
        internal RemoteWebDriver Driver
        {
            get
            {
                return driver;
            }
        }

        private RemoteWebDriver driver;

        public override bool IsHeadless { get { return false; } }

        private bool isLocal;

        public override bool IsLocal
        {
            get { return isLocal; }
        }

        public SeleniumBrowser(RemoteWebDriver driver, bool isLocal, Configuration cfg = null)
            : base(cfg)
        {
            this.driver = driver;
            this.isLocal = isLocal;

            if (ImplicitWaitTimeout.HasValue && ImplicitWaitTimeout.Value.TotalMilliseconds > 0)
            {
                this.driver.Manage().Timeouts().ImplicitlyWait(ImplicitWaitTimeout.Value);
            }

            string windowSize = Configuration.Get<string>("BrowserWindowSize", false);
            if (!string.IsNullOrWhiteSpace(windowSize))
            {
                int width = 0, height = 0;
                if (windowSize.Trim() != "*")
                {
                    var dimensions = windowSize.Split(new[] { 'x' }, StringSplitOptions.RemoveEmptyEntries);
                    int.TryParse(dimensions[0].Trim(), out width);
                    if (dimensions.Length > 1)
                    {
                        int.TryParse(dimensions[1].Trim(), out height);
                    }
                }

                if (width <= 0)
                {
                    width = IsLocal ? (int)System.Windows.SystemParameters.WorkArea.Width : 1024;
                }
                if (height <= 0)
                {
                    height = IsLocal ? (int)System.Windows.SystemParameters.WorkArea.Height : 768;
                }

                this.driver.Manage().Window.Position = new System.Drawing.Point(0, 0);
                this.driver.Manage().Window.Size = new System.Drawing.Size(width, height);
            }

        }

        #region IBrowserImpl

        protected override void DoVisit(Uri uri, bool onSite)
        {
            driver.Navigate().GoToUrl(uri);
        }

        protected override void DoBack()
        {
            driver.Navigate().Back();
        }

        protected override void DoForward()
        {
            driver.Navigate().Forward();
        }

        protected override void DoRefresh()
        {
            driver.Navigate().Refresh();
        }

        public override string PageTitle
        {
            get { return driver.Title; }
        }

        public override string Url
        {
            get { return driver.Url; }
        }

        public override Alert Alert
        {
            get
            {
                return new SeleniumAlert(driver.SwitchTo().Alert(), this);
            }
        }

        private SeleniumCookies cookies;

        public override Cookies Cookies
        {
            get
            {
                if (cookies == null)
                {
                    cookies = new SeleniumCookies(driver.Manage().Cookies, this);
                }
                return cookies;
            }
        }

        protected override void DoExecuteJS(string js)
        {
            driver.ExecuteScript(js);
        }

        protected override void DoExecuteJSAsync(string js)
        {
            driver.ExecuteAsyncScript(js);
        }

        protected override string DoEvaluateJS(string js)
        {
            return (driver.ExecuteScript(js) ?? "").ToString();
        }

        protected override IEnumerable<Element> DoQuery(string selector)
        {
            return driver.FindElements(BySizzle.CssSelector(selector))
                .Select(e => new SeleniumElement(e, this));
        }

        protected override Element DoQuerySingle(string selector)
        {
            return new SeleniumElement(driver.FindElement(BySizzle.CssSelector(selector)), this);
        }

        public override string Text
        {
            get { return driver.FindElementByTagName("body").Text; }
        }

        public override string Html
        {
            get { return EvaluateJS("document.body.parentNode.outerHTML"); }
        }

        protected override void DoClick()
        {
            driver.FindElementByTagName("body").Click();
        }

        protected override void DoSendKeys(string keys)
        {
            driver.Keyboard.SendKeys(keys);
        }

        protected override void DoDispose()
        {
            try
            {
                driver.Quit();
                driver.Dispose();
            }
            catch
            {
            }
        }

        protected override void DoLeaveIFrameContent()
        {
            driver.SwitchTo().DefaultContent();
        }

        protected override Element DoWaitElement(Func<Element> elementGet, TimeSpan wait)
        {
            // override default explicit wait to use native Selenium WebDriverWait
            var driverWait = new WebDriverWait(driver, wait);
            driverWait.IgnoreExceptionTypes(typeof(ElementNotFoundException));
            return driverWait.Until(d => elementGet());
        }

        #endregion
    }
}
