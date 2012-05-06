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
    public class PiroPiroBrowser : StepDefinitions
    {
        [AfterStep]
        public void ResetFocusAfterEveryStep()
        {
            Browser.WithinClear();
        }

        [AfterTestRun]
        public static void DisposeBrowsers()
        {
            Browser.DisposeAll();
        }
    }
}
