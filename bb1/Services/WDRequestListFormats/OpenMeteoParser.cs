using bb1.Components.Models;
using System.Text.Json;

namespace bb1.Services.WDRequestListFormats
{
    public class OpenMeteoParser : IWeatherDataParser
    {
        public WeatherDataCollection Parse(JsonElement root)
        {
            var weatherData = new WeatherDataCollection { List = new List<DailyForecast>() };

            var hourly = root.GetProperty("hourly");
            var temps = hourly.GetProperty("temperature_2m").EnumerateArray().ToArray();
            var winds = hourly.GetProperty("wind_speed_10m").EnumerateArray().ToArray();

            int count = Math.Min(temps.Length, winds.Length);
            int added = 0;

            // Начинаем с 6-го элемента (индекс 6) и берем каждые 6
            for (int i = 6; i < count && added < 20; i += 6)
            {
                weatherData.List.Add(new DailyForecast
                {
                    Temperature = temps[i].GetSingle(),
                    WindSpeed = winds[i].GetSingle()
                });

                added++;
            }

            return weatherData;
        }
    }
}