using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace TripleAzure;

public class Photo
{
    public string id { get; set; }
    public string Description { get; set; }
    public string urls { get; set; }
    public string Photographer { get; set; }
}

public class UnsplashService
{
    private const string ApiUrl = "https://api.unsplash.com/photos/random?count=5";
    private const string AccessKey = ""; // Replace with your Unsplash access key

    public static async Task<List<Photo>> GetRandomPhotosAsync()
    {
        using (HttpClient httpClient = new HttpClient())
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", AccessKey);

                HttpResponseMessage response = await httpClient.GetAsync(ApiUrl);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();

                var photos = JsonSerializer.Deserialize<List<JsonElement>>(json);
                var result = new List<Photo>();

                foreach (var photoElement in photos)
                {
                    result.Add(new Photo
                    {
                        id = photoElement.GetProperty("id").GetString(),
                        urls = photoElement.GetProperty("urls").GetProperty("full").GetString()                      
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching or parsing data: {ex.Message}");
                return new List<Photo>();
            }
        }
    }
}
