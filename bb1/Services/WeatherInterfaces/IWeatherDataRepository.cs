using bb1.Components.Models;

namespace bb1.Services.WeatherInterfaces
{
    public interface IWeatherDataRepository
    {
        IQueryable<WeatherRecordBase> GetRecordsQueryable(string tableReference);
        Task DeleteAllByCityAsync(string tableReference, string cityName);
        Task<bool> AnyByCityAndFutureDatesAsync(string tableReference, string cityName, DateTime today);
        Task<int> CountByCityAndFutureDatesAsync(string tableReference, string cityName, DateTime today);
    }
}
