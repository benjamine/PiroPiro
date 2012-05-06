using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Config;
using PiroPiro.Contract;
using System.Net.Sockets;
using System.Threading;
using PiroPiro.Zombie.NodeJSInterop;
using Newtonsoft.Json;

namespace PiroPiro.Zombie
{
    public class ZombieBrowser : Browser
    {
        public override bool IsHeadless { get { return true; } }

        public NodeJSClient Client { get; private set; }

        protected NodeJSClient.Variable browser;

        public override bool IsLocal
        {
            get { return true; }
        }

        public ZombieBrowser(Configuration cfg = null)
            : base(cfg)
        {
            // default zombie host & port
            Client = new NodeJSClient("localhost", 8124);
            Client.SessionReset += new EventHandler(Client_SessionReset);
            PrepareBrowser();
        }

        void Client_SessionReset(object sender, EventArgs e)
        {
            PrepareBrowser();
        }

        private void PrepareBrowser()
        {
            string authScheme = Configuration.Get("BrowserAuth_Scheme", false);
            string user = Configuration.Get("BrowserAuth_User", false);
            string password = Configuration.Get("BrowserAuth_Password", false);

            var options = new
            {
                runScripts = true,
                debug = true,
                waitFor = 5000,
                credentials = new
                {
                    scheme = authScheme,
                    user = user,
                    password = password
                },
                // behave as a Google Chrome browser
                userAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.7 (KHTML, like Gecko) Chrome/16.0.912.75 Safari/535.7"
            };

            browser = Client.CreateVar("browser", "new Browser(" + JsonConvert.SerializeObject(options) + ")");
            Client.Execute(browser + ".onconfirm(function(message){ browser.lastConfirmMessage = message; return true; });");
            Client.Execute(browser + ".on('loaded',function(){ console.log('\\nPAGE LOADED: '+" + browser + ".location+'\\n'); });");
        }

        /// <summary>
        /// Waits until all the resources are loaded and all pending events are processed
        /// </summary>
        public void Wait()
        {
            Client.ExecuteAsync(browser + ".wait(doneNoResult);");
        }

        #region IBrowserImpl

        protected override void DoVisit(Uri uri, bool onSite)
        {
            Client.ExecuteAsync(browser + ".visit('" + Client.Escape(uri.AbsoluteUri) +
                @"',function(err){
                    if (err){
                        done(err);
                    }
                    try {
                        var browser = " + browser + @";
                        browser.fire('load', browser.window);
                        browser.wait(10000, doneNoResult);
                    } catch(err) {
                        done(err);
                    }
                });");
        }

        protected override void DoBack()
        {
            throw new NotImplementedException();
            //driver.Navigate().Back();
        }

        protected override void DoForward()
        {
            throw new NotImplementedException();
            //driver.Navigate().Forward();
        }

        protected override void DoRefresh()
        {
            throw new NotImplementedException();
            //driver.Navigate().Refresh();
        }

        public override string PageTitle
        {
            get
            {
                return Client.Execute<string>("return " + browser + ".text('title');");
            }
        }

        public override string Url
        {
            get
            {

                return Client.Execute<string>("return " + browser + ".location.href || '';");
            }
        }

        public override Alert Alert
        {
            get
            {
                return new ZombieAlert(this, browser);
            }
        }

        //private SeleniumCookies cookies;

        public override Cookies Cookies
        {
            get
            {
                throw new NotImplementedException();
                //if (cookies == null)
                //{
                //    cookies = new SeleniumCookies(driver.Manage().Cookies, this);
                //}
                //return cookies;
            }
        }

        protected override void DoExecuteJS(string js)
        {
            Client.Execute(browser + ".evaluate('" + Client.Escape(js) + "');");
        }

        protected override void DoExecuteJSAsync(string js)
        {
            Client.Execute("setTimeout(function(){ " + browser + ".evaluate('" + Client.Escape(js) + "'); },1)");
        }

        protected override string DoEvaluateJS(string js)
        {
            return Client.Execute<string>("return " + browser + ".evaluate('" + Client.Escape(js) + "');");
        }

        protected override IEnumerable<Element> DoQuery(string selector)
        {
            return Client.Execute<NodeJSClient.Variable[]>(
                "return _ref(" + browser + ".querySelectorAll('" + Client.Escape(selector) + "').toArray());")
                .Select(e => new ZombieElement((ZombieBrowser)Browser, browser, e));
        }

        protected override Element DoQuerySingle(string selector)
        {
            // TODO: use zombie specific method (instead of querySelectorAll)
            return Client.Execute<NodeJSClient.Variable[]>(
                "return _ref(" + browser + ".querySelectorAll('" + Client.Escape(selector) + "').toArray());")
                .Select(e => new ZombieElement((ZombieBrowser)Browser, browser, e)).Single();
        }

        public override string Text
        {
            get
            {
                return Client.Execute<string>("return " + browser + ".text();");
            }
        }

        public override string Html
        {
            get
            {
                return Client.Execute<string>("return " + browser + ".html();");
            }
        }

        protected override void DoClick()
        {
            //driver.FindElementByTagName("body").Click();
            throw new NotImplementedException();
        }

        protected override void DoSendKeys(string keys)
        {
            //driver.Keyboard.SendKeys(keys);
            throw new NotImplementedException();
        }

        protected override void DoDispose()
        {
            try
            {
                Client.Dispose();
            }
            catch
            {
            }
        }

        protected override Element DoWaitElement(Func<Element> elementGet, TimeSpan wait)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
