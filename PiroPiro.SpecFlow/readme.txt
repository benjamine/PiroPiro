
This assembly provides integration with TechTalk.SpecFlow
In order to access the Browser Automation API from step definitions you have 2 choices:

1- Derive your step definition classes from StepDefinitions class

This will give you this properties:

Browser: a browser associated to your current ScenarioContext (an instance is created on first access)
Page: used to query for elements on the current page, points to the root Browser Element, or to the context Element selected with .Within() functions

2- Add IStepDefinitions interface to you step definition classes

This way you can derive from other classes (maintaining your class hierarchy intact), but you have to access Browser & Page using extension methods:

this.Browser()
this.Page()
