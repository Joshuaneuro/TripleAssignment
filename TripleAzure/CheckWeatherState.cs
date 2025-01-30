using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace TripleAzure
{
    public class CheckWeatherState
    {
        private readonly ILogger<CheckWeatherState> _logger;

        private const string QueueName = "check-weather-job";
        private readonly string _storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        private const string QueueNameImage = "weather-job-images";
        public CheckWeatherState(ILogger<CheckWeatherState> logger)
        {
            _logger = logger;
        }

        [Function("CheckWeatherState")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            var queueClient = new QueueClient(_storageConnectionString, QueueName);

            var queueClientImage = new QueueClient(_storageConnectionString, QueueNameImage);

            QueueMessage[] messages = queueClient.ReceiveMessages(maxMessages: 1);

            string messageCheck = "Not-finished";

            // Get the first message
            QueueMessage message = messages[0];

            // Optionally delete the message after processing
            queueClient.DeleteMessage(message.MessageId, message.PopReceipt);

            if (message.MessageText == "Not-finished")
            {
                // restart
                queueClient.SendMessage(messageCheck);
                return new OkObjectResult(new
                {               
                    MessageText = message.MessageText                   
                });
            }
            if (message.MessageText == "finished")
            {
                QueueMessage[] Images = queueClientImage.ReceiveMessages(maxMessages: 32);
                queueClientImage.Delete();
                var messagesList = new List<string>();
                foreach (var Image in Images)
                {
                    // Add message content to the list
                    messagesList.Add(Image.MessageText);

                    // Delete the message from the queue after reading it               
                }
                return new OkObjectResult(new
                {
                    MessageText = messagesList
                });
            }        // return new OkObjectResult("Welcome to Azure Functions!" + x);
            else
            {
                return new OkObjectResult(new
                {
                    MessageText = "something went wrong",
                });
            }
        }
    }
}
