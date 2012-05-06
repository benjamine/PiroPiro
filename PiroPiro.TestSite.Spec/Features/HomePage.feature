Feature: HomePage
	In order to note the site exists
	As a new user
	I want to see a home page on first visit

Scenario: WelcomeMessage
	When visit ""
	Then should read "Welcome"

Scenario: LinkToAboutPage
	When visit ""
	Then should see link "About"
