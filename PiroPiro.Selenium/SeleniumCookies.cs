using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Contract;

namespace PiroPiro.Selenium
{
    public class SeleniumCookies : Cookies
    {
        private OpenQA.Selenium.ICookieJar cookies;
        private Selenium.SeleniumBrowser browser;

        public SeleniumCookies(OpenQA.Selenium.ICookieJar cookies, SeleniumBrowser browser)
        {
            this.cookies = cookies;
            this.browser = browser;
        }

        #region CookiesImpl

        public override Browser Browser
        {
            get { return this.browser; }
        }

        public override void Set(string name, string value, string domain, string path, DateTime? expiry)
        {
            if (value == null)
            {
                cookies.DeleteCookieNamed(name);
            }
            else
            {
                cookies.AddCookie(new OpenQA.Selenium.Cookie(name, value, domain, path, expiry));
            }
        }

        public override string Get(string name)
        {
            return cookies.GetCookieNamed(name).Value;
        }

        public override void Clear()
        {
            cookies.DeleteAllCookies();
        }

        #endregion

    }
}
