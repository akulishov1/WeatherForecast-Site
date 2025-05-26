using bb1.Components.Models;
using System.Text.Json;

namespace bb1.Services.WDRequestListFormats
{
    public class OpenWeatherMapParser : IWeatherDataParser
    {
        public WeatherDataCollection Parse(JsonElement root)
        {
            var weatherData = new WeatherDataCollection { List = new() };
            int counter = 0;
            foreach (var item in root.GetProperty("list").EnumerateArray())
            {
                if (counter % 2 == 1)
                {
                    weatherData.List.Add(new DailyForecast
                    {
                        Temperature = item.GetProperty("main").GetProperty("temp").GetSingle(),
                        WindSpeed = item.GetProperty("wind").GetProperty("speed").GetSingle()
                    });
                }
                counter++;
            }
            return weatherData;
        }
    }
}
