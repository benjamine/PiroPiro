using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using PiroPiro.Contract;
using PiroPiro.SpecFlow.Web;
using PiroPiro.DbCleanup;

namespace PiroPiro.TestSite.Spec.TestSetup
{
    [Binding]
    public class PiroPiroDbCleanup : StepDefinitions
    {
        [BeforeTestRun]
        public static void CleanupDatabase()
        {
            DbCleanupStrategy.Default.Execute();
        }
    }
}
