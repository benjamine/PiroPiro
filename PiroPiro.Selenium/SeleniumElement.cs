using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Contract;
using OpenQA.Selenium;
using SizSelCsZzz;

namespace PiroPiro.Selenium
{
    public class SeleniumElement : Element
    {
        private IWebElement element;
        private SeleniumBrowser SeleniumBrowser;

        public SeleniumElement(IWebElement element, SeleniumBrowser browser)
            : base(browser)
        {
            this.element = element;
            this.SeleniumBrowser = browser;
        }

        #region ElementImpl

        protected override IEnumerable<Element> DoQuery(string selector)
        {
            return element.FindElements(BySizzle.CssSelector(selector))
                .Select(e => new SeleniumElement(e, SeleniumBrowser));
        }

        protected override Element DoQuerySingle(string selector)
        {
            return new SeleniumElement(element.FindElement(BySizzle.CssSelector(selector)), SeleniumBrowser);
        }

        protected override string DoGetTagName()
        {
            return element.TagName;
        }

        public override string Text
        {
            get { return element.Text; }
        }

        public override string Html
        {
            get { throw new NotImplementedException(); }
        }

        public override string DoGetAttribute(string name)
        {
            return element.GetAttribute(name);
        }

        public override bool Displayed
        {
            get { return element.Displayed && element.Size.Height > 0 && element.Size.Width > 0; }
        }

        public override bool Enabled
        {
            get { return element.Enabled; }
        }

        protected override void DoClick()
        {
            element.Click();
        }

        protected override void DoSendKeys(string keys)
        {
            element.SendKeys(keys);
        }

        protected override void DoClear()
        {
            element.Clear();
        }

        protected override void DoSetFile(string path)
        {
            // WARNING: selenium driver doesn't support clear on input[type=file], so only send keys
            SendKeys(path);
        }

        protected override PiroPiro.Contract.Element DoGetIFrameContent()
        {
            return new SeleniumElement(SeleniumBrowser.Driver.SwitchTo()
                .Frame(element)
                .FindElement(By.TagName("html")), SeleniumBrowser);
        }

        protected override void DoLeaveIFrameContent()
        {
            SeleniumBrowser.Driver.SwitchTo().DefaultContent();
        }

        #endregion

    }
}
