using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Queues;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;

namespace SendToQueue
{
    public static class SendToQueue
    {
        [FunctionName("sendemail")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var configration = new ConfigurationBuilder()
                                        .SetBasePath(Directory.GetCurrentDirectory())
                                        .AddJsonFile("local.settings.json", true, true)
                                        .AddEnvironmentVariables()
                                        .Build();
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            string connectionString = configration["AzureWebJobsStorage"];
            string queueString = configration["AzureQueueName"];

            QueueClient queueClient = new QueueClient(connectionString, queueString, new
                                        QueueClientOptions()
                                        { MessageEncoding = QueueMessageEncoding.Base64 });

            queueClient.CreateIfNotExists();


            try
            {
                queueClient.SendMessage(requestBody);
                log.LogInformation("item sent to queue");
            }
            catch (Exception )
            {
                log.LogInformation("Oops, something went wrong!");
            }
            return new OkObjectResult("");
        }
    }
}
