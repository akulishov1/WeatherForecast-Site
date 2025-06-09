using bb1.Components.Models;
using System.Text.Json;

namespace bb1.Services.WDRequestListFormats
{
    public class OpenWeatherMapParser : IWeatherDataParser
    {
        public WeatherDataCollection Parse(JsonElement root)
        {
            var weatherData = new WeatherDataCollection { List = new() };

            var forecastList = root.GetProperty("list").EnumerateArray();

            var allowedHours = new[] { 6, 12, 18, 0 };
            int count = 0;

            foreach (var item in forecastList)
            {
                var dtText = item.GetProperty("dt_txt").GetString();
                if (!DateTime.TryParse(dtText, out var dt))
                    continue;

                if (!allowedHours.Contains(dt.Hour))
                    continue;

                // Пропускаем 00:00 только если это первый пакет дня
                if (dt.Hour == 0 && weatherData.List.Count % 4 == 0)
                    continue;

                weatherData.List.Add(new DailyForecast
                {
                    Temperature = item.GetProperty("main").GetProperty("temp").GetSingle(),
                    WindSpeed = item.GetProperty("wind").GetProperty("speed").GetSingle()
                });

                count++;
                if (count == 20) // 5 дней * 4 записи (утро, день, вечер, ночь)
                    break;
            }

            return weatherData;
        }
    }
}