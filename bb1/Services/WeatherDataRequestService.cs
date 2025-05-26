using bb1.Components.Models;
using bb1.Services.WDRequestListFormats;
using System.Text.Json;

namespace bb1.Services
{
    public class WeatherDataRequestService
    {
        public interface IWeatherService
        {
            Task<WeatherDataCollection> FetchWeatherDataAsync(int siteId);
        }
        public class WeatherService : IWeatherService
        {
            private readonly HttpClient _httpClient;
            private readonly IConfiguration _configuration;

            public WeatherService(HttpClient httpClient, IConfiguration configuration)
            {
                _httpClient = httpClient;
                _configuration = configuration;
            }

            public async Task<WeatherDataCollection> FetchWeatherDataAsync(int siteId)
            {
                try
                {
                    string apiUrl;
                    IWeatherDataParser parser;

                    switch (siteId)
                    {
                        case 1:
                            string apiKey = _configuration["WeatherSettings:ApiKey"];
                            apiUrl = $"https://api.openweathermap.org/data/2.5/forecast?lat=49.44&lon=32.05&units=metric&appid={apiKey}";
                            parser = new OpenWeatherMapParser();
                            break;

                        case 2:
                            apiUrl = "https://api.open-meteo.com/v1/forecast?latitude=49.44&longitude=32.05&hourly=temperature_2m,wind_speed_10m";
                            parser = new OpenMeteoParser();
                            break;

                        default:
                            throw new ArgumentException("Unknown site ID");
                    }

                    var response = await _httpClient.GetStringAsync(apiUrl);
                    var json = JsonDocument.Parse(response);
                    return parser.Parse(json.RootElement);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching weather data: {ex.Message}");
                    return new WeatherDataCollection();
                }
            }
        }
    }
}
