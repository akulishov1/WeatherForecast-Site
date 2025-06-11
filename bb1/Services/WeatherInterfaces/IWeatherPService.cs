using bb1.Components.Models;

namespace bb1.Services.WeatherInterfaces
{
    public interface IWeatherDataProcessor
    {
        List<T> MapToRecordsSimple<T>(WeatherDataCollection weatherData, Func<T> recordFactory, string cityName) where T : WeatherRecordBase;
    }
}
