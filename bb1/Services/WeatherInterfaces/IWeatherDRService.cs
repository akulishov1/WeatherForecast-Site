using bb1.Components.Models;

namespace bb1.Services.WeatherInterfaces
{
    public interface IWeatherDRService
    {
        Task<WeatherDataCollection> FetchWeatherDataAsync(string siteKey, float latitude, float longitude);
    }
}
