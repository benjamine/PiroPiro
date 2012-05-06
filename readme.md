PiroPiro - Browser automation with a real user interaction API
==================================================

PiroPiro is a .Net automated testing framework inspired in [Capybara](https://github.com/jnicklas/capybara).

![PiroPiro Logo](https://github.com/benjamine/PiroPiro/raw/master/PiroPiro/icon/piropiro.png)

PiroPiro helps you test web applications by simulating how a real user would interact with your app. 
It is agnostic about the driver running your tests and comes with SpecFlow and Selenium support built in. 
A [Zombie.JS](http://zombie.labnotes.org/) (Node.JS headless "insanely fast" browser) driver is in progress.


(Description partially stolen from capybara project)

Installing
----------------

NuGet Packages will be published soon. 

In the meantime you can install by referencing the compiled assemblies, and structuring your feature spec project based on PiroPiro.TestSite.Spec project.


Code sample
--------------------

``` csharp

        [When(@"login as ""(.*)""" with password ""(.*)""")]
        public void WhenLoginAsWithPassword(string usernameOrEmail, string password)
        {
            Page.Visit("/login");
            
            Page.WithinFieldSet("Log in", fs => {
               // find fields using label element caption, input name or Sizzle css selectors
               fs.Field("Username or email").FillWith(usernameOrEmail);
               fs.Field("Password").FillWith(password);
               fs.Field("Remember me").Check();
            });
            
            Page.Button("Log in").Click();
            
            // explicit wait
            Page.Wait(Page.ElementWithText("Welcome"), seconds: 2);
        }

        [Then(@"user logged in successfully with role ""(.*)""")]
        public void UserIsLoggedInWithRole(string role)
        {        
            // query using Sizzle (the jQuery selector engine, css syntax + extensions)
            Page.Query(".error").Count().ShouldBe(0);
            
            // find an image by its tooltip
            Page.Image("user picture");
            
            Page.QuerySingle(".role").Text.ShouldContain(role);
        }

```


Project Structure
--------------------------------------

### PiroPiro

Main project, it provides the base abstract classes for browser automation (BrowserFactory, Browser, Element, etc.)


### PiroPiro.DbCleanup

A collection of simple strategies to restore databases between test runs.
At the moment of this writing there are:
- SqlRestore: performs a SQL RESTORE 
- SqlAttach: detachs a db, overwrites if .mdf file and attachs again
You can specify which to run (or none) using configuration


### PiroPiro.SpecFlow

Provides integration with [TechTalk SpecFlow](http://www.specflow.org/), allowing you to use the PiroPiro browser automation API from your SpecFlow step definitions.
In order to use it you have 2 options:
- Derive your step definitions class from PiroPiro.SpecFlow.Web.StepDefinitions (giving you .Browser and .Page properties)
- Add PiroPiro.SpecFlow.Web.IStepDefinitions to you step definitions class (giving you methods this.Browser() and this.Page())


### PiroPiro.Selenium

[Selenium](http://seleniumhq.org) Driver, it uses Selenium WebDriver allowing you to run tests on real browsers locally or on remote machines.


### PiroPiro.Zombie
A driver for [Zombie.JS](http://zombie.labnotes.org/). This is a very early stage, not usable right now.


### PiroPiro.TestSite

This is an example site built only to serve as a test target for PiroPiro feature testing.


### PiroPiro.TestSite.Spec

This is a feature spec project using PiroPiro, it tests PiroPiro features against the test site.
To use it see "Running the Unit Tests" below.

This project also is a good example of how to build your feature spec project, check the readme files on its folder structure.



System Requirements
--------------------------------------

To Run Tests:
- NUnit GUI or console exe
- Asp.Net Mvc 3
- .Net Framework 4

To Write Tests:
- Visual Studio 2010
- TechTalk SpecFlow extension (provides editor & compiling support for .feature files)


Running the Unit Tests
--------------------------------------

A test site and a feature spec project are included. No database is required. Feature tests are generated for NUnit.

In order to run tests you need:

- Set up PiroPiro.TestSite on your local IIS (eg. http://localhost/PiroPiro.TestSite)
- Compile the Solution
- check app.config in PiroPiro.TestSite.Spec, in appSettings you'll find:
 - Site: the base url for test site as you configured it in previous step
 - PiroPiro.BrowserFactory: the underlying web driver, by default Selenium WebDriver is used
 - PiroPiro.Selenium.Driver: if using Selenium, here you can choose the specific browser to instantiate, by default 'firefox'
- Right-click on PiroPiro.TestSite.Spec project and click on "Run Unit Tests" (you can use NUnit GUI or NUnit console if you prefer)

Why PiroPiro
---------------------

Piro-piro is just another name for capybara.
(http://en.wikipedia.org/wiki/Hydrochoerus_hydrochaeris)

The first version of this library has been written as part of my work at [Tercer Planeta](http://www.tercerplaneta.com).
When I left my job there we decided to open source this for anyone to freely read, use, fork, and contribute back.

License
----------------------------
[MIT License](https://github.com/benjamine/PiroPiro/blob/master/MIT-LICENSE.txt)
