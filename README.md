# FitNesseFitSharpRest ![workflow badge](https://github.com/essenius/FitNesseFitSharpRest/actions/workflows/fitsharp-rest-ci.yml/badge.svg)
This repo contains a fixture to enable testing REST services with JSON, XML or Text payloads. It include FitNesse test pages as well as 
corresponding C# fixtures, and gives examples of use.

# Installation
The steps to install are very similar to that of installing the [FibonacciDemo](../../../FitNesseFitSharpFibonacciDemo). If you have done all that, you can skip the steps to install Java, FitNesse, NuGet CLI and FitSharp.

Differences are:
* Download the repo code as a zip file and extract the contents of the folder `FitNesseFitSharpRest-master` to `%LOCALAPPDATA%\FitNesse` 
* Go to solution folder: `cd /D %LOCALAPPDATA%\FitNesse\Rest`
* If using .NET SDK, build fixture solution: `dotnet build --configuration release Rest.sln`
* If not using .NET SDK, download `RestTests.zip` from the latest [Release](../../releases) and extract into the `RestTests` folder
* Go to the fixture assembly folder: `cd RestTests\bin\Release\net6.0`
* Start FitNesse 
* Run the suite: Open a browser and enter the URL http://localhost:8080/FitSharpDemos.RestSuite?suite

Note: if you don't need the test assemblies and just want to use the Rest fixture, you can build the Rest project instead, or take Rest.zip from the release.

# Tutorial and Reference
See the [Wiki](../../wiki)

# Contribute
Enter an [issue](../../issues) or provide a [pull request](../../pulls). 
