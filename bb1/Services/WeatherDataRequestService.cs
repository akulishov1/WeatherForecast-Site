using bb1.Components.Models;
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
                    var response = await _httpClient.GetStringAsync("https://api.openweathermap.org/data/2.5/forecast?lat=49.4447888&lon=32.0587805&units=metric&appid=5bf61eb32d958014152c8c08e4a3ad33");

                    var jsonDocument = JsonDocument.Parse(response);
                    var root = jsonDocument.RootElement;

                    var weatherData = new WeatherData { List = new List<DailyForecast>() };

                    int counter = 0;
                    foreach (var item in root.GetProperty("list").EnumerateArray())
                    {
                        // Now use odd-numbered items: indices 1, 3, 5, …
                        if (counter % 2 == 1)
                        {
                            var forecast = new DailyForecast
                            {
                                Temperature = item.GetProperty("main").GetProperty("temp").GetSingle(),
                                WindSpeed = item.GetProperty("wind").GetProperty("speed").GetSingle()
                            };

                            weatherData.List.Add(forecast);
                        }
                        counter++;
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
