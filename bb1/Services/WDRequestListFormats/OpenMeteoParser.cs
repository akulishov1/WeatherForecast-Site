using bb1.Components.Models;
using System.Text.Json;

namespace bb1.Services.WDRequestListFormats
{
    public class OpenMeteoParser : IWeatherDataParser
    {
        public WeatherDataCollection Parse(JsonElement root)
        {
            var weatherData = new WeatherDataCollection { List = new() };

            var hourly = root.GetProperty("hourly");
            var temps = hourly.GetProperty("temperature_2m").EnumerateArray();
            var winds = hourly.GetProperty("wind_speed_10m").EnumerateArray();

            using var tempEnumerator = temps.GetEnumerator();
            using var windEnumerator = winds.GetEnumerator();

            while (tempEnumerator.MoveNext() && windEnumerator.MoveNext())
            {
                weatherData.List.Add(new DailyForecast
                {
                    Temperature = tempEnumerator.Current.GetSingle(),
                    WindSpeed = windEnumerator.Current.GetSingle()
                });
            }

            return weatherData;
        }
    }
}
