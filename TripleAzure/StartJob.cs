using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using AuthorizationLevel = Microsoft.Azure.Functions.Worker.AuthorizationLevel;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text;

//  WARNING : I AM NOT A SOFTWARE ENGINEER. I MAINLY HAVE DONE DATA RELATED PYTHON STUFF BEFORE COMING TO THIS MINOR.
// I HAVE NOT TOUCHED C# IN 10 YEARS 
// THERE WILL BE A LOT OF PROGRAMMING MISTAKES AND BAD PRACTICES
// I JUST WANTED IT TO WORK (AND IT DOES!!!!)

namespace TripleAzure
{
    public class StartJob
    {
        private readonly ILogger _logger;
        private const string QueueName = "start-weather-job";
        private readonly string _storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        public StartJob(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<StartJob>();
        }

        [Function("StartJob")]
        public async Task<HttpResponseData> Run(
            [Microsoft.Azure.Functions.Worker.HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("Processing request to send a message to Azure Queue Storage.");

            try
            {
                // Create the QueueClient
                var queueClient = new QueueClient(_storageConnectionString, QueueName);

                // Ensure the queue exists
                await queueClient.CreateIfNotExistsAsync();

                // Prepare the message
                string message = "Start Weather-Job";
                var bytes = Encoding.UTF8.GetBytes(message);

                // Send the message
                queueClient.SendMessage(Convert.ToBase64String(bytes));

                // Return a response indicating success
                var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await response.WriteStringAsync("Api/CheckWeatherState");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send message: {ex.Message}");

                var response = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("Failed to send message to the queue.");
                return response;
            }
        }
    }

}

