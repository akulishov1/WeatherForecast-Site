using bb1.Components.Models;
using Microsoft.EntityFrameworkCore;

namespace bb1.Services
{
    public class WeatherRecordsService
    {
        private readonly IDbContextFactory<WeatherDbContext> _contextFactory;

        public WeatherRecordsService(IDbContextFactory<WeatherDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<WeatherRecordBase?> GetRecordByIdAndSiteIDAsync(int siteId, int recordId)
        {
            await using var context = _contextFactory.CreateDbContext();

            return siteId switch
            {
                1 => await context.OpenWeatherRecords.FindAsync(recordId),
                2 => await context.OpenMeteoRecords.FindAsync(recordId),
                _ => throw new ArgumentException("Unknown site ID")
            };
        }
        public TimeSpan GetServerTime() => DateTime.Now.TimeOfDay;
    }
}
