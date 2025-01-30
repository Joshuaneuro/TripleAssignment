using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace TripleAzure
{
    public class ImageUploader
    {
        private readonly string _blobConnectionString;
        private readonly string _containerName;

        public ImageUploader(string blobConnectionString, string containerName)
        {
            _blobConnectionString = blobConnectionString;
            _containerName = containerName;
        }

        public async Task<string> UploadImageFromUrlAsync(string imageUrl, string blobName, string apiName, string apiTemp, string apiWind, string apiHumid)
        {
            // Get the image stream from the URL
            using (HttpClient httpClient = new HttpClient())
            using (HttpResponseMessage response = await httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead))

            {
                response.EnsureSuccessStatusCode();               

                using (Stream imageStream = await response.Content.ReadAsStreamAsync())
                {
                    // Upload the stream to Azure Blob Storage
                    BlobServiceClient blobServiceClient = new BlobServiceClient(_blobConnectionString);

                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                    var renderedImage = ImageHelper.AddTextToImage(imageStream,
                          (apiName, (10, 10), 320, "ffffff"),
                          (apiTemp, (10, 44), 240, "000000"),
                          (apiTemp, (10, 100), 320, "ffffff"),
                          (apiHumid, (10, 210), 320, "ffffff")
                          );
                    // Ensure the container exists
                    await containerClient.CreateIfNotExistsAsync();

                    BlobClient blobClient = containerClient.GetBlobClient(blobName);

                    await blobClient.UploadAsync(renderedImage, overwrite: true);

                    Console.WriteLine($"Image uploaded to blob: {blobClient.Uri}");

                    return blobClient.Uri.ToString();
                }
            }
        }
    }
}
