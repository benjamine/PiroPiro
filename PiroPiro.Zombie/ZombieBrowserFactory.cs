using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PiroPiro.Config;
using PiroPiro.Contract;

namespace PiroPiro.Zombie
{
    public class ZombieBrowserFactory : BrowserFactory
    {
        protected override Browser DoCreate(Configuration cfg)
        {
            return new ZombieBrowser(cfg);
        }
    }
}
