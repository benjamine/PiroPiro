using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Contract;
using PiroPiro.Zombie.NodeJSInterop;

namespace PiroPiro.Zombie
{
    public class ZombieElement : Element
    {

        private NodeJSClient Client;

        private NodeJSClient.Variable element, browser;

        public ZombieElement(ZombieBrowser zbrowser, NodeJSClient.Variable browser, NodeJSClient.Variable element)
            : base(zbrowser)
        {
            Client = zbrowser.Client;
            this.browser = browser;
            this.element = element;
        }

        #region ElementImpl

        protected override IEnumerable<Element> DoQuery(string selector)
        {
            return Client.Execute<NodeJSClient.Variable[]>(
                "return _ref(" + browser + ".queryAll('" + Client.Escape(selector) + "'," + element + "));")
                .Select(e => new ZombieElement((ZombieBrowser)Browser, browser, e));
        }

        protected override Element DoQuerySingle(string selector)
        {
            // TODO: use zombie specific method (instead of queryAll)
            return Client.Execute<NodeJSClient.Variable[]>(
                "return _ref(" + browser + ".queryAll('" + Client.Escape(selector) + "'," + element + "));")
                .Select(e => new ZombieElement((ZombieBrowser)Browser, browser, e)).Single();
        }

        protected override string DoGetTagName()
        {
            return Client.Execute<string>("return " + element + ".tagName;");
        }

        public override string Text
        {
            get
            {

                return Client.Execute<string>("return " + browser + ".text(null," + element + ");");
            }
        }

        public override string Html
        {
            get
            {
                return Client.Execute<string>("return " + element + ".innerHTML;");
            }
        }

        public override string DoGetAttribute(string name)
        {
            return Client.Execute<string>("return " + element + ".getAttribute('" + name + "');");
        }

        public override bool Displayed
        {
            get { throw new NotImplementedException(); }
        }

        public override bool Enabled
        {
            get { throw new NotImplementedException(); }
        }

        protected override void DoClick()
        {
            Client.ExecuteAsync(browser + ".fire('click', " + element + ", function(err) { if(err){done(err);}else{" + browser + ".wait(doneNoResult);}});");
        }

        protected override void DoSendKeys(string keys)
        {
            if (InputType == "text" || InputType == "textarea")
            {
                Client.ExecuteAsync(element + ".value = '" + Client.Escape(keys) + "'; " + browser + ".fire('change', " + element + ", doneNoResult);");
            }
        }

        protected override void DoClear()
        {
            if (InputType == "text" || InputType == "textarea")
            {
                Client.Execute(element + ".value = '';");
            }
        }

        #endregion

    }
}
