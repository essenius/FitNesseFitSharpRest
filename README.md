# FitNesseFitSharpRest
This repo contains a fixture to enable testing REST services with JSON, XML or Text payloads

# Getting Started
1. Download FitNesse (http://fitnesse.org) and install it to <I>C:\Apps\FitNesse</I>
2. Download FitSharp (https://github.com/jediwhale/fitsharp) and install it to <I>C:\Apps\FitNesse\FitSharp</I>.
   <br />Or install the package via nuget: 
   ```
   nuget install FitSharp
   ```
4. Clone the repo to a local folder (e.g. <I>C:\Data\FitNesseDemo</I>)
5. Update <I>plugins.properties</I> to point to the FitSharp folder (if you took other folders than suggested)
6. Build all projects in the solution Rest
7. Ensure you have Java installed (1.7 or higher)
8. Start FitNesse with the root repo folder as the data folder, and the assembly folder as the current directory:

	```
	cd /D C:\Data\FitNesseDemo\FitNesseFitSharpRest\Rest\Rest\bin\Debug\net5.0

	java -jar C:\Apps\FitNesse\fitnesse-standalone.jar -d C:\Data\FitNesseDemo\FitNesseFitSharpRest
	```
    
8. Open a browser and enter the URL http://localhost:8080/FitSharpDemos.RestSuite?suite

# Contribute
Enter an issue or provide a pull request.
