# az-fanoutfanin-durable-example
A fan out/fan in Azure Durable function orchestration example. This example demonstrates the generation of multiple blob files concurrently into a blob storage account.

## What are Azure Durable Functions ?
Azure Durable Functions are an extension of basic Azure serveless resource named "Azure Functions". Azure Durable Functions adds context to your serverless experience and even enables you to orchestrate your own pipeline of Azure Functions to implement complex and efficient architectural patterns in a serverless way. More info in the official Microsoft docs: (https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview?tabs=csharp)

## What is the Fan out/Fan in pattern ?
The fan out/fan in pattern is a very popular and efficient pattern that slices data into small pieces and feeds each into computational units that process its own part. At the end, a final aggregation might be executed to get the desired output.

![image](https://user-images.githubusercontent.com/38964606/194766671-0b6585f8-a1d3-4b78-87d2-42fbbce9f224.png)

More info in the official Microsoft docs: (https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-cloud-backup?tabs=csharp)

## How to set up the environment
### Requirements
- Visual Studio Code
- Azure Functions Extensions for Visual Studio Code installed
- Azure Functions Core Tools installed (tested with version 4.x)
- An Azure Blob Storage account to demonstrate the generation of some files 
### Create your Azure Functions project
1. Create a new folder for your project and open the folder in Visual Studio Code
2. From the Azure Blade, in the Workspace section click on Create Function
3. Previous step will trigger a dialog to allow you to create a new Azure Functions project. Click on YES
4. Select .NET 6.0 LTS as the framework to use (not tested in 7.0)
5. Select the template "Durable Functions Orchestration". This is a simple example of function chaining pattern
6. To go for our Fan out/Fan in example, replace the code by the one inside file **FanOutFanIn.cs**
7. Fill the placeholders inside the file **local.settings.json** with the data demanded from your Azure Storage Account and add it to the project. Needed data will include:
- Blob storage account name
- Blob storage account key
- Blob container name you want use
### How to test the project
This is a very simple project. We will set up an endpoint that would be awaiting for a set of interval of numbers in the form: a-b. For each of these intervals, our backend routine will dump the whole list of numbers between the intervals into a blob file. For example:
POST call to https://<endpoint_name>:port?input=1-10,1-100,1-1000 would generate three files:
- "1-10.txt" => From 1 to 10
- "1-100.txt" => From 1 to 100
- "1-1000.txt" => From 1 to 1000
You can debug locally or deploy to an online Azure function app. Main difference is that debugging locally would get the settings from our **local.settings.json** file whereas deploying to function app would get the values from the function app setting slots.
**NOTE:**Durable functions will require context, so a storage account is mandatory. The **AzureWebJobsStorage** setting allows you to specify a Connection String for using a particular Storage Account. In local debugging you could use Microsoft Azure Storage Emulator alternatively.
