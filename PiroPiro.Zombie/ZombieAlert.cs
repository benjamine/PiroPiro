using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Contract;
using PiroPiro.Zombie.NodeJSInterop;

namespace PiroPiro.Zombie
{
    public class ZombieAlert : Alert
    {

        public override string Text
        {
            get { return browser.Client.Execute<string>("return " + browser + ".lastConfirmMessage;"); }
        }

        public override void Accept()
        {
           browser.Client.Execute("if (" + browser + ".lastConfirmMessage) { "+browser+".lastConfirmMessage=null; } else { throw new Error('There\'s not alert to accept'); };");
        }

        public override void Dismiss()
        {
            throw new InvalidOperationException("Dismissing alerts is not supported by this driver");
        }

        protected NodeJSClient.Variable browser;

        public ZombieAlert(ZombieBrowser zbrowser, NodeJSClient.Variable browser)
            : base(zbrowser)
        {
            this.browser = browser;
        }
    }
}
