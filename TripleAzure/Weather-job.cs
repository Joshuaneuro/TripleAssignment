using System;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;

namespace TripleAzure
{
    public class Weather_job
    {
        private readonly ILogger _logger;

        private const string QueueNameCheck = "check-weather-job";
        private const string QueueNameImage = "weather-job-images";
        private readonly string _storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        public Weather_job(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Weather_job>();
        }

        [Function("Weatherjob")]
        public async Task Run(
            [QueueTrigger("start-weather-job", Connection = "AzureWebJobsStorage")] string queueMessage)
        {
            try
            {
                var queueClientCheck = new QueueClient(_storageConnectionString, QueueNameCheck);
                var queueClientImage = new QueueClient(_storageConnectionString, QueueNameImage);
                _logger.LogInformation($"Received message: {queueMessage}");

                // create queue for checkState
                queueClientCheck.CreateIfNotExists();
                queueClientImage.CreateIfNotExists();
                string message = "Not-finished";
                // Send the message
                queueClientCheck.SendMessage(message);

                //start image job       
                List<StationMeasurement> measurements = await BuienradarService.GetTopStationMeasurementsAsync();

                foreach (var measurement in measurements)
                {
                    Console.WriteLine($"Station: {measurement}");
                }

                List<Photo> photos = await UnsplashService.GetRandomPhotosAsync();
                var i = 0;
                foreach (var photo in photos)
                {
                    string containerName = "container-weather-images"; // Replace with your container name
                    string imageUrl = photo.urls;
                    string blobName = $"" + photo.id + ".jpg";
                    string apiName = measurements[i].stationname;
                    string apiTemp = measurements[i].temperature.ToString();
                    string apiWind = measurements[i].windspeed.ToString();
                    string apiHumid = measurements[i].humidity.ToString();

                    ImageUploader uploader = new ImageUploader(_storageConnectionString, containerName);
                    var x = await uploader.UploadImageFromUrlAsync(imageUrl, blobName, apiName, apiTemp,apiWind,apiHumid);
                    queueClientImage.SendMessage(x);
                    Console.WriteLine(x);
                    i++;
                }
                // finished job
                queueClientCheck.ClearMessages();
                queueClientCheck.SendMessage("finished");
                _logger.LogInformation($"Jobs done");



            }
            catch (Exception ex)
            {
                // Log an error if message processing fails
                _logger.LogError($"Failed to process the message: {ex.Message}");

                // Throwing an exception here will leave the message in the queue for retry
                throw;
            }
        }
    }
}
