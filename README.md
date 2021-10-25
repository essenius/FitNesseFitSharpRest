# FitNesseFitSharpRest ![example workflow](https://github.com/essenius/FitNesseFitSharpRest/actions/workflows/fitsharp-rest-ci.yml/badge.svg)
This repo contains a fixture to enable testing REST services with JSON, XML or Text payloads. It include FitNesse test pages as well as 
corresponding C# fixtures, and gives examples of use.

# Installation
The steps to install are very similar to that of installing the [FibonacciDemo](../../../FitNesseFitSharpFibonacciDemo).

Differences are:
* Download the repo code as a zip file and extract the contents of the folder ```FitNesseFitSharpRest```. 
* Build command becomes: ```dotnet build %LOCALAPPDATA%\FitNesse\Rest\Rest.sln```
* Go to folder: ```cd /D %LOCALAPPDATA%\FitNesse\Rest\RestTests\bin\debug\net5.0```
* Run the suite: Open a browser and enter the URL http://localhost:8080/FitSharpDemos.RestSuite?suite

# Tutorial
See the [Wiki](../../wiki)

# Contribute
Enter an issue or provide a pull request. 
