using System.Text.Json;
using bb1.Components.Models;

namespace bb1.Services.WDRequestListFormats
{
    public interface IWeatherDataParser
    {
        WeatherDataCollection Parse(JsonElement root);
    }
}
