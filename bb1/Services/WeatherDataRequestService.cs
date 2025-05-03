using bb1.Components.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace bb1.Services
{
    public class WeatherDataRequestService
    {
        public class WeatherData
        {
            public List<DailyForecast> List { get; set; }
        }

        public class WeatherService
        {
            private readonly HttpClient _httpClient;

            public WeatherService(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }


            public async Task<WeatherData> FetchWeatherDataAsync()
            {
                try
                {
                    var response = await _httpClient.GetStringAsync("https://api.openweathermap.org/data/2.5/forecast?lat=49.4447888&lon=32.0587805&appid=5bf61eb32d958014152c8c08e4a3ad33");

                    var jsonDocument = JsonDocument.Parse(response);
                    var root = jsonDocument.RootElement;

                    var weatherData = new WeatherData { List = new List<DailyForecast>() };

                    int EverySecondList = 0;

                    foreach (var item in root.GetProperty("list").EnumerateArray())
                    {
                        if (EverySecondList % 2 == 0) // Only add items with even index (every second item)
                        {
                            var forecast = new DailyForecast
                            {
                                Temperature = item.GetProperty("main").GetProperty("temp").GetSingle(),
                                TemperatureFeels = item.GetProperty("main").GetProperty("feels_like").GetSingle(),
                                MinTemperature = item.GetProperty("main").GetProperty("temp_min").GetSingle(),
                                MaxTemperature = item.GetProperty("main").GetProperty("temp_max").GetSingle(),
                                WindSpeed = item.GetProperty("wind").GetProperty("speed").GetSingle()
                            };

                            weatherData.List.Add(forecast);
                        }

                        EverySecondList++;
                    }

                    return weatherData;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching weather data: {ex.Message}");
                    return new WeatherData { List = new List<DailyForecast>() };
                }
            }
        }
    }
}
