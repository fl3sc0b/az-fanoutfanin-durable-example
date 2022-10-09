using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;

using System.Text;

namespace com.fl3sc0b.durable
{
    public static class DurableExample
    {
        /// <summary>
        /// Simple class to wrap a string
        /// </summary>
        public class StringWrapper
        {
            /// <summary>
            /// Property to store a string inside
            /// </summary>
            /// <value></value>
            public string value {get;set;}
        }

        /// <summary>
        /// Durable orchestrator function   
        /// </summary>
        /// <param name="context">Context object to orchestrate the function calls and to get the parameters from the caller</param>
        /// <param name="log">Logger object to register events</param>
        /// <returns>Nothing</returns>
        [FunctionName("DurableExample_Orchestrator")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {   
           string input = context.GetInput<StringWrapper>().value; // Unwrap the parameter
           log.LogInformation($"parameter received: {input}"); // Log the received parameter

           string[] files = input.Split(','); // Example: "1-10,1-100,1-1000" => Three files:
                                              //    * "1-10.txt" => From 1 to 10
                                              //    * "1-100.txt" => From 1 to 100
                                              //    * "1-1000.txt" => From 1 to 1000

           var tasks = new Task<string>[files.Length]; // This array will store the list of activities to be executed concurrently
        
           for (int i = 0; i < files.Length; i++)
           {
                tasks[i] = context.CallActivityAsync<string>("DurableExample_Activity", files[i]); // Load each of the activities with its slice of data
           }

           await Task.WhenAll(tasks); // Asynchronously run all the activities concurrently
        }

        /// <summary>
        /// This function will be called concurrently with its slice of data. As it's being invoked from the orchestrator, technically is named an "Activity" 
        /// </summary>
        /// <param name="fileName">Slice of data provided. In this case, an interval of numbers "a-b"</param>
        /// <param name="log">Logger object to register events</param>
        /// <returns>The string "ok", if the execution finishes properly</returns>
        [FunctionName("DurableExample_Activity")]
        public static async Task<string> GenerateFile([ActivityTrigger] string fileName, ILogger log)
        {
            // Use this method to get app settings from function app settings (online) or local.settings.json (local debugging)
            string account_name = System.Environment.GetEnvironmentVariable("StorageName");
            
            // Get storage account credentials (key1) and the name of the blob container
            StorageCredentials creds = new StorageCredentials(account_name,
                                                              System.Environment.GetEnvironmentVariable("StorageKey"), "key1");
            CloudBlobContainer cbc = new CloudBlobContainer(new System.Uri($"https://{account_name}.blob.core.windows.net/durable"), creds);

            // Determine the interval of numbers
            string[] numbers = fileName.Split('-');
            int start = int.Parse(numbers[0]);
            int end = int.Parse(numbers[1]);

            // Generate the file content
            StringBuilder sb = new StringBuilder();
            for (int i = start; i <= end; i++)
            {
                sb.AppendLine(i.ToString());
            }

            // Generate the blob file and upload the file content to it
            var blockBlob = cbc.GetBlockBlobReference(fileName + ".txt");
            await blockBlob.UploadTextAsync(sb.ToString());

            log.LogInformation($"File {fileName} generated.");
            return "ok";
        }

        /// <summary>
        /// Entry point for our FanOut/FanIn architecture. It is triggered using an http request against the endpoint 
        /// </summary>
        /// <param name="req">Captured content of the request</param>
        /// <param name="starter">Client object to call our durable orchestrator</param>
        /// <param name="log">Logger object to register events</param>
        /// <returns>Response depending on the orchestrator status</returns>
        [FunctionName("DurableExample_HttpStart")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string input = req.Query["input"]; // Use of ASP.NET routing model to access the parameters from the request
            input ??= "1-10"; // If the input parameter comes empty, select a default value 
            StringWrapper input_parameter = new StringWrapper() {value = input}; // Wrap the parameter to send it to the orchestrator

            // Asynchronous call to the orchestrator (including our wrapped parameter)
            string instanceId = await starter.StartNewAsync<StringWrapper>("DurableExample_Orchestrator", input_parameter);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            // Generate a http response based on our orchestrator status
            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}