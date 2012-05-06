using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using PiroPiro.SpecFlow;
using Shouldly;

namespace PiroPiro.SpecFlow.Web
{
    /// <summary>
    /// Generic steps for web apps
    /// </summary>
    [Binding]
    public class GenericSteps : StepDefinitions
    {
        [When(@"visit ""(.*)""")]
        public void WhenVisit__(string url)
        {
            Browser.Visit(url);
        }

        [When(@"visit ""(.*)"", if not there")]
        public void WhenVisit__IfNotThere(string url)
        {
            Browser.Visit(url, true);
        }

        [When(@"reload page")]
        public void WhenReloadPage()
        {
            Browser.Refresh();
        }

        [When(@"navigate Back")]
        public void WhenPressBackButton()
        {
            Browser.Back();
        }

        [When(@"navigate Forward")]
        public void WhenPressForwardButton()
        {
            Browser.Forward();
        }

        [When(@"clear cookies")]
        public void WhenClearCookies()
        {
            Browser.Cookies.Clear();
        }

        [When(@"press keys ""(.*)""")]
        public void WhenPressKeys(string keys)
        {
            Browser.SendKeys(keys);
        }

        [When(@"type ""(.*)""")]
        public void WhenType(string keys)
        {
            Browser.SendKeys(keys);
        }

        [When(@"click button ""(.*)""")]
        public void WhenClickButton(string captionOrSelector)
        {
            Page.Button(captionOrSelector).Click();
        }

        [When(@"click link ""(.*)""")]
        public void WhenClickLink(string textOrSelector)
        {
            Page.Link(textOrSelector).Click();
        }

        [When(@"click on ""(.*)""")]
        public void WhenClickOn(string captionOrSelector)
        {
            Page.ButtonOrLink(captionOrSelector).Click();
        }

        [When(@"fill ""(.*)"" with ""(.*)""")]
        public void WhenFillWith(string labelNameOrSelector, string value)
        {
            Page.Field(labelNameOrSelector).FillWith(value);
        }

        [When(@"fill fields with:")]
        public void WhenFillFieldsWith(TechTalk.SpecFlow.Table table)
        {
            Page.FillFields(table.ToFieldValuesDictionary());
        }

        [Then(@"should read ""(.*)""")]
        public void ThenShouldRead(string text)
        {
            Page.Text.ShouldContain(text);
        }

        [Then(@"should not read ""(.*)""")]
        public void ThenShouldNotRead(string text)
        {
            Page.Text.ShouldNotContain(text);
        }

        [Then(@"should see title ""(.*)""")]
        public void ThenShouldSeeTitle(string text)
        {
            Page.Query(":header").FirstOrDefault(h => h.Text.Contains(text)).ShouldNotBe(null);
        }

        [Then(@"should see no errors")]
        public void ThenShouldSeeNoErrors()
        {
            var errText = (Page.Query(".error")
                .Select(err => err.Text).FirstOrDefault() ?? "").Trim();
            errText.ShouldBeEmpty();
        }

        [Then(@"should see (d+) errors")]
        public void ThenShouldSeeErrors(int errorCount)
        {
            Page.Query(".error").Count().ShouldBe(errorCount);
        }

        [Then(@"should read error ""(.*)""")]
        public void ThenShouldReadError(string text)
        {
            Page.Query(".error").FirstOrDefault(h => h.Text.Contains(text)).ShouldNotBe(null);
        }

        [Then(@"should see (d+) rows")]
        public void ThenShouldSeeRows(int rowcount)
        {
            Page.TableRows().Count().ShouldBe(rowcount);
        }

        [Then(@"field ""(.*)"" is ""(.*)""")]
        public void ThenFieldIs(string labelNameOrSelector, string value)
        {
            (Page.Field(labelNameOrSelector).GetFieldValue() ?? "<null>")
                .ShouldBe((value ?? "<null>").Trim());
        }

        [Then(@"fields are:")]
        public void ThenFieldsAre(Table table)
        {
            foreach (var row in table.Rows)
            {
                ThenFieldIs(row["Field"], row["Value"]);
            }
        }

        [When(@"click link with tooltip ""(.*)""")]
        public void WhenClickLinkWithTooltip(string tooltip)
        {
            Page.Link("a[title*='" + tooltip + "']").Click();
        }

        [When(@"click image ""(.*)""")]
        public void WhenClickImage(string tooltip)
        {
            Page.Image(tooltip).Click();
        }

        [When(@"click on text ""(.*)""")]
        public void WhenClickOnText__(string text)
        {
            Page.ElementWithText(text).Click();
        }

        [When(@"focus on row ""(.*)""")]
        public void WhenFocusOnRow(string textOrSelector)
        {
            Browser.WithinBegin(Page.TableRow(textOrSelector));
        }

        [When(@"focus on iframe")]
        public void WhenFocusOnIFrame()
        {
            // set focus on first displayed iframe
            var iframe = Page.Query("iframe").Where(i => i.Displayed).FirstOrDefault();
            if (iframe == null)
            {
                throw new ElementNotFoundException("no displayed iframe was found");
            }
            Browser.WithinBegin(iframe.IFrameContent());
        }

        [When(@"reset focus")]
        public void WhenResetFocus()
        {
            Browser.WithinClear();
        }

        [When(@"accept modal dialog")]
        public void WhenAcceptConfirmDialog()
        {
            Browser.Alert.Accept();
        }

        [When(@"dismiss modal dialog")]
        public void WhenDismissConfirmDialog()
        {
            Browser.Alert.Dismiss();
        }

        [Then(@"should see image with tooltip ""(.*)""")]
        public void ThenShouldSeeImageWithTooltip__(string tooltip)
        {
            Page.Image(tooltip);
        }

        [Then(@"should see link with tooltip ""(.*)""")]
        public void ThenShouldSeeLinkWithTooltip__(string tooltip)
        {
            Page.Link("a[title*='" + tooltip + "']");
        }

        [Then(@"should see link ""(.*)""")]
        public void ThenShouldSeeLink__(string textOrSelector)
        {
            Page.Link(textOrSelector);
        }

        [Then(@"should not see image with tooltip ""(.*)""")]
        public void ThenShouldNotSeeImageWithTooltip__(string tooltip)
        {
            Should.Throw<ElementNotFoundException>(() =>
                Page.Image(tooltip).SeenByUser()
            );
        }

        [Then(@"should not see link with tooltip ""(.*)""")]
        public void ThenShouldNotSeeLinkWithTooltip__(string tooltip)
        {
            Should.Throw<ElementNotFoundException>(() =>
                Page.Link("a[title*='" + tooltip + "']").SeenByUser()
            );
        }

        [Then(@"should not see link ""(.*)""")]
        public void ThenShouldNotSeeLink__(string textOrSelector)
        {
            Should.Throw<ElementNotFoundException>(() =>
                Page.Link(textOrSelector).SeenByUser()
            );
        }

    }
}
