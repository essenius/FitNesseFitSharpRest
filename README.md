# FitNesseFitSharpRest
This repo contains a fixture to enable testing REST services with JSON, XML or Text payloads

# Getting Started
## Windows 
1. Download FitNesse (http://fitnesse.org) and install it to <I>C:\Apps\FitNesse</I>
2. Download FitSharp (https://github.com/jediwhale/fitsharp) and install it to <I>C:\Apps\FitNesse\FitSharp</I>.
   <br />Or install the package via nuget with target output directory specified:
   ```
   nuget install FitSharp -OutputDirectory C:\Apps\FitNesse
   ```
   if you prefer not having version number as part of package folder name, use <I>-ExcludeVersion</I> switch to exclude the version number.
   
   ```
   nuget install FitSharp -OutputDirectory C:\Apps\FitNesse -ExcludeVersion
   ```
   After executing above command, FitSharp will be installed to <I>C:\Apps\FitNesse\FitSharp</I>.
   
3. Clone the repo to a local folder (e.g. <I>C:\Data\FitNesseDemo</I>)
4. Update <I>plugins.properties</I> to point to the FitSharp folder (if you took other folders than suggested)
5. Build all projects in the solution Rest in Visual Studio Code:
   ```
   dotnet build Rest.sln
   ```
6. Ensure you have Java installed (1.7 or higher)
7. Start FitNesse with the root repo folder as the data folder, and the assembly folder as the current directory:

	```
	cd /D C:\Data\FitNesseDemo\FitNesseFitSharpRest\Rest\Rest\bin\Debug\net5.0

	java -jar C:\Apps\FitNesse\fitnesse-standalone.jar -d C:\Data\FitNesseDemo\FitNesseFitSharpRest
	```
    
8. Open a browser and enter the URL http://localhost:8080/FitSharpDemos.RestSuite?suite

## Mac
1. Download FitNesse (http://fitnesse.org) and install it to a target folder, e.g. <I>/Users/[username]/Downloads/FitNesse</I>
2. Install NuGet via following command:
	```
	brew install nuget
	```
3. Install FitSharp (https://github.com/jediwhale/fitsharp) package via nuget with target output directory specified:
   ```
   nuget install FitSharp -OutputDirectory /Users/[username]/Downloads/FitNesse
   ```
   if you prefer not having version number as part of package folder name, use <I>-ExcludeVersion</I> switch to exclude the version number.
   
   ```
   nuget install FitSharp -OutputDirectory /Users/[username]/Downloads/FitNesse -ExcludeVersion
   ```
   After executing above command, FitSharp will be installed to <I>/Users/[username]/Downloads/FitNesse/FitSharp</I>.
   
3. Clone the repo to a local folder (e.g. <I>/Users/[username]/Downloads/FitNesseDemo</I>)
4. Update <I>plugins.properties</I> to point to the FitSharp folder (if you took other folders than suggested). Mac users, please check the comments in the <I>plugins.properties</I> for reference.
5. Build all projects in the solution Rest in Visual Studio Code:
   ```
   dotnet build Rest.sln
   ```
7. Ensure you have Java installed (1.7 or higher)
8. Start FitNesse with the root repo folder as the data folder, and the assembly folder as the current directory:

	```
	cd /Users/[username]/Downloads/FitNesseDemo/FitNesseFitSharpRest/Rest/Rest/bin/Debug/net5.0

	java -jar /Users/[username]/Downloads/FitNesse/fitnesse-standalone.jar -d /Users/[username]/Downloads/FitNesseDemo/FitNesseFitSharpRest
	```
    
8. Open a browser and enter the URL http://localhost:8080/FitSharpDemos.RestSuite?suite
# Contribute
Enter an issue or provide a pull request.
