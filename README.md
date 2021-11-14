# FitNesseFitSharpRest ![example workflow](https://github.com/essenius/FitNesseFitSharpRest/actions/workflows/fitsharp-rest-ci.yml/badge.svg)
This repo contains a fixture to enable testing REST services with JSON, XML or Text payloads. It include FitNesse test pages as well as 
corresponding C# fixtures, and gives examples of use.

# Installation
The steps to install are very similar to that of installing the [FibonacciDemo](../../../FitNesseFitSharpFibonacciDemo). If you have done all that, you can skip the steps to install Java, FitNesse, NuGet CLI and FitSharp.

Differences are:
* Download the repo code as a zip file and extract the contents of the folder `FitNesseFitSharpRest-master` to `%LOCALAPPDATA%\FitNesse` 
* Go to solution folder: `cd /D %LOCALAPPDATA%\FitNesse\Rest`
* Build fixture solution: `dotnet build --configuration release Rest.sln`
* No need to publish as we are leveraging the RestTest folder
* Go to the fixture assembly folder: `cd RestTests\bin\Release\net5.0`
* Start FitNesse 
* Run the suite: Open a browser and enter the URL http://localhost:8080/FitSharpDemos.RestSuite?suite

# Tutorial and Reference
See the [Wiki](../../wiki)

# Contribute
Enter an issue or provide a pull request. 
