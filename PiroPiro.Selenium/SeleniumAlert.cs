using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Contract;

namespace PiroPiro.Selenium
{
    public class SeleniumAlert : Alert
    {
        private OpenQA.Selenium.IAlert alert;

        public SeleniumAlert(OpenQA.Selenium.IAlert alert, SeleniumBrowser browser)
            : base(browser)
        {
            this.alert = alert;
        }

        public override string Text
        {
            get { return alert.Text; }
        }

        public override void Accept()
        {
            alert.Accept();
        }

        public override void Dismiss()
        {
            alert.Dismiss();
        }
    }
}
