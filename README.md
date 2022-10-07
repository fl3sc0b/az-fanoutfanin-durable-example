# az-fanoutfanin-durable-example
A fan out/fan in Azure Durable function orchestration example. This example demonstrates the generation of multiple blob files concurrently into a blob storage account.
## How to set up the environment
### Requirements
- Visual Studio Code
- Azure Functions Extensions for Visual Studio Code installed
- Azure Functions Core Tools installed (tested with version 4.x)
- An Azure Blob Storage account to test 
### Create your Azure Functions project
1. Create a new folder for your project and open the folder in Visual Studio Code
2. From the Azure Blade, in the Workspace section click on Create Function
3. Previous step will trigger a dialog to allow you to create a new Azure Functions project. Click on YES
4. Select .NET 6.0 LTS as the framework to use (not tested in 7.0)
5. Select the template "Durable Functions Orchestration". This is a simple example of function chaining pattern
6. To go for our Fan out/Fan in example, replace the code by the one inside file **FanOutFanIn.cs**
7. Fill the placeholders inside the file **local.settings.json** with the data demanded from your Azure Storage Account and add it to the project

### How to test the project
