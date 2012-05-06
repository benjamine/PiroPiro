PiroPiro - Browser automation with a real user interaction API
==================================================

PiroPiro is a .Net automated testing framework inspired in [Capybara](https://github.com/jnicklas/capybara).


PiroPiro helps you test web applications by simulating how a real user would interact with your app. 
It is agnostic about the driver running your tests and comes with SpecFlow and Selenium support built in. 
A [Zombie.JS](http://zombie.labnotes.org/) (Node.JS headless "insanely fast" browser) driver is in progress.


(Description partially stolen from capybara project)

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
