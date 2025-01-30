using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TripleAzure
{
    public class StationMeasurement
    {
        public string stationname { get; set; }
        public double? temperature { get; set; }
        public double? humidity { get; set; }
        public double? windspeed { get; set; }
        // Add more properties as needed, matching the JSON structure.

        public Dictionary<string, string> ToStringProperties()
        {
            return new Dictionary<string, string>
            {
                { nameof(stationname), stationname ?? string.Empty },
                { nameof(temperature), temperature?.ToString() ?? string.Empty },
                { nameof(humidity), humidity?.ToString() ?? string.Empty },
                { nameof(windspeed), windspeed?.ToString() ?? string.Empty }
            };
        }
    }

    public class BuienradarService
    {
        private const string ApiUrl = "https://data.buienradar.nl/2.0/feed/json";

        public static async Task<List<StationMeasurement>> GetTopStationMeasurementsAsync(int topCount = 5)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(ApiUrl);
                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();

                    using (JsonDocument document = JsonDocument.Parse(json))
                    {
                        var root = document.RootElement;
                        var actualElement = root.GetProperty("actual");
                        var stationMeasurementsElement = actualElement.GetProperty("stationmeasurements");

                        var measurements = JsonSerializer.Deserialize<List<StationMeasurement>>(stationMeasurementsElement.GetRawText());
                        var newmeasure = measurements.Select(m => m.ToStringProperties()).ToList();
                        var newmeasure2 = new List<Dictionary<string, string>>();
                        Console.Write(newmeasure2);
                        return measurements?.Take(topCount).ToList() ?? new List<StationMeasurement>();
                    }            
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching or parsing data: {ex.Message}");
                    return new List<StationMeasurement>();
                }
            }
        }
    }
}