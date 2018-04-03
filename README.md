# EmailExtensions

There are two solutions in this repository:

## Refactored.Email
[![Refactored.Email](https://img.shields.io/nuget/v/Refactored.Email.svg)](https://www.nuget.org/packages/Refactored.Email/)
[![Refactored.Email](https://img.shields.io/nuget/dt/Refactored.Email.svg)](https://www.nuget.org/packages/Refactored.Email/)

A generic Email Library built around the .Net 4.5 System.Net.Mail set of classes.

SMTP Email Functionality includes Mail Merge and HTML/Plain text alternate views.

### Features

* Email Template merging with records from `Dictionary`, `Object`, or `NameValueCollection` objects.
* Record properties that are Enumerable will be rendered as a list of items.
* Parsing Templates configurable through App Settings or directly in code
* Email Attachments
* Optionally embed images in cid format (default is on)

## Refactored.UmbracoEmailExtensions
[![Refactored.Email](https://img.shields.io/nuget/v/Refactored.UmbracoEmailExtensions.svg)](https://www.nuget.org/packages/Refactored.UmbracoEmailExtensions/)
[![Refactored.Email](https://img.shields.io/nuget/dt/Refactored.UmbracoEmailExtensions.svg)](https://www.nuget.org/packages/Refactored.UmbracoEmailExtensions/)

An Umbraco Plugin that takes advantage of `Refactored.Email` to provide enhanced email capabilities for custom forms with the ability to utilise Umbraco Content Nodes as email templates with Mail Merge capabilities.
