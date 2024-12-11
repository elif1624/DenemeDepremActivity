using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DepremVeriProjesi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EarthquakeController : ControllerBase
    {
        private static readonly HttpClient _client = new HttpClient();
        private static readonly string _apiUrl = "https://api.orhanaydogdu.com.tr/deprem/";

        [HttpGet("get-earthquake-data")]
        public async Task<IActionResult> GetEarthquakeData()
        {
            try
            {
                var response = await _client.GetStringAsync(_apiUrl);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(response, options);

                if (apiResponse?.Result != null)
                {
                    var popUpData = new Dictionary<string, List<EarthquakeSummary>>();

                    foreach (var quake in apiResponse.Result)
                    {
                        if (quake.GeoJson?.Coordinates?.Count >= 2)
                        {
                            var dateTimeParts = quake.Date?.Split(' ');
                            var summary = new EarthquakeSummary
                            {
                                Date = dateTimeParts?[0],
                                Time = dateTimeParts?[1],
                                Depth = quake.Depth ?? 0,
                                Magnitude = quake.Magnitude ?? 0
                            };

                            var cityName = quake.LocationProperties?.ClosestCity?.Name;
                            if (!string.IsNullOrEmpty(cityName))
                            {
                                if (!popUpData.ContainsKey(cityName))
                                    popUpData[cityName] = new List<EarthquakeSummary>();

                                popUpData[cityName].Add(summary);
                            }
                        }
                    }

                    return Ok(popUpData);
                }
                else
                {
                    return BadRequest("Veri işlenirken bir hata oluştu.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Hata oluştu: " + ex.Message);
            }
        }
    }

    public class ApiResponse
    {
        [JsonPropertyName("result")]
        public List<Earthquake>? Result { get; set; } // Nullable list
    }

    public class Earthquake
    {
        [JsonPropertyName("date")]
        public string? Date { get; set; } // Nullable string

        [JsonPropertyName("mag")]
        public double? Magnitude { get; set; } // Nullable double

        [JsonPropertyName("depth")]
        public double? Depth { get; set; } // Nullable double

        [JsonPropertyName("geojson")]
        public GeoJson? GeoJson { get; set; } // Nullable GeoJson

        [JsonPropertyName("location_properties")]
        public LocationProperties? LocationProperties { get; set; } // Nullable LocationProperties
    }

    public class GeoJson
    {
        [JsonPropertyName("coordinates")]
        public List<double>? Coordinates { get; set; } // Nullable list
    }

    public class LocationProperties
    {
        [JsonPropertyName("closestCity")]
        public ClosestCity? ClosestCity { get; set; } // Nullable ClosestCity
    }

    public class ClosestCity
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; } // Nullable string
    }

    public class EarthquakeSummary
    {
        public string? Date { get; set; } // Nullable string
        public string? Time { get; set; } // Nullable string
        public double Depth { get; set; } // Non-nullable
        public double Magnitude { get; set; } // Non-nullable
    }
}
